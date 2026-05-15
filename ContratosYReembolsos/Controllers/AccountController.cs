using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ContratosYReembolsos.Models;
using Microsoft.AspNetCore.Authorization;
using ContratosYReembolsos.Services.Interfaces;

namespace ContratosYReembolsos.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // --- AÑADE ESTE MÉTODO GET ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si el usuario ya está autenticado, redirigir al Home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "El usuario y la contraseña son obligatorios.");
                return View();
            }

            var result = await _accountService.LoginAsync(username, password, rememberMe);

            if (result.Succeeded)
            {
                // Si hay una URL de retorno válida, redirigir allí, si no al Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            // Manejo de bloqueos o errores específicos si lo deseas
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "La cuenta está bloqueada temporalmente.");
            }
            else
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}