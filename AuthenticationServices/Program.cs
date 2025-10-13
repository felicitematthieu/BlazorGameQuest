using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Controllers ([ApiController], [Route], etc.)
builder.Services.AddControllers();

// OpenAPI (optionnel)
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Auth API v1"));
}

// Expose les contrôleurs (incluant /api/health via HealthController)
app.MapControllers();

app.Run();

// Pour tests d'intégration
public partial class Program { }
