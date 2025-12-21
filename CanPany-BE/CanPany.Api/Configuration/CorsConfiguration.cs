namespace CanPany.Api.Configuration;

public static class CorsConfiguration
{
    public const string CorsPolicy = "CanPanyCors";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
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
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))
            );
        });

        return services;
    }
}

