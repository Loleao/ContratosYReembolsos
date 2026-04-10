using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, IUbigeoService ubigeoService, IntermentService intermentService)
        {
            // 1. Ubigeos (Servicio Externo)
            await ubigeoService.SeedIfEmptyAsync();

            // 2. Filiales (Branches)
            if (!context.Filiales.Any())
            {
                var branches = new List<Branch>
                {
                    new Branch
                    {
                        Name = "Almacen Central - LIMA",
                        UbigeoId = "150108",
                        Code = "LIM1",
                        Address = "Av. Alipio Ponce Vasquez Chorrillos, Av. Los Eucaliptos",
                        Phone = "999999999",
                        Email = "lima@gmail.com",
                        IsActive = true,
                        HasWakeService = false,
                        HasOwnCemetery = false
                    },
                    new Branch
                    {
                        Name = "Filial Av. Brasil - LIMA",
                        UbigeoId = "150120",
                        Code = "LIM2",
                        Address = "Av. Brasil 2905, Magdalena del Mar 15086",
                        Phone = "999999999",
                        Email = "avbrasil@gmail.com",
                        IsActive = true,
                        HasWakeService = true,
                        HasOwnCemetery = true
                    }
                };
                context.Filiales.AddRange(branches);
                await context.SaveChangesAsync();
            }

            // 3. Cementerios
            if (!context.Cementerios.Any())
            {
                var branchLim2 = await context.Filiales.FirstOrDefaultAsync(f => f.Code == "LIM2");
                if (branchLim2 != null)
                {
                    var cemetery = new Cemetery
                    {
                        RUC = "20123456789",
                        Name = "PEC SANTA ROSA",
                        BranchId = branchLim2.Id,
                        Address = "Av. Alipio Ponce Vasquez Chorrillos, Av. Los Eucaliptos",
                        Phone = "999999999",
                        Email = "lima@gmail.com",
                        IsActive = true
                    };
                    context.Cementerios.Add(cemetery);
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
            // 6. Estructuras y Construcción de Nichos
            if (!context.SepulturasEstructura.Any())
            {
                var structures = new List<IntermentStructure>
                {
                    new IntermentStructure { Name = "San Pedro", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = 1, TemplateId = 1},
                    new IntermentStructure { Name = "San Pablo", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = 1, TemplateId = 2},
                    new IntermentStructure { Name = "San Juan", Type = "PABELLON", Status = "DISPONIBLE", CemeteryId = 1, TemplateId = 3},
                    new IntermentStructure { Name = "San Miguel", Type = "COLUMBARIO", Status = "DISPONIBLE", CemeteryId = 1, TemplateId = 4},
                };

                context.SepulturasEstructura.AddRange(structures);
                await context.SaveChangesAsync();

                // Construcción de nichos mediante el servicio
                foreach (var item in structures)
                {
                    int templateId = item.TemplateId ?? 0;
                    int structureId = item.Id;

                    if (templateId > 0)
                    {
                        await intermentService.BuildFromTemplateAsync(templateId, structureId);
                    }
                }
            }
        }
    }
}