using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Branches;
using ContratosYReembolsos.Services.Interfaces;

namespace ContratosYReembolsos.Services.Implementations
{
    public class WakeService : IWakeService
    {
        private readonly ApplicationDbContext _context;

        public WakeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> ImportFromExcelAsync(string filePath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                var table = result.Tables[0];
                var wakesList = new List<Wake>();

                foreach (DataRow row in table.Rows)
                {
                    // Validamos que la fila tenga al menos un nombre y un BranchId válido
                    if (row["Name"] == DBNull.Value || row["BranchId"] == DBNull.Value)
                        continue;

                    wakesList.Add(new Wake
                    {
                        // Mapeo directo usando tus encabezados exactos: Id, Name, Address, IsInternal, BranchId, IsActive
                        Name = row["Name"].ToString().ToUpper().Trim(),
                        Address = row["Address"] == DBNull.Value ? null : row["Address"].ToString().Trim(),
                        IsInternal = row["IsInternal"] != DBNull.Value && Convert.ToBoolean(row["IsInternal"]),
                        BranchId = Convert.ToInt32(row["BranchId"]),
                        IsActive = row["IsActive"] == DBNull.Value || Convert.ToBoolean(row["IsActive"])
                    });
                }

                if (wakesList.Any())
                {
                    // Limpieza total preventiva de la tabla antes de re-importar
                    var existing = await _context.Velatorios.ToListAsync();
                    _context.Velatorios.RemoveRange(existing);

                    await _context.Velatorios.AddRangeAsync(wakesList);
                    await _context.SaveChangesAsync();
                }

                return (true, $"Se importaron {wakesList.Count} velatorios correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al importar velatorios: {ex.Message}");
            }
        }

        public async Task SeedIfEmptyAsync()
        {
            if (!await _context.Velatorios.AnyAsync())
            {
                // El archivo debe llamarse Wakes.xlsx en tu carpeta de SeedData
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Wakes.xlsx");

                if (File.Exists(path))
                {
                    var result = await ImportFromExcelAsync(path);
                    if (result.success)
                    {
                        Console.WriteLine($">>> FONAFUN: {result.message}");
                    }
                    else
                    {
                        Console.WriteLine($">>> FONAFUN ERROR: {result.message}");
                    }
                }
                else
                {
                    Console.WriteLine($">>> FONAFUN WARNING: No se encontró Wakes.xlsx en la ruta esperada.");
                }
            }
        }
    }
}