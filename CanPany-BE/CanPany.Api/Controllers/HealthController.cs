using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    // 1) Chỉ đọc config thô, KHÔNG inject MongoDbContext ở action này
    [HttpGet("config")]
    public IActionResult Config([FromServices] IOptions<MongoOptions> opt, [FromServices] IConfiguration cfg)
    {
        var o = opt.Value;
        return Ok(new
        {
            ok = true,
            env = cfg["ASPNETCORE_ENVIRONMENT"],
            mongo = new
            {
                hasConn = !string.IsNullOrWhiteSpace(o.ConnectionString),
                hasDb = !string.IsNullOrWhiteSpace(o.DbName),
                dbName = o.DbName
            },
            jwtKeySet = !string.IsNullOrWhiteSpace(cfg["Jwt:Key"])
        });
    }

    // 2) Kiểm tra Mongo có try/catch – nếu fail, trả 500 nhưng KHÔNG nổ unhandled
    [HttpGet("mongo")]
    public async Task<IActionResult> Mongo([FromServices] MongoDbContext db, [FromServices] IOptions<MongoOptions> opt)
    {
        try
        {
            // Test ping trước
            await db.Database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
            var names = await db.Database.ListCollectionNames().ToListAsync();
            return Ok(new { 
                ok = true, 
                collections = names,
                dbName = opt.Value.DbName,
                connectionStringMasked = opt.Value.ConnectionString?.Substring(0, Math.Min(50, opt.Value.ConnectionString.Length)) + "..."
            });
        }
        catch (MongoDB.Driver.MongoAuthenticationException ex)
        {
            return StatusCode(500, new { 
                ok = false, 
                error = "MongoDB Authentication Failed",
                message = ex.Message,
                details = new[] {
                    "1. Check username and password in connection string",
                    "2. Check IP whitelist in MongoDB Atlas Network Access",
                    "3. Check database user permissions"
                },
                connectionStringMasked = opt.Value.ConnectionString?.Substring(0, Math.Min(50, opt.Value.ConnectionString.Length)) + "..."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                ok = false, 
                error = ex.GetType().Name,
                message = ex.Message,
                connectionStringMasked = opt.Value.ConnectionString?.Substring(0, Math.Min(50, opt.Value.ConnectionString.Length)) + "..."
            });
        }
    }

    // 3) Alias /health -> /health/config cho dễ gọi
    [HttpGet]
    public IActionResult Root([FromServices] IOptions<MongoOptions> opt, [FromServices] IConfiguration cfg)
        => Config(opt, cfg);
}
