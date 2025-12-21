// CanPany.Api/Program.Refactored.cs
// File này là version refactored của Program.cs
// Sau khi test xong, có thể rename thành Program.cs

using CanPany.Api.Configuration;
using CanPany.Api.Extensions;
using CanPany.Api.Hubs;
using CanPany.Api.Middlewares;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

Console.WriteLine("==== App starting... ====");
Console.WriteLine($"MongoDb:DbName = {config["MongoDb:DbName"]}");
Console.WriteLine($"MongoDb:ConnectionString = {(string.IsNullOrEmpty(config["MongoDb:ConnectionString"]) ? "NULL" : "FOUND")}");
Console.WriteLine("=========================");

// ========== Logging ==========
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ========== SignalR ==========
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// ========== Controllers ==========
services.AddControllers();

// ========== Configuration Extensions ==========
services.AddSwaggerConfiguration();
services.AddCorsConfiguration();
services.AddAuthenticationConfiguration(config);
services.AddInfrastructure(config);
services.AddApplication();

var app = builder.Build();

// ========== Exception Handling ==========
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
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
    var msgs = new List<string>();
    for (var e = ex; e != null; e = e.InnerException) 
        msgs.Add(e.GetType().FullName + ": " + e.Message);

    lf.CreateLogger("Global").LogError(ex, "Unhandled exception");

    return Results.Problem(
        title: "Server error",
        detail: string.Join(" => ", msgs),
        statusCode: StatusCodes.Status500InternalServerError);
});

// ========== Middlewares ==========
app.UseCors(CorsConfiguration.CorsPolicy);
app.UseCanPanySwagger();
app.UseMiddleware<AuditLoggingMiddleware>();

if (!string.IsNullOrWhiteSpace(config["Jwt:Key"]))
{
    app.UseAuthentication();
}
app.UseAuthorization();

app.MapControllers();
app.UseCanPanyHubs();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls;
    Console.WriteLine("==== CanPany API is running on: ====");
    foreach (var url in urls)
    {
        Console.WriteLine(url);
    }
    Console.WriteLine("=====================================");
});

app.Run();

