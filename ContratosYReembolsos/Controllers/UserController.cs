using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager; // FALTABA ESTO
    private readonly ApplicationDbContext _context;

    // Actualizamos el constructor para recibir los 3 servicios
    public UserController(UserManager<ApplicationUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager; // FALTABA ESTO
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.Include(u => u.Branch).ToListAsync();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserListViewModel
            {
                Id = user.Id,
                DNI = user.DNI,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Operador",
                BranchName = user.Branch?.Name ?? "Sede Central (Global)"
            });
        }
        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new RegisterUserViewModel
        {
            Branches = await _context.Filiales.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            }).ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.DNI,
                DNI = model.DNI,
                FullName = model.FullName,
                Email = model.Email,
                BranchId = model.BranchId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ahora _roleManager sí funcionará
                if (!await _roleManager.RoleExistsAsync("Operador"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Operador"));
                }

                await _userManager.AddToRoleAsync(user, "Operador");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // Recarga de filiales si falla
        model.Branches = await _context.Filiales
            .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
            .ToListAsync();

        return View(model);
    }
}