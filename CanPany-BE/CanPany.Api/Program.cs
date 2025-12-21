// ===== Usings =====
using CanPany.Api.Hubs;
using CanPany.Api.Services;
using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;             // UserService, v.v.
using CanPany.Domain.Entities;
using CanPany.Infrastructure.Data;
using CanPany.Infrastructure.Initialization;
using CanPany.Infrastructure.Repositories;
using CanPany.Infrastructure.Services;          // JwtTokenService, CloudinaryImageUploadService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

Console.WriteLine("==== App starting... ====");
Console.WriteLine($"MongoDb:DbName = {config["MongoDb:DbName"]}");
Console.WriteLine($"MongoDb:ConnectionString = {(string.IsNullOrEmpty(config["MongoDb:ConnectionString"]) ? "NULL" : "FOUND")}");
Console.WriteLine("=========================");

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
// Chỉ đăng ký 1 IRealtimeService - SignalRRealtimeService có logging tốt hơn
builder.Services.AddScoped<IRealtimeService, SignalRRealtimeService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();   // 👈 bắt buộc để log ra stdout/stderr cho az webapp log tail

// ========== Mongo Options + DbContext ==========
services.Configure<MongoOptions>(config.GetSection("MongoDb"));
services.AddSingleton<MongoDbContext>();

// Initializer (tạo DB/collection/index khi app start)
services.AddSingleton<IMongoInitializer, MongoInitializer>();
services.AddHostedService<MongoInitializerHostedService>();

// ========== Controllers ==========
services.AddControllers(options =>
{
    // Add global exception filter
    options.Filters.Add<CanPany.Api.Filters.ApiExceptionFilter>();
});

// ========== Swagger ==========
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CanPany API", Version = "v1" });

    // 👇 DÙNG FULL NAME để tránh trùng (loại bỏ dấu '+'' của nested type)
    c.CustomSchemaIds(t =>
    {
        if (t.FullName == null)
            return t.Name;
        return t.FullName.Replace('+', '.');
    });

    // Ignore obsolete properties
    c.IgnoreObsoleteProperties();
    
    // Map types to avoid schema generation errors
    c.MapType<System.DateTime>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date-time"
    });
    
    c.MapType<System.DateTimeOffset>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date-time"
    });
    
    // Configure Swagger to handle file uploads (IFormFile)
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    
    // Add operation filter to handle file uploads properly
    // This filter converts IFormFile parameters to multipart/form-data request body
    c.OperationFilter<CanPany.Api.Filters.FileUploadOperationFilter>();

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});


// ========== CORS (FE Vite) ==========
const string CorsPolicy = "CanPanyCors";
services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "https://localhost:5173",
            "https://127.0.0.1:5173",
            "http://localhost:5174",
            "http://127.0.0.1:5174",
            "https://localhost:5174",
            "https://127.0.0.1:5174",
            "https://CanPany.vercel.app",
            "https://CanPany-b8pdczhsq-binzdapoet0209-7462s-projects.vercel.app",
            "https://CanPany-n9nw1kgam-binzdapoet0209-7462s-projects.vercel.app",
            "https://CanPany-1egppvfb7-binzdapoet0209-7462s-projects.vercel.app",
            "https://CanPany-fe-def2eaaah7cjbbc4.malaysiawest-01.azurewebsites.net"
        // Nếu dùng IPv6 trên Windows:
        // "http://[::1]:5173", "https://[::1]:5173"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)) // Cache preflight 24h
    );
});

// ========== JWT Authentication ==========
var jwtKey = config["Jwt:Key"];
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // dev
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                RoleClaimType = ClaimTypes.Role
            };
        });
}
services.AddAuthorization();

// ========== Repositories ==========
services.AddScoped<IUserRepository>(sp =>
    new UserRepository(sp.GetRequiredService<MongoDbContext>().Users));
services.AddScoped<IUserProfileRepository>(sp =>
    new UserProfileRepository(sp.GetRequiredService<MongoDbContext>().UserProfiles));
services.AddScoped<ICategoryRepository>(sp =>
    new CategoryRepository(sp.GetRequiredService<MongoDbContext>().Categories));
services.AddScoped<ISkillRepository>(sp =>
    new SkillRepository(sp.GetRequiredService<MongoDbContext>().Skills));
services.AddScoped<IProjectRepository>(sp =>
    new ProjectRepository(sp.GetRequiredService<MongoDbContext>().Projects));
services.AddScoped<IProjectSkillRepository>(sp =>
    new ProjectSkillRepository(sp.GetRequiredService<MongoDbContext>().ProjectSkills));
services.AddScoped<IProposalRepository>(sp =>
    new ProposalRepository(sp.GetRequiredService<MongoDbContext>().Proposals));
services.AddScoped<IContractRepository>(sp =>
    new ContractRepository(sp.GetRequiredService<MongoDbContext>().Contracts));
services.AddScoped<IPaymentRepository>(sp =>
    new PaymentRepository(sp.GetRequiredService<MongoDbContext>().Payments));
services.AddScoped<IMessageRepository>(sp =>
    new MessageRepository(sp.GetRequiredService<MongoDbContext>().Messages));
services.AddScoped<INotificationRepository>(sp =>
    new NotificationRepository(sp.GetRequiredService<MongoDbContext>().Notifications));
services.AddScoped<IReviewRepository>(sp =>
    new ReviewRepository(sp.GetRequiredService<MongoDbContext>().Reviews));
services.AddScoped<IWalletRepository>(sp =>
    new WalletRepository(sp.GetRequiredService<MongoDbContext>().Wallets));
services.AddScoped<IWalletTransactionRepository>(sp =>
    new WalletTransactionRepository(sp.GetRequiredService<MongoDbContext>().WalletTransactions));
services.AddScoped<IUserSettingsRepository>(sp =>
    new UserSettingsRepository(sp.GetRequiredService<MongoDbContext>().UserSettings));
services.AddScoped<IBannerRepository>(sp =>
    new BannerRepository(sp.GetRequiredService<MongoDbContext>().Banners));

// ========== APRS - New Repositories ==========
services.AddScoped<ICompanyRepository>(sp =>
    new CompanyRepository(sp.GetRequiredService<MongoDbContext>().Companies));
services.AddScoped<IJobRepository>(sp =>
    new JobRepository(sp.GetRequiredService<MongoDbContext>().Jobs));
services.AddScoped<ICVRepository>(sp =>
    new CVRepository(sp.GetRequiredService<MongoDbContext>().CVs));
services.AddScoped<IApplicationRepository>(sp =>
    new ApplicationRepository(sp.GetRequiredService<MongoDbContext>().Applications));

// ========== Audit Logging ==========
services.AddScoped<IAuditLogRepository>(sp =>
    new AuditLogRepository(sp.GetRequiredService<MongoDbContext>().AuditLogs));
services.AddScoped<IAuditService, AuditService>();

// ========== I18N / Localization Services ==========
services.AddHttpContextAccessor(); // Required for LocalizationService
services.AddScoped<II18nService, CanPany.Application.Services.I18nService>();
services.AddScoped<ILocalizationService, CanPany.Infrastructure.Localization.LocalizationService>();

// ========== Services (Application) ==========
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserProfileService, UserProfileService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<ISkillService, SkillService>();
services.AddScoped<IProjectService, ProjectService>();
services.AddScoped<IProjectSkillService, ProjectSkillService>();
services.AddScoped<IProposalService, ProposalService>();
services.AddScoped<IContractService, ContractService>();
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IMessageService, MessageService>();
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IReviewService, ReviewService>();
services.AddSingleton<IJwtTokenService, JwtTokenService>();
services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IWalletService, WalletService>();
services.AddScoped<IBannerService, BannerService>();

// ========== APRS - New Services ==========
services.AddScoped<ICompanyService, CompanyService>();
services.AddScoped<IJobService, JobService>();
services.AddScoped<ICVService, CVService>();

// ========== Cloudinary Image Upload ==========
services.Configure<CloudinaryOptions>(config.GetSection("Cloudinary"));
services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
services.AddScoped<IUserSettingsService, UserSettingsService>();

// ========== Gemini & Vector Services ==========
services.AddHttpClient<IGeminiService, GeminiService>();
services.AddScoped<IVectorService, VectorService>();

// ========== Email Service ==========
services.AddSingleton<CanPany.Infrastructure.Services.EmailService>(sp =>
{
    var smtpHost = config["Email:SmtpHost"] ?? "smtp.gmail.com";
    var smtpPort = int.Parse(config["Email:SmtpPort"] ?? "587");
    var fromEmail = config["Email:FromEmail"] ?? "";
    var fromName = config["Email:FromName"] ?? "CanPany";
    var password = config["Email:Password"] ?? "";
    return new CanPany.Infrastructure.Services.EmailService(smtpHost, smtpPort, fromEmail, fromName, password);
});
services.AddScoped<IEmailService, EmailServiceAdapter>();

// ========== Redis Background Jobs ==========
services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var connectionString = config.GetConnectionString("Redis") ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
});

// Register Background Job Service
services.AddScoped<IBackgroundJobService, CanPany.Infrastructure.Services.RedisBackgroundJobService>();

// Register Job Handlers
services.AddScoped<CanPany.Infrastructure.Services.SendEmailJobHandler>();
services.AddScoped<CanPany.Infrastructure.Services.ProcessPaymentJobHandler>();
services.AddScoped<CanPany.Infrastructure.Services.GenerateReportJobHandler>();

// Register Background Worker (có thể chạy nhiều workers)
var workerCount = config.GetValue<int>("BackgroundJobs:WorkerCount", 2);
for (int i = 0; i < workerCount; i++)
{
    services.AddHostedService<CanPany.Infrastructure.Services.BackgroundJobWorker>();
}

var app = builder.Build();

// ========== Language Detection ==========
// Language Detection Middleware - Phải đặt TRƯỚC các middleware khác để set language cho I18N
app.UseMiddleware<CanPany.Api.Middlewares.LanguageDetectionMiddleware>();

// ========== Global Exception Handling ==========
// Global Exception Handler Middleware - Đảm bảo không có exception nào làm hệ thống dừng
app.UseMiddleware<CanPany.Api.Middlewares.GlobalExceptionHandlerMiddleware>();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
        // ghi log ra console để xem bằng "az webapp log tail"
        Console.Error.WriteLine($"[ERR] {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/500",
            title = "Server Error",
            status = 500,
            detail = ex?.Message,
            traceId = context.TraceIdentifier
        });
    });
});

app.UseExceptionHandler("/__error");
app.Map("/__error", (HttpContext http, ILoggerFactory lf) =>
{
    var ex = http.Features.Get<IExceptionHandlerFeature>()?.Error;

    // Unwrap toàn bộ inner exceptions
    var msgs = new List<string>();
    for (var e = ex; e != null; e = e.InnerException) msgs.Add(e.GetType().FullName + ": " + e.Message);

    lf.CreateLogger("Global").LogError(ex, "Unhandled exception");

    return Results.Problem(
        title: "Server error",
        detail: string.Join(" => ", msgs),  // <<< xem nguyên nhân thật ở đây
        statusCode: StatusCodes.Status500InternalServerError);
});


// ========== Middlewares ==========
// CORS PHẢI được đặt ở đầu pipeline để xử lý preflight OPTIONS requests
app.UseCors(CorsPolicy);

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CanPany API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.EnableValidator();
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls; // IUrlCollection
    Console.WriteLine("==== CanPany API is running on: ====");
    foreach (var url in urls)
    {
        Console.WriteLine(url);
    }
    Console.WriteLine("=====================================");
});

// Tắt HTTPS redirection trong development để tránh ảnh hưởng đến CORS preflight
// app.UseHttpsRedirection();

// ========== Audit Logging Middleware ==========
app.UseMiddleware<CanPany.Api.Middlewares.AuditLoggingMiddleware>();

if (!string.IsNullOrWhiteSpace(jwtKey))
{
    app.UseAuthentication();
}
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<MessageHub>("/hubs/message");

app.Run();