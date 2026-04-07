using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ContratosYReembolsos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // LISTADO: Usa UserListViewModel
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

        // REGISTRO (GET): Prepara el combo de filiales
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

        // REGISTRO (POST): Procesa y guarda
        [HttpPost]
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
                    BranchId = model.BranchId // AMARRE FÍSICO EN BD
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Operador");
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            // Si falla, recargar combo
            model.Branches = _context.Filiales.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name });
            return View(model);
        }
    }
}