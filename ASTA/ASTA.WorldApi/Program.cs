using ASTA.WorldApi;
using ASTA.WorldApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration de la base de données ---
var useInMemory = builder.Configuration.GetValue("UseInMemory", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AstaDbContext>(o => o.UseInMemoryDatabase("asta-world"));
}
else
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("Connection string 'Default' missing");
    builder.Services.AddDbContext<AstaDbContext>(o => o.UseNpgsql(cs));
}

// --- Enregistrement des services métier ---
builder.Services.AddScoped<DungeonService>();

// --- Configuration des controllers ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SchemaFilter<SwaggerExamples>(); 
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:5173")
     .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- Configuration du pipeline HTTP ---
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// --- Redirection racine vers Swagger ---
app.MapGet("/", () => Results.Redirect("/swagger"));

// --- Mapping des controllers ---
app.MapControllers();

// --- Initialisation de la base de données ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AstaDbContext>();
    await Seed.EnsureAsync(db);
}

app.Run();

// Exposition pour les tests d'intégration (WebApplicationFactory)
public partial class Program { }

