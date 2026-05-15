using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.ViewModels.Users;
using ContratosYReembolsos.Services.Interfaces;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IUserService userService, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index() => View(await _userService.GetAllUsersAsync());

    [HttpGet]
    public async Task<IActionResult> GetUserForm(string? id)
    {
        var model = string.IsNullOrEmpty(id)
            ? new RegisterUserViewModel()
            : await _userService.GetUserForEditingAsync(id);

        if (model == null) return NotFound();

        // Las filiales se cargan para el dropdown
        model.Branches = await _context.Filiales
            .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
            .ToListAsync();

        return PartialView("Partials/_UserForm", model);
    }

    // Solo necesitamos este método para guardar ambos casos (Create/Edit)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUser(RegisterUserViewModel model)
    {
        // 1. Validación de servidor
        if (!ModelState.IsValid)
        {
            await LoadBranchesToModel(model);
            return PartialView("Partials/_UserForm", model);
        }

        IdentityResult result;

        // 2. Lógica de negocio via Service
        if (string.IsNullOrEmpty(model.Id))
            result = await _userService.CreateUserAsync(model);
        else
            result = await _userService.UpdateUserAsync(model);

        // 3. Respuesta para AJAX
        if (result.Succeeded) return Ok();

        // 4. Manejo de errores de Identity (DNI duplicado, clave débil, etc.)
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        await LoadBranchesToModel(model);
        return PartialView("Partials/_UserForm", model);
    }

    // Método privado para evitar repetir código de carga de sedes
    private async Task LoadBranchesToModel(RegisterUserViewModel model)
    {
        model.Branches = await _context.Filiales
            .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> ManagePermissions(string userId)
    {
        var model = await _userService.GetUserForPermissionsAsync(userId);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "Permissions.Usuarios.Permisos")]
    public async Task<IActionResult> ManagePermissions(ManageUserPermissionsViewModel model)
    {
        var success = await _userService.UpdateUserPermissionsAsync(model);

        if (success)
        {
            if (model.UserId == _userManager.GetUserId(User))
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null) await _signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Permisos actualizados correctamente para " + model.FullName;

            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "No se pudieron actualizar los permisos.");
        return View(model);
    }
}
