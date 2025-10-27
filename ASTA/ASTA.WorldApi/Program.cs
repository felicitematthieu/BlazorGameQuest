using ASTA.WorldApi;                 // <= son propre namespace
using ASTA.SharedModels;            
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


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

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));


var dungeons = app.MapGroup("/dungeons");


dungeons.MapGet("/", async (AstaDbContext db)
    => await db.Dungeons.Include(d => d.Rooms).AsNoTracking().ToListAsync());


dungeons.MapGet("/{id:int}", async (int id, AstaDbContext db)
    => await db.Dungeons.Include(d => d.Rooms).AsNoTracking().FirstOrDefaultAsync(d => d.Id == id) is { } d
        ? Results.Ok(d) : Results.NotFound());


dungeons.MapPost("/", async (Dungeon d, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(d);
    if (!ok) return Results.ValidationProblem(errors);

    db.Dungeons.Add(d);
    await db.SaveChangesAsync();
    return Results.Created($"/dungeons/{d.Id}", d);
});


dungeons.MapPost("/{id:int}/rooms", async (int id, Room r, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(r);
    if (!ok) return Results.ValidationProblem(errors);

    if (await db.Dungeons.FindAsync(id) is null) return Results.NotFound();
    r.DungeonId = id;
    db.Rooms.Add(r);
    await db.SaveChangesAsync();
    return Results.Created($"/dungeons/{id}/rooms/{r.Id}", r);
});


dungeons.MapDelete("/{id:int}", async (int id, AstaDbContext db) =>
{
    var d = await db.Dungeons.FindAsync(id);
    if (d is null) return Results.NotFound();
    db.Remove(d);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AstaDbContext>();
    await Seed.EnsureAsync(db);
}

app.Run();
