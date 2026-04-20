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
                        UbigeoId = "150108",
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

            // 7. Catálogo de Productos (Ataúdes con SKU Inteligente)
            if (!context.ProductosCategorias.Any())
            {
                // A. Crear Categoría Maestra
                var catAtaudes = new ProductCategory
                {
                    Name = "Ataudes",
                    ShowInContracts = true
                };
                context.ProductosCategorias.Add(catAtaudes);
                await context.SaveChangesAsync(); // Guardamos para obtener el catAtaudes.Id

                // B. Crear Subcategorías
                var subCats = new List<ProductSubcategory>
                {
                    new ProductSubcategory { Name = "Estandar", CategoryId = catAtaudes.Id, ShowInContracts = true },
                    new ProductSubcategory { Name = "Semiviciado", CategoryId = catAtaudes.Id, ShowInContracts = true },
                    new ProductSubcategory { Name = "Parvulo", CategoryId = catAtaudes.Id, ShowInContracts = true }
                };
                context.ProductosSubcategorias.AddRange(subCats);
                await context.SaveChangesAsync(); // Guardamos para obtener los IDs de subcategorías

                // C. Crear Productos (1 Marrón y 1 Blanco por cada subcategoría)
                var productos = new List<Product>();
                var colores = new[] { "Marron", "Blanco" };

                // Prefijo de categoría (3 letras + ID)
                string catPart = (catAtaudes.Name.Length >= 3 ? catAtaudes.Name.Substring(0, 3) : catAtaudes.Name).ToUpper();
                string catIdPart = catAtaudes.Id.ToString();

                foreach (var sub in subCats)
                {
                    // Prefijo de subcategoría (3 letras + ID)
                    string subPart = (sub.Name.Length >= 3 ? sub.Name.Substring(0, 3) : sub.Name).ToUpper();
                    string subIdPart = sub.Id.ToString();

                    int correlativo = 1;

                    foreach (var color in colores)
                    {
                        string colorTitle = char.ToUpper(color[0]) + color.Substring(1).ToLower();
                        string subTitle = char.ToUpper(sub.Name[0]) + sub.Name.Substring(1).ToLower();

                        // Formato: CAT[ID]-SUB[ID]-000000
                        string generatedSku = $"{catPart}{catIdPart}-{subPart}{subIdPart}-{correlativo.ToString("D6")}";

                        productos.Add(new Product
                        {
                            Name = $"Ataud {colorTitle} {subTitle}",
                            Sku = generatedSku,
                            ControlType = ControlType.Stock,
                            CategoryId = catAtaudes.Id,
                            SubCategoryId = sub.Id,
                            IsAvailableForContract = true
                        });

                        correlativo++;
                    }
                }

                context.Productos.AddRange(productos);
                await context.SaveChangesAsync();
            }
        }
    }
}