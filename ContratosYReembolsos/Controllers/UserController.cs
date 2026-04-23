using ContratosYReembolsos.Models.ViewModels;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContratosYReembolsos.Constants;
using System.Linq;
using ContratosYReembolsos.Data.Contexts;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _signInManager = signInManager;
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

    [HttpGet]
    public async Task<IActionResult> ManagePermissions(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            TempData["InfoMessage"] = "Los administradores tienen acceso total por defecto y sus permisos no pueden ser modificados.";
            return RedirectToAction(nameof(Index));
        }

        var existingClaims = await _userManager.GetClaimsAsync(user);

        var model = new ManageUserPermissionsViewModel
        {
            UserId = userId,
            FullName = user.FullName,
            DNI = user.DNI,
            Claims = new List<UserClaimViewModel>
            {
                new UserClaimViewModel { Value = Permissions.Ataudes.Ver, DisplayName = "Ver Inventario", Group = "Ataúdes" },
                new UserClaimViewModel { Value = Permissions.Ataudes.Catalogo, DisplayName = "Gestionar Modelos/Catálogo", Group = "Ataúdes" },
                new UserClaimViewModel { Value = Permissions.Ataudes.Traslados, DisplayName = "Realizar Traslados entre Sedes", Group = "Ataúdes" },
                new UserClaimViewModel { Value = Permissions.Ataudes.Ingresos, DisplayName = "Registrar Ingresos de Stock", Group = "Ataúdes" },

                new UserClaimViewModel { Value = Permissions.Contratos.Ver, DisplayName = "Ver Contratos", Group = "Contratos" },
                new UserClaimViewModel { Value = Permissions.Contratos.Crear, DisplayName = "Crear Contratos", Group = "Contratos" },
                new UserClaimViewModel { Value = Permissions.Contratos.Editar, DisplayName = "Editar Contratos", Group = "Contratos" },
                new UserClaimViewModel { Value = Permissions.Contratos.Eliminar, DisplayName = "Eliminar Contratos", Group = "Contratos" },

                new UserClaimViewModel { Value = Permissions.Movilidad.Ver, DisplayName = "Ver Movilidad", Group = "Ataúdes" },
                new UserClaimViewModel { Value = Permissions.Movilidad.Vehiculos, DisplayName = "Ver y Crear Vehiculos", Group = "Movilidad" },
                new UserClaimViewModel { Value = Permissions.Movilidad.Conductores, DisplayName = "Ver y Crear Conductores", Group = "Movilidad" },
                new UserClaimViewModel { Value = Permissions.Movilidad.Gestion, DisplayName = "Asignar Vehiculos y Conductores a Contratos", Group = "Movilidad" },

                new UserClaimViewModel { Value = Permissions.Sepulturas.Ver, DisplayName = "Ver Sepulturas", Group = "Sepulturas" },
                new UserClaimViewModel { Value = Permissions.Sepulturas.Asignar, DisplayName = "Asignar", Group = "Sepulturas" },
                new UserClaimViewModel { Value = Permissions.Sepulturas.Modelos, DisplayName = "Crear Modelos de Columbarios/Pabellones", Group = "Sepulturas" },

                new UserClaimViewModel { Value = Permissions.Filiales.Ver, DisplayName = "Ver Filiales", Group = "Filiales" },
                new UserClaimViewModel { Value = Permissions.Filiales.Crear, DisplayName = "Crear Filiales", Group = "Filiales" },
                new UserClaimViewModel { Value = Permissions.Filiales.Editar, DisplayName = "Editar Filiales", Group = "Filiales" },
                new UserClaimViewModel { Value = Permissions.Filiales.Eliminar, DisplayName = "Eliminar Filiales", Group = "Filiales" },

                new UserClaimViewModel { Value = Permissions.Agencias.Ver, DisplayName = "Ver Agencias", Group = "Agencias" },
                new UserClaimViewModel { Value = Permissions.Agencias.Crear, DisplayName = "Crear Agencias", Group = "Agencias" },
                new UserClaimViewModel { Value = Permissions.Agencias.Editar, DisplayName = "Editar Agencias", Group = "Agencias" },
                new UserClaimViewModel { Value = Permissions.Agencias.Eliminar, DisplayName = "Eliminar Agencias", Group = "Agencias" },

                new UserClaimViewModel { Value = Permissions.Personal.Ver, DisplayName = "Ver Lista de Personal", Group = "Personal" },
                new UserClaimViewModel { Value = Permissions.Personal.Crear, DisplayName = "Crear Usuarios", Group = "Personal" },
                new UserClaimViewModel { Value = Permissions.Personal.EditarDatos, DisplayName = "Editar Datos Personales", Group = "Personal" },
                new UserClaimViewModel { Value = Permissions.Personal.Permisos, DisplayName = "Modificar Accesos y Permisos", Group = "Personal" },
                new UserClaimViewModel { Value = Permissions.Personal.Eliminar, DisplayName = "Elimimar Usuarios", Group = "Personal" },

                new UserClaimViewModel { Value = Permissions.Inventario.Ver, DisplayName = "Ver Inventario", Group = "Inventario" },
                new UserClaimViewModel { Value = Permissions.Inventario.Ingresos, DisplayName = "Realizar ingresos directos", Group = "Inventario" },
                new UserClaimViewModel { Value = Permissions.Inventario.ConfigurarAlertas, DisplayName = "Definir stock minimo", Group = "Inventario" },
                new UserClaimViewModel { Value = Permissions.Inventario.Traslados, DisplayName = "Realizar traslados", Group = "Inventario" },
                new UserClaimViewModel { Value = Permissions.Inventario.Kardex, DisplayName = "Ver kardex", Group = "Inventario" },

            }
        };

        // Marcamos los permisos que el usuario ya tiene en la base de datos
        foreach (var claim in model.Claims)
        {
            if (existingClaims.Any(c => c.Type == "Permission" && c.Value == claim.Value))
            {
                claim.IsSelected = true;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManagePermissions(ManageUserPermissionsViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        // 1. Obtener todos los claims actuales
        var claims = await _userManager.GetClaimsAsync(user);

        // 2. Filtrar y eliminar solo los de tipo "Permission" para limpiar
        var permissionClaims = claims.Where(c => c.Type == "Permission");
        var removeResult = await _userManager.RemoveClaimsAsync(user, permissionClaims);

        if (!removeResult.Succeeded)
        {
            ModelState.AddModelError("", "Error al limpiar permisos antiguos.");
            return View(model);
        }

        // 3. Agregar los nuevos seleccionados en la vista
        var selectedClaims = model.Claims
            .Where(c => c.IsSelected)
            .Select(c => new System.Security.Claims.Claim("Permission", c.Value));

        var addResult = await _userManager.AddClaimsAsync(user, selectedClaims);

        if (addResult.Succeeded)
        {
            // --- LÓGICA DE ACTUALIZACIÓN EN TIEMPO REAL ---
            // Verificamos si el usuario que estamos editando es el mismo que está logueado
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id)
            {
                // Esto actualiza la cookie del Admin actual para que los cambios 
                // en el Sidebar se vean sin tener que cerrar sesión.
                await _signInManager.RefreshSignInAsync(user);
            }
            // ----------------------------------------------

            return RedirectToAction(nameof(Index), new { message = "Permisos actualizados correctamente" });
        }

        ModelState.AddModelError("", "Error al asignar nuevos permisos.");
        return View(model);
    }
}