using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;
using QuizCraft.Core.Interfaces;
using QuizCraft.Infrastructure.Data;
using QuizCraft.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// FUNC_ConfigurarBaseDatos: Configuración de Entity Framework y contexto de BD
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("QuizCraft.Infrastructure");
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    
    // Configuración adicional para desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// FUNC_ConfigurarIdentity: Configuración de ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configuración de contraseñas
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    
    // Configuración de usuarios
    options.User.RequireUniqueEmail = true;
    
    // Configuración de bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configuración de confirmación de email (deshabilitado para desarrollo)
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// FUNC_ConfigurarRepositorios: Inyección de dependencias para repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

// FUNC_ConfigurarAutenticacion: Configuración de cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// FUNC_ConfigurarMVC: Configuración de MVC y servicios web
builder.Services.AddControllersWithViews(options =>
{
    // Filtros globales si son necesarios
    // options.Filters.Add(new AuthorizeFilter());
});

// FUNC_ConfigurarSesiones: Configuración de sesiones para el estado de la aplicación
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// FUNC_ConfigurarMemoryCache: Configuración de caché en memoria
builder.Services.AddMemoryCache();

var app = builder.Build();

// FUNC_ConfigurarPipeline: Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// FUNC_ConfigurarAutenticacionPipeline: Configuración del pipeline de autenticación
app.UseAuthentication();
app.UseAuthorization();

// FUNC_ConfigurarSesionesPipeline: Activar sesiones
app.UseSession();

// FUNC_ConfigurarRutas: Configuración de rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// FUNC_InicializarBaseDatos: Inicialización de la base de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    try
    {
        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();
        
        // Inicializar roles básicos
        await FUNC_InicializarRoles(roleManager);
        
        // Crear usuario administrador si no existe
        await FUNC_CrearUsuarioAdministrador(userManager);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error durante la inicialización de la base de datos");
    }
}

app.Run();

// FUNC_InicializarRoles: Crear roles básicos del sistema
static async Task FUNC_InicializarRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Administrador", "Profesor", "Estudiante" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// FUNC_CrearUsuarioAdministrador: Crear usuario administrador por defecto
static async Task FUNC_CrearUsuarioAdministrador(UserManager<ApplicationUser> userManager)
{
    const string adminEmail = "admin@quizcraft.com";
    const string adminPassword = "Admin123!";
    
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            NombreCompleto = "Administrador del Sistema",
            EmailConfirmed = true,
            FechaRegistro = DateTime.UtcNow,
            PreferenciaIdioma = "es",
            NotificacionesHabilitadas = true
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
    }
}
