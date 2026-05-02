using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Inventory;
using ContratosYReembolsos.Services.Interfaces;

namespace ContratosYReembolsos.Services.Implementations
{
    public class CatalogService : ICatalogService
    {
        private readonly ApplicationDbContext _context;

        public CatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedCatalogIfEmptyAsync()
        {
            // 1. Seed de Categorías
            if (!await _context.ProductosCategorias.AnyAsync())
            {
                var pathCat = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Categories.xlsx");
                if (File.Exists(pathCat))
                {
                    await ImportCategoriesAsync(pathCat);
                    Console.WriteLine(">>> FONAFUN: Categorías sincronizadas.");
                }
            }

            // 2. Seed de Subcategorías
            if (!await _context.ProductosSubcategorias.AnyAsync())
            {
                var pathSub = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Subcategories.xlsx");
                if (File.Exists(pathSub))
                {
                    await ImportSubcategoriesAsync(pathSub);
                    Console.WriteLine(">>> FONAFUN: Subcategorías sincronizadas.");
                }
            }

            // 3. Seed de Productos con generación de SKU
            if (!await _context.Productos.AnyAsync())
            {
                var pathProd = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Products.xlsx");
                if (File.Exists(pathProd))
                {
                    await ImportProductsWithSkuAsync(pathProd);
                    Console.WriteLine(">>> FONAFUN: Productos cargados con SKU generado.");
                }
            }
        }

        private async Task ImportCategoriesAsync(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            var table = result.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                // Conversión segura de int (0/1) a bool
                bool showInContractsValue = row["ShowInContracts"] != DBNull.Value &&
                                           Convert.ToInt32(row["ShowInContracts"]) == 1;

                _context.ProductosCategorias.Add(new ProductCategory
                {
                    Name = row["Name"].ToString().Trim().ToUpper(),
                    ShowInContracts = showInContractsValue
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task ImportSubcategoriesAsync(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            var table = result.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                // Conversión segura de int (0/1) a bool
                bool showInContractsValue = row["ShowInContracts"] != DBNull.Value &&
                                           Convert.ToInt32(row["ShowInContracts"]) == 1;

                _context.ProductosSubcategorias.Add(new ProductSubcategory
                {
                    Name = row["Name"].ToString().Trim().ToUpper(),
                    ShowInContracts = showInContractsValue,
                    CategoryId = Convert.ToInt32(row["CategoryId"])
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task ImportProductsWithSkuAsync(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            var table = result.Tables[0];

            // Diccionario para llevar el correlativo por subcategoría y no repetir SKUs
            var correlativos = new Dictionary<int, int>();

            foreach (DataRow row in table.Rows)
            {
                int catId = Convert.ToInt32(row["CategoryId"]);
                int subId = Convert.ToInt32(row["SubCategoryId"]);
                string name = row["Name"].ToString().Trim().ToUpper();

                // Obtener nombres para las iniciales del SKU (opcional, o puedes usar "PROD")
                var category = await _context.ProductosCategorias.FindAsync(catId);
                var subcategory = await _context.ProductosSubcategorias.FindAsync(subId);

                string catPart = (category?.Name.Length >= 3 ? category.Name.Substring(0, 3) : "CAT").ToUpper();
                string subPart = (subcategory?.Name.Length >= 3 ? subcategory.Name.Substring(0, 3) : "SUB").ToUpper();

                // Manejo del correlativo por subcategoría
                if (!correlativos.ContainsKey(subId)) correlativos[subId] = 1;
                int currentCorrelativo = correlativos[subId]++;

                // Formato: CAT[ID]-SUB[ID]-000000
                string generatedSku = $"{catPart}{catId}-{subPart}{subId}-{currentCorrelativo:D6}";

                _context.Productos.Add(new Product
                {
                    Name = name,
                    Sku = generatedSku,
                    Unit = Enum.Parse<UnitOfMeasure>(row["Unit"].ToString()), // Lee UND, LTS, M3C, etc.
                    CategoryId = catId,
                    SubCategoryId = subId,
                    IsAvailableForContract = Convert.ToInt32(row["IsAvailableForContract"]) == 1
                });
            }
            await _context.SaveChangesAsync();
        }

        public Task<(bool success, string message)> ImportCatalogFromExcelAsync(string filePath)
        {
            throw new NotImplementedException("Usar SeedCatalogIfEmptyAsync para carga inicial.");
        }
    }
}