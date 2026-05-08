using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Models.Entities.Cemeteries;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Models.Entities.Transport;
using ContratosYReembolsos.Services;
using ContratosYReembolsos.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Data.SeedData
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context, 
            IUbigeoService ubigeoService,
            ICatalogService catalogService,
            IFuneralService funeralService,
            IBranchService branchService,
            IntermentService intermentService, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Servicios Externos
            await ubigeoService.SeedIfEmptyAsync();
            await branchService.SeedIfEmptyAsync();
            await catalogService.SeedCatalogIfEmptyAsync();
            await funeralService.SeedIfEmptyAsync();

            // 3. Cementerios
            if (!context.Cementerios.Any())
            {
                var branchLim2 = await context.Filiales.FirstOrDefaultAsync(f => f.Code == "LIM2");

                if (branchLim2 != null)
                {
                    var cemeteries = new List<Cemetery>
                    {
                        new Cemetery {
                            RUC = "20123456789",
                            Name = "PEC SANTA ROSA",
                            BranchId = branchLim2.Id,
                            UbigeoId = "150108",
                            Address = "Av. Alipio Ponce Vasquez Chorrillos",
                            Phone = "999999999", // Asegúrate de incluirlo si es obligatorio
                            Email = "pecsantarosa@fonafun.com", // CORRECCIÓN: Agregar valor para evitar el error de NULL
                            IsActive = true,
                            IsInternal = true
                        },
                        new Cemetery {
                            RUC = "11111111111",
                            Name = "Cementerio Generico",
                            BranchId = branchLim2.Id,
                            UbigeoId = "150108",
                            Address = "Av. generico",
                            Phone = "111111111", // Asegúrate de incluirlo si es obligatorio
                            Email = "generico@generico.com", // CORRECCIÓN: Agregar valor para evitar el error de NULL
                            IsActive = true,
                            IsInternal = false
                        }
                    };
                    context.Cementerios.AddRange(cemeteries);
                    await context.SaveChangesAsync();
                }
            }

            // 4. Tipos de Vehículo
            if (!context.TiposVehiculo.Any())
            {
                var types = new List<VehicleType>
                {
                    new VehicleType { Name = "CARROZA FÚNEBRE", Icon = "fa-car-side" },
                    new VehicleType { Name = "COCHE DE FLORES", Icon = "fa-truck-pickup" },
                    new VehicleType { Name = "BUS DE ACOMPAÑANTES", Icon = "fa-bus" }
                };
                context.TiposVehiculo.AddRange(types);
                await context.SaveChangesAsync();
            }

            // 5. Templates de Sepulturas
            if (!context.TemplatesSepulturas.Any())
            {
                var templates = new List<IntermentStructureTemplate>
                {
                    new IntermentStructureTemplate { Name = "Pabellón Modelo 1 - 1 piso", Type = "PABELLON", TotalFloors = 1, RowsCount = 4, ColsPerFace = 25, IsDoubleFace = true, DefaultPrice = 0},
                    new IntermentStructureTemplate { Name = "Pabellón Modelo 1 - 2 pisos", Type = "PABELLON", TotalFloors = 2, RowsCount = 4, ColsPerFace = 25, IsDoubleFace = true, DefaultPrice = 0},
                    new IntermentStructureTemplate { Name = "Pabellón Modelo 2 - 1 piso", Type = "PABELLON", TotalFloors = 1, RowsCount = 6, ColsPerFace = 25, IsDoubleFace = true, DefaultPrice = 0},
                    new IntermentStructureTemplate { Name = "Columbario Modelo 1 - 1 piso", Type = "COLUMBARIO", TotalFloors = 1, RowsCount = 4, ColsPerFace = 18, IsDoubleFace = true, DefaultPrice = 0},
                };
                context.TemplatesSepulturas.AddRange(templates);
                await context.SaveChangesAsync();
            }

            // 6. Estructuras y Construcción de Nichos
            if (!context.SepulturasEstructura.Any())
            {
                // Obtenemos el cementerio que acabamos de crear
                var santaRosa = await context.Cementerios.FirstOrDefaultAsync(c => c.Name == "PEC SANTA ROSA");

                if (santaRosa != null)
                {
                    var structures = new List<IntermentStructure>
            {
                // Usamos santaRosa.Id en lugar de "1"
                new IntermentStructure { Name = "San Pedro", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = santaRosa.Id, TemplateId = 1},
                new IntermentStructure { Name = "San Pablo", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = santaRosa.Id, TemplateId = 2},
                new IntermentStructure { Name = "San Juan", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = santaRosa.Id, TemplateId = 3},
                new IntermentStructure { Name = "San Miguel", Type = "COLUMBARIO", Status = "DISPONIBLE", CemeteryId = santaRosa.Id, TemplateId = 4},
            };

                    context.SepulturasEstructura.AddRange(structures);
                    await context.SaveChangesAsync();

                    // Construcción física de nichos
                    foreach (var item in structures)
                    {
                        if (item.TemplateId > 0)
                        {
                            await intermentService.BuildFromTemplateAsync(item.TemplateId.Value, item.Id);
                        }
                    }
                }
            }

            // 7. Roles y Usuario Admin (SECCIÓN NUEVA)
            await SeedIdentity(userManager, roleManager, context);
        }

        private static async Task SeedIdentity(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            // Crear Rol Admin
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminDni = "00000000";
            var adminUser = await userManager.FindByNameAsync(adminDni);

            if (adminUser == null)
            {
                // Buscamos la filial LIM2 que creamos arriba para asignarla
                var defaultBranch = await context.Filiales.FirstOrDefaultAsync(f => f.Code == "LIM2")
                                    ?? await context.Filiales.FirstOrDefaultAsync();

                var user = new ApplicationUser
                {
                    UserName = adminDni,
                    DNI = adminDni,
                    FullName = "Administrador Sistema",
                    Email = "admin@fonafun.com",
                    EmailConfirmed = true,
                    BranchId = defaultBranch?.Id // Asignación segura del ID real
                };

                var result = await userManager.CreateAsync(user, "Admin123*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}