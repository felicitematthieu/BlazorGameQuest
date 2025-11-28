using ASTA.GameApi.Services;
using ASTA.SharedModels;
using Microsoft.AspNetCore.Mvc;

namespace ASTA.GameApi.Controllers;

/// <summary>
/// Contrôleur pour la gestion des joueurs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly PlayerService _playerService;
    private readonly AdminService _adminService;

    public PlayersController(PlayerService playerService, AdminService adminService)
    {
        _playerService = playerService;
        _adminService = adminService;
    }

    /// <summary>
    /// Récupère une liste paginée de joueurs.
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Taille de page (par défaut 20)</param>
    /// <param name="q">Recherche par nom d'utilisateur</param>
    /// <returns>Liste paginée de joueurs</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null)
    {
        var result = await _playerService.GetPlayersAsync(page, pageSize, q);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un joueur par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>Le joueur</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player is null) return NotFound();
        return Ok(player);
    }

    /// <summary>
    /// Crée un nouveau joueur.
    /// </summary>
    /// <param name="player">Joueur à créer</param>
    /// <returns>Le joueur créé</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Player>> CreatePlayer([FromBody] Player player)
    {
        var (isValid, errors) = ValidationUtil.Validate(player);
        if (!isValid) return ValidationProblem(new ValidationProblemDetails(errors));

        var created = await _playerService.CreatePlayerAsync(player);
        return CreatedAtAction(nameof(GetPlayer), new { id = created.Id }, created);
    }

    /// <summary>
    /// Met à jour un joueur existant.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <param name="player">Nouvelles données</param>
    /// <returns>NoContent si succès</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePlayer(int id, [FromBody] Player player)
    {
        var (isValid, errors) = ValidationUtil.Validate(player);
        if (!isValid) return ValidationProblem(new ValidationProblemDetails(errors));

        var updated = await _playerService.UpdatePlayerAsync(id, player);
        if (!updated) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Supprime un joueur.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>NoContent si succès</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePlayer(int id)
    {
        var deleted = await _playerService.DeletePlayerAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Récupère l'historique des aventures d'un joueur.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>Liste des aventures du joueur</returns>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Adventure>>> GetPlayerHistory(int id)
    {
        var history = await _adminService.GetPlayerHistoryAsync(id);
        return Ok(history);
    }
}
