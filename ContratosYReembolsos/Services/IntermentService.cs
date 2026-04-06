using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;

namespace ContratosYReembolsos.Services
{
    public class IntermentService
    {
        private readonly ApplicationDbContext _context;

        public IntermentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task BuildFromTemplateAsync(int templateId, int structureId)
        {
            var template = await _context.TemplatesSepulturas.FindAsync(templateId);
            var structure = await _context.SepulturasEstructura.FindAsync(structureId);

            if (template == null || structure == null) return;

            var spaces = new List<IntermentSpace>();

            for (int p = 1; p <= template.TotalFloors; p++)
            {
                for (int r = 0; r < template.RowsCount; r++)
                {
                    char rowLetter = (char)('A' + r);
                    // Si es doble cara, llegamos hasta 50, si no, hasta 25
                    int maxCols = template.IsDoubleFace ? (template.ColsPerFace * 2) : template.ColsPerFace;

                    for (int c = 1; c <= maxCols; c++)
                    {
                        spaces.Add(new IntermentSpace
                        {
                            StructureId = structure.Id,
                            FloorNumber = p,
                            RowLetter = rowLetter.ToString(),
                            ColumnNumber = c,
                            // Código: PAB-P1-FA-C01
                            Code = $"{structure.Name.Substring(0, 3).ToUpper()}-P{p}-F{rowLetter}-C{c.ToString("D2")}",
                            Status = IntermentStatus.Disponible,
                            Price = template.DefaultPrice
                        });
                    }
                }
            }

            _context.SepulturasNichos.AddRange(spaces);
            await _context.SaveChangesAsync();
        }
    }
}
