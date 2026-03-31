using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContratosYReembolsos.Controllers
{
    public class CoffinController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CoffinController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Coffins (Listado tipo Catálogo)
        public async Task<IActionResult> Index()
        {
            // Obtenemos los ataúdes de la base de datos
            var data = await _context.Ataudes.ToListAsync();
            return View(data);
        }

        // GET: Coffins/Create
        public IActionResult Create() => View();

        // POST: Coffins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coffin coffin, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Lógica para guardar la imagen si existe
                if (imageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(wwwRootPath, "images", "coffins");

                    // Crear carpeta si no existe
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    coffin.ImageUrl = "/images/coffins/" + fileName;
                }

                _context.Add(coffin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coffin);
        }

        // POST: Coffins/UpdateStock (Para ajustes rápidos de inventario)
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int amount)
        {
            var coffin = await _context.Ataudes.FindAsync(id);
            if (coffin == null) return NotFound();

            coffin.CurrentStock += amount;
            await _context.SaveChangesAsync();

            return Json(new { success = true, newStock = coffin.CurrentStock });
        }

        // GET: Coffins/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var coffin = await _context.Ataudes.FindAsync(id);
            if (coffin != null)
            {
                // Borrar imagen física si existe
                if (!string.IsNullOrEmpty(coffin.ImageUrl))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, coffin.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Exists(imagePath);
                }

                _context.Ataudes.Remove(coffin);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
