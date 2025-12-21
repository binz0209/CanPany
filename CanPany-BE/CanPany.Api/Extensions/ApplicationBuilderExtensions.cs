using CanPany.Api.Configuration;
using CanPany.Api.Hubs;

namespace CanPany.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCanPanySwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CanPany API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }

    public static WebApplication UseCanPanyHubs(this WebApplication app)
    {
        app.MapHub<NotificationHub>("/hubs/notification");
        app.MapHub<MessageHub>("/hubs/message");

        return app;
    }
}

