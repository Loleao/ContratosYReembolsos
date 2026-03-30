using ContratosYReembolsos.Data;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContratosYReembolsos.Controllers
{
    public class PavilionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PavilionController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            return View();
        }

    }
}
