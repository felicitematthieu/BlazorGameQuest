using ASTA.SharedModels;
using ASTA.WorldApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTA.WorldApi.Controllers;

/// <summary>
/// Contrôleur pour la gestion des donjons (WorldApi).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DungeonsController : ControllerBase
{
    private readonly DungeonService _dungeonService;

    public DungeonsController(DungeonService dungeonService)
    {
        _dungeonService = dungeonService;
    }

    /// <summary>
    /// Récupère tous les donjons avec leurs salles.
    /// </summary>
    /// <returns>Liste des donjons</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Dungeon>>> GetDungeons()
    {
        var dungeons = await _dungeonService.GetAllDungeonsAsync();
        return Ok(dungeons);
    }

    /// <summary>
    /// Récupère un donjon par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <returns>Le donjon</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dungeon>> GetDungeon(int id)
    {
        var dungeon = await _dungeonService.GetDungeonByIdAsync(id);
        if (dungeon is null) return NotFound();
        return Ok(dungeon);
    }

    /// <summary>
    /// Crée un nouveau donjon.
    /// </summary>
    /// <param name="dungeon">Donjon à créer</param>
    /// <returns>Le donjon créé</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dungeon>> CreateDungeon([FromBody] Dungeon dungeon)
    {
        var (isValid, errors) = ValidationUtil.Validate(dungeon);
        if (!isValid) return ValidationProblem(new ValidationProblemDetails(errors));

        var created = await _dungeonService.CreateDungeonAsync(dungeon);
        return CreatedAtAction(nameof(GetDungeon), new { id = created.Id }, created);
    }

    /// <summary>
    /// Ajoute une salle à un donjon.
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <param name="room">Salle à ajouter</param>
    /// <returns>La salle créée</returns>
    [HttpPost("{id:int}/rooms")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Room>> AddRoom(int id, [FromBody] Room room)
    {
        var (isValid, errors) = ValidationUtil.Validate(room);
        if (!isValid) return ValidationProblem(new ValidationProblemDetails(errors));

        var created = await _dungeonService.AddRoomToDungeonAsync(id, room);
        if (created is null) return NotFound();
        return CreatedAtAction(nameof(GetDungeon), new { id }, created);
    }

    /// <summary>
    /// Supprime un donjon.
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <returns>NoContent si succès</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDungeon(int id)
    {
        var deleted = await _dungeonService.DeleteDungeonAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
