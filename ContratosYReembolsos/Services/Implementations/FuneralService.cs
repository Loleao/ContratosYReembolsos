using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Models.Entities.Contracts;
using ContratosYReembolsos.Services.Interfaces;

namespace ContratosYReembolsos.Services.Implementations
{
    public class FuneralServiceImplementation : IFuneralService
    {
        private readonly ApplicationDbContext _context;

        public FuneralServiceImplementation(ApplicationDbContext context)
        {
            _context = context;
        }

        // ... tus implementaciones anteriores de GetAll y Upsert ...

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
                var servicesList = new List<FuneralService>();

                foreach (DataRow row in table.Rows)
                {
                    servicesList.Add(new FuneralService
                    {
                        Name = row["Name"].ToString(),
                        // Convertimos el precio asegurando que no falle por nulos o formatos
                        Price = Convert.ToDecimal(row["Price"] ?? 0),
                        Description = row["Description"]?.ToString(),
                        IsActive = true
                    });
                }

                if (servicesList.Any())
                {
                    // Evitamos duplicados limpiando la tabla o podrías comparar por nombre
                    var existing = await _context.ServiciosFunerarios.ToListAsync();
                    _context.ServiciosFunerarios.RemoveRange(existing);

                    await _context.ServiciosFunerarios.AddRangeAsync(servicesList);
                    await _context.SaveChangesAsync();
                }

                return (true, $"Se importaron {servicesList.Count} servicios correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al importar servicios: {ex.Message}");
            }
        }

        public async Task SeedIfEmptyAsync()
        {
            if (!await _context.ServiciosFunerarios.AnyAsync())
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "FuneralServices.xlsx");

                if (File.Exists(path))
                {
                    await ImportFromExcelAsync(path);
                    Console.WriteLine(">>> FONAFUN: Servicios Funerarios cargados desde Excel.");
                }
                else
                {
                    Console.WriteLine(">>> FONAFUN WARNING: No se encontró FuneralServices.xlsx.");
                }
            }
        }

        // Implementación requerida por la interfaz para cumplir con el contrato anterior
        public async Task<List<FuneralService>> GetAllServicesAsync() => await _context.ServiciosFunerarios.ToListAsync();

        public async Task<(bool success, string message)> UpsertServiceAsync(FuneralService model)
        {
            if (model.Id == 0) _context.Add(model); else _context.Update(model);
            await _context.SaveChangesAsync();
            return (true, "Ok");
        }
    }
}