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

// FUNC_ConfigurarServicios: Inyección de dependencias para servicios de aplicación
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IFileUploadService, QuizCraft.Infrastructure.Services.FileUploadService>();

// FUNC_ConfigurarGeneracionFlashcards: Servicios para generación automática de flashcards
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IFlashcardGenerationService, QuizCraft.Infrastructure.Services.DocumentProcessing.FlashcardGenerationService>();
builder.Services.AddScoped<QuizCraft.Application.Interfaces.ITraditionalDocumentProcessor, QuizCraft.Infrastructure.Services.DocumentProcessing.TraditionalDocumentProcessor>();
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IDocumentTextExtractor, QuizCraft.Infrastructure.Services.DocumentTextExtractor>();

// FUNC_ConfigurarAlgoritmoRepaso: Servicios para algoritmo de repetición espaciada
builder.Services.AddScoped<IAlgoritmoRepasoService, QuizCraft.Infrastructure.Services.AlgoritmoRepasoService>();

// FUNC_ConfigurarGemini: Configuración de servicios de Google Gemini para generación con IA
builder.Services.Configure<QuizCraft.Application.Models.GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IAIConfigurationService, QuizCraft.Infrastructure.Services.GeminiConfigurationService>();

// Registrar HttpClient para GeminiService
builder.Services.AddHttpClient<QuizCraft.Infrastructure.Services.GeminiService>();

// Decidir qué servicio usar basado en la configuración de Gemini
var geminiSettings = builder.Configuration.GetSection("Gemini").Get<QuizCraft.Application.Models.GeminiSettings>();
if (geminiSettings == null || 
    string.IsNullOrEmpty(geminiSettings.ApiKey) || 
    geminiSettings.ApiKey == "TU_CLAVE_GEMINI_AQUI")
{
    // Usar servicio mock cuando no hay configuración válida de Gemini
    builder.Services.AddScoped<QuizCraft.Application.Interfaces.IAIService, QuizCraft.Infrastructure.Services.MockAIService>();
    builder.Services.AddLogging(loggingBuilder => 
        loggingBuilder.AddConsole().AddDebug());
    Console.WriteLine("⚠️  Usando servicio mock de IA - Gemini no configurado");
}
else
{
    // Usar servicio real de Gemini cuando hay configuración válida
    builder.Services.AddScoped<QuizCraft.Application.Interfaces.IAIService, QuizCraft.Infrastructure.Services.GeminiService>();
    Console.WriteLine($"✅ Usando Google Gemini - Modelo: {geminiSettings.Model}");
}

builder.Services.AddScoped<QuizCraft.Application.Interfaces.IAIDocumentProcessor, QuizCraft.Infrastructure.Services.AIDocumentProcessor>();

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
    // IMPORTANTE: Cambiar esta contraseña en producción
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";
    
    // Buscar si ya existe el usuario administrador
    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    
    if (existingAdmin == null)
    {
        // Si no existe, crear nuevo usuario
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
    else
    {
        // Si existe pero no tiene contraseña (viene de migración), asignar contraseña
        if (string.IsNullOrEmpty(existingAdmin.PasswordHash))
        {
            await userManager.AddPasswordAsync(existingAdmin, adminPassword);
        }
        
        // Asegurar que tiene el rol de administrador
        if (!await userManager.IsInRoleAsync(existingAdmin, "Administrador"))
        {
            await userManager.AddToRoleAsync(existingAdmin, "Administrador");
        }
    }
}
