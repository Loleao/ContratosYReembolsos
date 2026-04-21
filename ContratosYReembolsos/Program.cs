using System;
using ContratosYReembolsos.Constants;
using ContratosYReembolsos.Data;
using ContratosYReembolsos.Hubs;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var externalConnectionString = builder.Configuration.GetConnectionString("LimaConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<LimaContractsDbContext>(options =>
    options.UseSqlServer(externalConnectionString));

builder.Services.AddScoped<IntermentService>();
builder.Services.AddScoped<IUbigeoService, UbigeoService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddSignalR();

// 1. Registrar Identity para usar nuestra clase ApplicationUser y Roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    // --- SCOPES DE ATAÚDES ---
    options.AddPolicy("Policy.Ataudes.Full", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            (context.User.HasClaim("Permission", Permissions.Ataudes.Ingresos) &&
             context.User.HasClaim("Permission", Permissions.Ataudes.Traslados))));

    // --- SCOPES DE VENTAS ---
    options.AddPolicy("Policy.Contratos.Operar", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("Permission", Permissions.Contratos.Crear)));

    // --- SCOPE DE RRHH / SEGURIDAD ---
    options.AddPolicy("Policy.Personal.Admin", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("Permission", Permissions.Personal.Permisos)));

    // --- SCOPE GLOBAL DE LECTURA (Para el Sidebar) ---
    // Si tiene CUALQUIER claim que termine en ".Ver", puede ver el módulo correspondiente
    options.AddPolicy("Policy.Global.Lectura", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.Claims.Any(c => c.Type == "Permission" && c.Value.EndsWith(".Ver"))));
});

// 2. Configurar hacia dónde redirigir si no está logueado
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var ubigeoService = services.GetRequiredService<IUbigeoService>();
        var intermentService = services.GetRequiredService<IntermentService>();

        // Llamada al inicializador externo
        await DbInitializer.SeedAsync(context, ubigeoService, intermentService);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar los datos maestros.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminDni = "00000000";
        var adminUser = await userManager.FindByNameAsync(adminDni);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminDni,
                DNI = adminDni,
                FullName = "Administrador Sistema",
                Email = "admin@fonafun.com",
                EmailConfirmed = true
            };

            // La clave será Admin123* (puedes cambiarla)
            await userManager.CreateAsync(user, "Admin123*");
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Erro al crear el usuario Admin inicial.");
    }
}


app.Run();
