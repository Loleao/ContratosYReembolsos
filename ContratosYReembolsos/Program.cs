using System;
using ContratosYReembolsos.Data.Contexts;
using ContratosYReembolsos.Data.SeedData;
using ContratosYReembolsos.Hubs;
using ContratosYReembolsos.Models;
using ContratosYReembolsos.Services;
using ContratosYReembolsos.Services.Implementations;
using ContratosYReembolsos.Services.Implementations.Agencies;
using ContratosYReembolsos.Services.Implementations.Branches;
using ContratosYReembolsos.Services.Implementations.Cemeteries;
using ContratosYReembolsos.Services.Implementations.Contracts;
using ContratosYReembolsos.Services.Implementations.Inventory;
using ContratosYReembolsos.Services.Implementations.Transport;
using ContratosYReembolsos.Services.Interfaces;
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
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IFuneralService, FuneralServiceImplementation>();
builder.Services.AddScoped<IWakeService, WakeService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<ICemeteryService, CemeteryService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IExhumationService, ExhumationService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IInhumationService, InhumationService>();

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
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<AdditionalUserClaimsPrincipalFactory>();

builder.Services.AddAuthorization(options =>
{
    // Función auxiliar para no repetir código (opcional, pero limpia el Program.cs)
    void AddPermissionPolicy(string name, string claimValue)
    {
        options.AddPolicy(name, policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim("Permission", claimValue) ||
                context.User.IsInRole("Admin")));
    }

    // --- MÓDULO CONTRATOS ---
    AddPermissionPolicy("Permissions.Contratos.Ver", "Contratos.Ver");
    AddPermissionPolicy("Permissions.Contratos.Crear", "Contratos.Crear");
    AddPermissionPolicy("Permissions.Contratos.Editar", "Contratos.Editar");


    // --- MÓDULO EXHUMACIONES ---
    AddPermissionPolicy("Permissions.Exhumaciones.Ver", "Exhumaciones.Ver");
    AddPermissionPolicy("Permissions.Exhumaciones.Crear", "Exhumaciones.Crear");

    // --- MÓDULO CATÁLOGO DE SERVICIOS ---
    AddPermissionPolicy("Permissions.CatalogoServicios.Ver", "CatalogoServicios.Ver");
    AddPermissionPolicy("Permissions.CatalogoServicios.Crear", "CatalogoServicios.Crear");
    AddPermissionPolicy("Permissions.CatalogoServicios.Editar", "CatalogoServicios.Editar");
    AddPermissionPolicy("Permissions.CatalogoServicios.Eliminar", "CatalogoServicios.Eliminar");

    // --- MÓDULO INVENTARIO ---
    AddPermissionPolicy("Permissions.Inventario.Ver", "Inventario.Ver");
    AddPermissionPolicy("Permissions.Inventario.Ingreso", "Inventario.Ingreso");
    AddPermissionPolicy("Permissions.Inventario.Transferencia", "Inventario.Transferencia");
    AddPermissionPolicy("Permissions.Inventario.Alertas", "Inventario.Alertas");

    // --- MÓDULO CATÁLOGO DE INVENTARIO ---
    AddPermissionPolicy("Permissions.CatalogoInventario.Ver", "CatalogoInventario.Ver");
    AddPermissionPolicy("Permissions.CatalogoInventario.Crear", "CatalogoInventario.Crear");
    AddPermissionPolicy("Permissions.CatalogoInventario.Editar", "CatalogoInventario.Editar");
    AddPermissionPolicy("Permissions.CatalogoInventario.Eliminar", "CatalogoInventario.Eliminar");

    // --- MÓDULO ACTIVOS FIJOS ---
    AddPermissionPolicy("Permissions.Activos.Ver", "Activos.Ver");

    // --- MÓDULO MOVILIDAD ---
    AddPermissionPolicy("Permissions.Movilidad.Ver", "Movilidad.Ver");
    AddPermissionPolicy("Permissions.Movilidad.Asignar", "Movilidad.Asignar");
    AddPermissionPolicy("Permissions.Movilidad.Finalizar", "Movilidad.Finalizar");

    // --- MÓDULO CATÁLOGO DE VEHICULOS ---
    AddPermissionPolicy("Permissions.CatalogoVehiculos.Ver", "CatalogoVehiculos.Ver");
    AddPermissionPolicy("Permissions.CatalogoVehiculos.Crear", "CatalogoVehiculos.Crear");
    AddPermissionPolicy("Permissions.CatalogoVehiculos.Editar", "CatalogoVehiculos.Editar");
    AddPermissionPolicy("Permissions.CatalogoVehiculos.Eliminar", "CatalogoVehiculos.Eliminar");

    // --- MÓDULO VEHICULOS ---
    AddPermissionPolicy("Permissions.Vehiculos.Ver", "Vehiculos.Ver");
    AddPermissionPolicy("Permissions.Vehiculos.Crear", "Vehiculos.Crear");
    AddPermissionPolicy("Permissions.Vehiculos.Editar", "Vehiculos.Editar");
    AddPermissionPolicy("Permissions.Vehiculos.Eliminar", "Vehiculos.Eliminar");

    // --- MÓDULO CONDUCTORES ---
    AddPermissionPolicy("Permissions.Conductores.Ver", "Conductores.Ver");
    AddPermissionPolicy("Permissions.Conductores.Crear", "Conductores.Crear");
    AddPermissionPolicy("Permissions.Conductores.Editar", "Conductores.Editar");
    AddPermissionPolicy("Permissions.Conductores.Eliminar", "Conductores.Eliminar");

    // --- MÓDULO CEMENTERIOS ---
    AddPermissionPolicy("Permissions.Cementerios.Ver", "Cementerios.Ver");
    AddPermissionPolicy("Permissions.Cementerios.Crear", "Cementerios.Crear");
    AddPermissionPolicy("Permissions.Cementerios.Editar", "Cementerios.Editar");
    AddPermissionPolicy("Permissions.Cementerios.Eliminar", "Cementerios.Eliminar");

    // --- MÓDULO ESTRUCTURAS ---
    AddPermissionPolicy("Permissions.Estructuras.Ver", "Estructuras.Ver");
    AddPermissionPolicy("Permissions.Estructuras.Crear", "Estructuras.Crear");
    AddPermissionPolicy("Permissions.Estructuras.Editar", "Estructuras.Editar");
    AddPermissionPolicy("Permissions.Estructuras.Eliminar", "Estructuras.Eliminar");

    // --- MÓDULO ESPACIOS ---
    AddPermissionPolicy("Permissions.Espacios.Ver", "Espacios.Ver");
    AddPermissionPolicy("Permissions.Espacios.Crear", "Espacios.Crear");
    AddPermissionPolicy("Permissions.Espacios.Editar", "Espacios.Editar");
    AddPermissionPolicy("Permissions.Espacios.Eliminar", "Espacios.Eliminar");

    // --- MÓDULO MODELOS ---
    AddPermissionPolicy("Permissions.Modelos.Ver", "Modelos.Ver");
    AddPermissionPolicy("Permissions.Modelos.Crear", "Modelos.Crear");
    AddPermissionPolicy("Permissions.Modelos.Editar", "Modelos.Editar");
    AddPermissionPolicy("Permissions.Modelos.Eliminar", "Modelos.Eliminar");

    // --- MÓDULO FILIALES ---
    AddPermissionPolicy("Permissions.Filiales.Ver", "Filiales.Ver");
    AddPermissionPolicy("Permissions.Filiales.Crear", "Filiales.Crear");
    AddPermissionPolicy("Permissions.Filiales.Editar", "Filiales.Editar");
    AddPermissionPolicy("Permissions.Filiales.Eliminar", "Filiales.Eliminar");

    // --- MÓDULO CONVENIOS ---
    AddPermissionPolicy("Permissions.Convenios.Ver", "Convenios.Ver");
    AddPermissionPolicy("Permissions.Convenios.Crear", "Convenios.Crear");
    AddPermissionPolicy("Permissions.Convenios.Editar", "Convenios.Editar");
    AddPermissionPolicy("Permissions.Convenios.Eliminar", "Convenios.Eliminar");

    // --- MÓDULO SEGURIDAD ---
    AddPermissionPolicy("Permissions.Usuarios.Gestionar", "Usuarios.Gestionar");
    AddPermissionPolicy("Permissions.Usuarios.Permisos", "Usuarios.Permisos");
});

// 2. Configurar hacia dónde redirigir si no está logueado
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Fuerza HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // Permite redirecciones de navegación
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Duración de la sesión
});

var app = builder.Build();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        var catalogService = services.GetRequiredService<ICatalogService>();
        var ubigeoService = services.GetRequiredService<IUbigeoService>();
        var funeralService = services.GetRequiredService<IFuneralService>();
        var branchService = services.GetRequiredService<IBranchService>();
        var wakeService = services.GetRequiredService<IWakeService>();
        var intermentService = services.GetRequiredService<IntermentService>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.SeedAsync(context, ubigeoService, catalogService, funeralService, branchService, wakeService, intermentService, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error durante la migración o el sembrado en el servidor.");
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


app.Run();
