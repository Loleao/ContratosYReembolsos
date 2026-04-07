using ExcelDataReader;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ContratosYReembolsos.Services
{
    public class UbigeoService : IUbigeoService
    {
        private readonly ApplicationDbContext _context;

        public UbigeoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> ImportFromExcelAsync(string filePath)
        {
            try
            {
                // Registro necesario para que ExcelDataReader funcione en .NET Core
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                var table = result.Tables[0];
                var ubigeosList = new List<Ubigeo>();

                foreach (DataRow row in table.Rows)
                {
                    ubigeosList.Add(new Ubigeo
                    {
                        // Aseguramos que el Id siempre tenga 6 dígitos (rellenando con 0 a la izquierda)
                        Id = row["Id"].ToString().PadLeft(6, '0'),
                        Region = row["Region"].ToString().ToUpper(),
                        Province = row["Province"].ToString().ToUpper(),
                        District = row["District"].ToString().ToUpper(),
                        Abbreviation = row["Abbreviation"] == DBNull.Value ? null : row["Abbreviation"].ToString().ToUpper()
                    });
                }

                if (ubigeosList.Any())
                {
                    // Limpieza opcional para evitar duplicados si re-importas
                    var existing = await _context.Ubigeos.ToListAsync();
                    _context.Ubigeos.RemoveRange(existing);

                    await _context.Ubigeos.AddRangeAsync(ubigeosList);
                    await _context.SaveChangesAsync();
                }

                return (true, $"Se importaron {ubigeosList.Count} registros correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task SeedIfEmptyAsync()
        {
            // Solo actuamos si la tabla está vacía
            if (!await _context.Ubigeos.AnyAsync())
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Ubigeos.xlsx");

                if (File.Exists(path))
                {
                    await ImportFromExcelAsync(path);
                    Console.WriteLine(">>> FONAFUN: Ubigeos cargados automáticamente desde Excel.");
                }
                else
                {
                    Console.WriteLine(">>> FONAFUN WARNING: No se encontró Ubigeos.xlsx en la carpeta Data.");
                }
            }
        }
    }
}