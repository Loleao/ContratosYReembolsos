using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.ViewModels.Users;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<List<UserListViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.Include(u => u.Branch).ToListAsync();
            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName, // Agregado
                    DNI = user.DNI,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "Operador",
                    BranchName = user.Branch?.Name ?? "Sede Central"
                });
            }
            return model;
        }

        public async Task<RegisterUserViewModel> GetUserForEditingAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            return new RegisterUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                DNI = user.DNI,
                FullName = user.FullName,
                Email = user.Email,
                BranchId = user.BranchId
            };
        }

        public async Task<IdentityResult> CreateUserAsync(RegisterUserViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                DNI = model.DNI,
                FullName = model.FullName,
                Email = model.Email,
                BranchId = model.BranchId,
                EmailConfirmed = true
            };

            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<IdentityResult> UpdateUserAsync(RegisterUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado" });

            // Actualizar datos básicos
            user.DNI = model.DNI;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.BranchId = model.BranchId;

            var result = await _userManager.UpdateAsync(user);

            // Si se escribió una nueva contraseña, actualizarla
            if (result.Succeeded && !string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.Password);
            }

            return result;
        }

        public async Task<ManageUserPermissionsViewModel> GetUserForPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // Obtenemos los claims actuales del usuario
            var existingClaims = await _userManager.GetClaimsAsync(user);

            // Obtenemos la lista maestra
            var allPermissions = GetAllAvailablePermissions();

            // Cruzamos los datos
            foreach (var permission in allPermissions)
            {
                // Marcamos como seleccionado si el usuario ya tiene ese claim de tipo 'Permission'
                permission.IsSelected = existingClaims.Any(c => c.Type == "Permission" && c.Value == permission.Value);
            }

            return new ManageUserPermissionsViewModel
            {
                UserId = userId,
                FullName = user.FullName,
                DNI = user.DNI,
                Claims = allPermissions
            };
        }

        public async Task<bool> UpdateUserPermissionsAsync(ManageUserPermissionsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return false;

            // 1. Obtener y eliminar todos los claims actuales de tipo "Permission"
            var currentClaims = await _userManager.GetClaimsAsync(user);
            var permissionClaims = currentClaims.Where(c => c.Type == "Permission");
            await _userManager.RemoveClaimsAsync(user, permissionClaims);

            // 2. Filtrar los que el Admin marcó en la vista
            var selectedClaims = model.Claims
                .Where(c => c.IsSelected)
                .Select(c => new System.Security.Claims.Claim("Permission", c.Value));

            // 3. Guardar los nuevos claims
            var result = await _userManager.AddClaimsAsync(user, selectedClaims);

            return result.Succeeded;
        }

        private List<UserClaimViewModel> GetAllAvailablePermissions()
        {
            return new List<UserClaimViewModel>
            {
                // Módulo Contratos
                new UserClaimViewModel { Category="Contratos", Group = "Contratos", DisplayName = "Ver Contratos", Value = "Contratos.Ver" },
                new UserClaimViewModel { Category="Contratos", Group = "Contratos", DisplayName = "Crear Nuevo Contrato", Value = "Contratos.Crear" },
                new UserClaimViewModel { Category="Contratos", Group = "Contratos", DisplayName = "Editar Contrato", Value = "Contratos.Editar" },

                // Modulo Exhumaciones
                new UserClaimViewModel { Category="Contratos", Group = "Exhumaciones", DisplayName = "Ver Exhumaciones", Value = "Exhumaciones.Ver" },
                new UserClaimViewModel { Category="Contratos", Group = "Exhumaciones", DisplayName = "Crear Nueva Exhumacion", Value = "Exhumaciones.Crear" },

                // Módulo Catálogo de Servicios (Funerarios)
                new UserClaimViewModel { Category="Contratos", Group = "Catálogo de Servicios", DisplayName = "Ver Catálogo", Value = "CatalogoServicios.Ver" },
                new UserClaimViewModel { Category="Contratos", Group = "Catálogo de Servicios", DisplayName = "Crear Servicios", Value = "CatalogoServicios.Crear" },
                new UserClaimViewModel { Category="Contratos", Group = "Catálogo de Servicios", DisplayName = "Editar Servicios", Value = "CatalogoServicios.Editar" },
                new UserClaimViewModel { Category="Contratos", Group = "Catálogo de Servicios", DisplayName = "Eliminar Servicios", Value = "CatalogoServicios.Eliminar" },

                // Módulo Usuarios (Admin)
                new UserClaimViewModel { Category="Seguridad", Group = "Seguridad", DisplayName = "Gestionar Usuarios", Value = "Usuarios.Gestionar" },
                new UserClaimViewModel { Category="Seguridad", Group = "Seguridad", DisplayName = "Asignar Permisos", Value = "Usuarios.Permisos" },

                // Módulo Inventario
                new UserClaimViewModel { Category="Inventario", Group = "Inventario", DisplayName = "Ver Inventario", Value = "Inventario.Ver" },
                new UserClaimViewModel { Category="Inventario", Group = "Inventario", DisplayName = "Ingreso Directo", Value = "Inventario.Ingreso" },
                new UserClaimViewModel { Category="Inventario", Group = "Inventario", DisplayName = "Transferencia", Value = "Inventario.Transferencia" },
                new UserClaimViewModel { Category="Inventario", Group = "Inventario", DisplayName = "Alertas", Value = "Inventario.Alertas" },


                // Módulo Catálogo de Inventario
                new UserClaimViewModel { Category="Inventario", Group = "Catálogo de Inventario", DisplayName = "Ver Catálogo", Value = "CatalogoInventario.Ver" },
                new UserClaimViewModel { Category="Inventario", Group = "Catálogo de Inventario", DisplayName = "Crear categorias, subcategorias o productos", Value = "CatalogoInventario.Crear" },
                new UserClaimViewModel { Category="Inventario", Group = "Catálogo de Inventario", DisplayName = "Editar categorias, subcategorias o productos", Value = "CatalogoInventario.Editar" },
                new UserClaimViewModel { Category="Inventario", Group = "Catálogo de Inventario", DisplayName = "Eliminar categorias, subcategorias o productos", Value = "CatalogoInventario.Eliminar" },

                // Módulo Activos Fijos
                new UserClaimViewModel { Category="Activos Fijos", Group = "Activos Fijos", DisplayName = "Ver Activos Fijos", Value = "Activos.Ver" },

                // Módulo Movilidad
                new UserClaimViewModel { Category="Movilidad", Group = "Movilidad", DisplayName = "Ver Gestión de Unidades", Value = "Movilidad.Ver" },
                new UserClaimViewModel { Category="Movilidad", Group = "Movilidad", DisplayName = "Asignar vehiculos", Value = "Movilidad.Asignar" },
                new UserClaimViewModel { Category="Movilidad", Group = "Movilidad", DisplayName = "Finalizar servicio", Value = "Movilidad.Finalizar" },

                // Módulo Catalogo de Vehiculos
                new UserClaimViewModel { Category="Movilidad", Group = "Catálogo de Vehiculos", DisplayName = "Ver Catálogo", Value = "CatalogoVehiculos.Ver" },
                new UserClaimViewModel { Category="Movilidad", Group = "Catálogo de Vehiculos", DisplayName = "Crear tipo de vehiculo", Value = "CatalogoVehiculos.Crear" },
                new UserClaimViewModel { Category="Movilidad", Group = "Catálogo de Vehiculos", DisplayName = "Editar tipo de vehiculo", Value = "CatalogoVehiculos.Editar" },
                new UserClaimViewModel { Category="Movilidad", Group = "Catálogo de Vehiculos", DisplayName = "Eliminar tipo de vehiculo", Value = "CatalogoVehiculos.Eliminar" },

                // Módulo Vehiculos
                new UserClaimViewModel { Category="Movilidad", Group = "Vehiculos", DisplayName = "Ver Vehiculos", Value = "Vehiculos.Ver" },
                new UserClaimViewModel { Category="Movilidad", Group = "Vehiculos", DisplayName = "Crear Vehiculos", Value = "Vehiculos.Crear" },
                new UserClaimViewModel { Category="Movilidad", Group = "Vehiculos", DisplayName = "Editar Vehiculos", Value = "Vehiculos.Editar" },
                new UserClaimViewModel { Category="Movilidad", Group = "Vehiculos", DisplayName = "Eliminar Vehiculos", Value = "Vehiculos.Eliminar" },

                // Módulo Conductores
                new UserClaimViewModel { Category="Movilidad", Group = "Conductores", DisplayName = "Ver Conductores", Value = "Conductores.Ver" },
                new UserClaimViewModel { Category="Movilidad", Group = "Conductores", DisplayName = "Crear Conductores", Value = "Conductores.Crear" },
                new UserClaimViewModel { Category="Movilidad", Group = "Conductores", DisplayName = "Editar Conductores", Value = "Conductores.Editar" },
                new UserClaimViewModel { Category="Movilidad", Group = "Conductores", DisplayName = "Eliminar Conductores", Value = "Conductores.Eliminar" },

                // Módulo Cementerios
                new UserClaimViewModel { Category="Sepulturas", Group = "Cementerios", DisplayName = "Ver Cementerios", Value = "Cementerios.Ver" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Cementerios", DisplayName = "Crear Cementerios", Value = "Cementerios.Crear" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Cementerios", DisplayName = "Editar Cementerios", Value = "Cementerios.Editar" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Cementerios", DisplayName = "Eliminar Cementerios", Value = "Cementerios.Eliminar" },

                // Módulo EStructuras
                new UserClaimViewModel { Category="Sepulturas", Group = "Estructuras", DisplayName = "Ver Estructuras", Value = "Estructuras.Ver" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Estructuras", DisplayName = "Crear Estructuras", Value = "Estructuras.Crear" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Estructuras", DisplayName = "Editar Estructuras", Value = "Estructuras.Editar" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Estructuras", DisplayName = "Eliminar Estructuras", Value = "Estructuras.Eliminar" },

                // Módulo Espacios
                new UserClaimViewModel { Category="Sepulturas", Group = "Espacios", DisplayName = "Ver Espacios", Value = "Espacios.Ver" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Espacios", DisplayName = "Crear Espacios", Value = "Espacios.Crear" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Espacios", DisplayName = "Editar Espacios", Value = "Espacios.Editar" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Espacios", DisplayName = "Eliminar Espacios", Value = "Espacios.Eliminar" },

                // Módulo Modelos
                new UserClaimViewModel { Category="Sepulturas", Group = "Modelos de Cementerios", DisplayName = "Ver Modelos", Value = "Modelos.Ver" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Modelos de Cementerios", DisplayName = "Crear Modelos", Value = "Modelos.Crear" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Modelos de Cementerios", DisplayName = "Editar Modelos", Value = "Modelos.Editar" },
                new UserClaimViewModel { Category="Sepulturas", Group = "Modelos de Cementerios", DisplayName = "Eliminar Modelos", Value = "Modelos.Eliminar" },

                // Módulo Filiales
                new UserClaimViewModel { Category="Filiales", Group = "Filiales", DisplayName = "Ver Filiales", Value = "Filiales.Ver" },
                new UserClaimViewModel { Category="Filiales", Group = "Filiales", DisplayName = "Crear Filiales", Value = "Filiales.Crear" },
                new UserClaimViewModel { Category="Filiales", Group = "Filiales", DisplayName = "Editar Filiales", Value = "Filiales.Editar" },
                new UserClaimViewModel { Category="Filiales", Group = "Filiales", DisplayName = "Eliminar Filiales", Value = "Filiales.Eliminar" },

                // Módulo Agencias
                new UserClaimViewModel { Category="Convenios", Group = "Convenios", DisplayName = "Ver Convenios", Value = "Convenios.Ver" },
                new UserClaimViewModel { Category="Convenios", Group = "Convenios", DisplayName = "Crear Convenios", Value = "Convenios.Crear" },
                new UserClaimViewModel { Category="Convenios", Group = "Convenios", DisplayName = "Editar Convenios", Value = "Convenios.Editar" },
                new UserClaimViewModel { Category="Convenios", Group = "Convenios", DisplayName = "Eliminar Convenios", Value = "Convenios.Eliminar" },
            };
        }
    }
}
