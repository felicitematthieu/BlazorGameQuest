using ASTA.GameApi;                     // AstaDbContext, Seed, ValidationUtil, SwaggerExamples
using ASTA.SharedModels;                // Player, Dungeon, Room
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- DB ---
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


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SchemaFilter<SwaggerExamples>();   
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:5173")
     .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));


var players = app.MapGroup("/players");

players.MapGet("/", async (AstaDbContext db, int page = 1, int pageSize = 20, string? q = null) =>
{
    var query = db.Players.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(p => EF.Functions.ILike(p.UserName, $"%{q}%"));

    var total = await query.CountAsync();
    var data = await query.OrderBy(p => p.Id)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

    return Results.Ok(new { total, page, pageSize, data });
});

players.MapGet("/{id:int}", async Task<Results<Ok<Player>, NotFound>> (int id, AstaDbContext db)
    => await db.Players.FindAsync(id) is { } p ? TypedResults.Ok(p) : TypedResults.NotFound());

players.MapPost("/", async Task<Results<Created<Player>, ValidationProblem>> (Player p, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(p);
    if (!ok) return TypedResults.ValidationProblem(errors);

    db.Players.Add(p);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/players/{p.Id}", p);
});

players.MapPut("/{id:int}", async Task<Results<NoContent, NotFound, ValidationProblem>> (int id, Player input, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(input);
    if (!ok) return TypedResults.ValidationProblem(errors);

    var p = await db.Players.FindAsync(id);
    if (p is null) return TypedResults.NotFound();

    p.UserName = input.UserName;
    p.Level = input.Level;
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});

players.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AstaDbContext db) =>
{
    var p = await db.Players.FindAsync(id);
    if (p is null) return TypedResults.NotFound();

    db.Remove(p);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});


var dungeons = app.MapGroup("/dungeons");

dungeons.MapGet("/", async (AstaDbContext db)
    => await db.Dungeons.Include(d => d.Rooms).AsNoTracking().ToListAsync());

dungeons.MapGet("/{id:int}", async Task<Results<Ok<Dungeon>, NotFound>> (int id, AstaDbContext db)
    => await db.Dungeons.Include(d => d.Rooms).FirstOrDefaultAsync(d => d.Id == id) is { } d
        ? TypedResults.Ok(d) : TypedResults.NotFound());

dungeons.MapPost("/", async Task<Results<Created<Dungeon>, ValidationProblem>> (Dungeon d, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(d);
    if (!ok) return TypedResults.ValidationProblem(errors);

    db.Dungeons.Add(d);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/dungeons/{d.Id}", d);
});

dungeons.MapPost("/{id:int}/rooms", async Task<Results<Created<Room>, NotFound, ValidationProblem>> (int id, Room r, AstaDbContext db) =>
{
    var (ok, errors) = ValidationUtil.Validate(r);
    if (!ok) return TypedResults.ValidationProblem(errors);

    if (await db.Dungeons.FindAsync(id) is null) return TypedResults.NotFound();

    r.DungeonId = id;
    db.Rooms.Add(r);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/dungeons/{id}/rooms/{r.Id}", r);
});

dungeons.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AstaDbContext db) =>
{
    var d = await db.Dungeons.FindAsync(id);
    if (d is null) return TypedResults.NotFound();

    db.Remove(d);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AstaDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
    await Seed.EnsureAsync(db, log);
}

app.Run();
