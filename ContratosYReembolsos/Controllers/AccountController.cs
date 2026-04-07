using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;

namespace ContratosYReembolsos.Controllers
{
    // Permitimos acceso anónimo a todo el controlador para que puedan loguearse
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string dni, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "El DNI y la contraseña son obligatorios.");
                return View();
            }

            // Identity usará el DNI para buscar en la columna UserName
            var result = await _signInManager.PasswordSignInAsync(dni, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "DNI o contraseña incorrectos.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}