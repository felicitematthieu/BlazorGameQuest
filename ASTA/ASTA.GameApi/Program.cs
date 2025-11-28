using ASTA.GameApi;
using ASTA.GameApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration de la base de données ---
var useInMemory = builder.Configuration.GetValue("UseInMemory", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AstaDbContext>(o => o.UseInMemoryDatabase("asta"));
}
else
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("Connection string 'Default' missing");
    builder.Services.AddDbContext<AstaDbContext>(o => o.UseNpgsql(cs));
}

// --- Enregistrement des services métier ---
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<DungeonService>();
builder.Services.AddScoped<AdventureService>();
builder.Services.AddScoped<AdminService>();

// --- Configuration des controllers ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SchemaFilter<SwaggerExamples>();   
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:5173", "http://localhost:5109")
     .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();

var app = builder.Build();

// --- Configuration du pipeline HTTP ---
app.UseExceptionHandler();
app.UseStatusCodePages();
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
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
    await Seed.EnsureAsync(db, log);
}

app.Run();

// Exposition pour les tests d'intégration (WebApplicationFactory)
public partial class Program { }

