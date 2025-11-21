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
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Configuración de bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configuración de confirmación de email (deshabilitado para desarrollo)
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<QuizCraft.Web.Services.SpanishIdentityErrorDescriber>()
.AddSignInManager<SignInManager<ApplicationUser>>();

// FUNC_ConfigurarRepositorios: Inyección de dependencias para repositorios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizCompartidoRepository, QuizCompartidoRepository>();
builder.Services.AddScoped<IEstadisticaEstudioRepository, EstadisticaEstudioRepository>();
builder.Services.AddScoped<IResultadoQuizRepository, ResultadoQuizRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// FUNC_ConfigurarServicios: Inyección de dependencias para servicios de aplicación
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IFileUploadService, QuizCraft.Infrastructure.Services.FileUploadService>();

// FUNC_ConfigurarServiciosCompartir: Servicio para compartir e importar quizzes
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IQuizCompartidoService, QuizCraft.Infrastructure.Services.QuizCompartidoService>();

// FUNC_ConfigurarGeneracionFlashcards: Servicios para generación automática de flashcards
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IFlashcardGenerationService, QuizCraft.Infrastructure.Services.DocumentProcessing.FlashcardGenerationService>();
builder.Services.AddScoped<QuizCraft.Application.Interfaces.ITraditionalDocumentProcessor, QuizCraft.Infrastructure.Services.DocumentProcessing.TraditionalDocumentProcessor>();
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IDocumentTextExtractor, QuizCraft.Infrastructure.Services.DocumentTextExtractor>();

// FUNC_ConfigurarAlgoritmoRepaso: Servicios para algoritmo de repetición espaciada
builder.Services.AddScoped<IAlgoritmoRepasoService, QuizCraft.Infrastructure.Services.AlgoritmoRepasoService>();

// FUNC_ConfigurarRepasosProgramados: Servicio para gestión de repasos programados
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IRepasoProgramadoService, QuizCraft.Infrastructure.Services.RepasoProgramadoService>();

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

// FUNC_ConfigurarGeneracionQuizzes: Servicio para generación automática de quizzes con IA
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IQuizGenerationService, QuizCraft.Infrastructure.Services.QuizGeneration.QuizGenerationService>();

// FUNC_ConfigurarEstadisticas: Servicio para análisis y estadísticas de desempeño
builder.Services.AddScoped<QuizCraft.Application.Interfaces.IStatisticsService, QuizCraft.Infrastructure.Services.StatisticsService>();

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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogWarning("=================================================");
        logger.LogWarning("INICIO DE INICIALIZACIÓN DE BASE DE DATOS");
        logger.LogWarning("=================================================");
        
        logger.LogInformation("Iniciando migraciones de base de datos...");
        // Aplicar migraciones pendientes
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones completadas exitosamente");
        
        logger.LogInformation("Inicializando roles del sistema...");
        // Inicializar roles básicos
        await FUNC_InicializarRoles(roleManager, logger);
        logger.LogInformation("Roles inicializados correctamente");
        
        logger.LogWarning("==> VERIFICANDO USUARIO ADMINISTRADOR...");
        // Crear usuario administrador si no existe
        await FUNC_CrearUsuarioAdministrador(userManager, logger);
        logger.LogWarning("==> VERIFICACIÓN DE USUARIO ADMINISTRADOR COMPLETADA");
        
        logger.LogWarning("=================================================");
        logger.LogWarning("INICIALIZACIÓN COMPLETADA CON ÉXITO");
        logger.LogWarning("=================================================");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "❌❌❌ ERROR CRÍTICO durante la inicialización de la base de datos ❌❌❌");
        logger.LogCritical($"Mensaje: {ex.Message}");
        logger.LogCritical($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            logger.LogCritical($"InnerException: {ex.InnerException.Message}");
        }
        // NO lanzar la excepción para que la app siga arrancando
        Console.WriteLine($"\n\n❌❌❌ ERROR FATAL: {ex.Message}\n\n");
    }
}

app.Run();

// FUNC_InicializarRoles: Crear roles básicos del sistema
static async Task FUNC_InicializarRoles(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    string[] roles = { "Administrador", "Profesor", "Estudiante" };
    
    foreach (var role in roles)
    {
        try
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation($"✅ Rol '{role}' creado exitosamente");
                }
                else
                {
                    logger.LogError($"❌ Error al crear rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"ℹ️  Rol '{role}' ya existe");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"❌ Excepción al crear/verificar rol '{role}'");
        }
    }
}

// FUNC_CrearUsuarioAdministrador: Crear usuario administrador por defecto
static async Task FUNC_CrearUsuarioAdministrador(UserManager<ApplicationUser> userManager, ILogger logger)
{
    const string adminEmail = "admin@quizcraft.com";
    // IMPORTANTE: Cambiar esta contraseña en producción
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";
    
    logger.LogWarning($">>> Iniciando verificación de usuario administrador: {adminEmail}");
    Console.WriteLine($"\n>>> VERIFICANDO ADMIN: {adminEmail}\n");
    
    try
    {
        // Buscar si ya existe el usuario administrador
        logger.LogWarning(">>> Buscando usuario administrador en la base de datos...");
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        
        if (existingAdmin == null)
        {
            logger.LogWarning($">>> Usuario administrador NO encontrado. Procediendo a crear...");
            Console.WriteLine($">>> CREANDO NUEVO USUARIO ADMIN\n");
            // Si no existe, crear nuevo usuario
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Nombre = "Admin",
                Apellido = "QuizCraft",
                NombreCompleto = "Admin QuizCraft",
                FechaRegistro = DateTime.UtcNow,
                PreferenciaIdioma = "es",
                EstaActivo = true,
                NotificacionesEmail = true,
                NotificacionesWeb = true,
                NotificacionesHabilitadas = true,
                TemaPreferido = "dark"
            };
            
            logger.LogWarning($">>> Intentando crear usuario con email: {adminUser.Email}");
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                logger.LogWarning($">>> ✅ Usuario creado exitosamente. Asignando rol...");
                await userManager.AddToRoleAsync(adminUser, "Administrador");
                logger.LogWarning($"✅✅✅ Usuario administrador creado y configurado: {adminEmail}");
                Console.WriteLine($"\n✅✅✅ ADMIN CREADO: {adminEmail}\n");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogCritical($"❌❌❌ Error al crear usuario administrador: {errors}");
                Console.WriteLine($"\n❌❌❌ ERROR CREANDO ADMIN: {errors}\n");
            }
        }
        else
        {
            logger.LogWarning($">>> ℹ️ Usuario administrador YA EXISTE: {existingAdmin.Email}");
            Console.WriteLine($"\n>>> ADMIN YA EXISTE: {existingAdmin.Email}\n");
            
            // Si existe pero no tiene contraseña (viene de migración), asignar contraseña
            if (string.IsNullOrEmpty(existingAdmin.PasswordHash))
            {
                logger.LogWarning(">>> Usuario sin contraseña detectado. Asignando contraseña...");
                await userManager.AddPasswordAsync(existingAdmin, adminPassword);
                logger.LogWarning("✅ Contraseña asignada al usuario administrador existente");
                Console.WriteLine("✅ CONTRASEÑA ASIGNADA\n");
            }
            
            // Asegurar que tiene el rol de administrador
            if (!await userManager.IsInRoleAsync(existingAdmin, "Administrador"))
            {
                logger.LogWarning(">>> Asignando rol Administrador al usuario existente...");
                await userManager.AddToRoleAsync(existingAdmin, "Administrador");
                logger.LogWarning("✅ Rol Administrador asignado al usuario existente");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "❌❌❌ EXCEPCIÓN CRÍTICA al crear usuario administrador");
        Console.WriteLine($"\n❌❌❌ EXCEPCIÓN: {ex.Message}\n{ex.StackTrace}\n");
    }
}
