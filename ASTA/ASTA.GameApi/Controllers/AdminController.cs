using ASTA.GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTA.GameApi.Controllers;

/// <summary>
/// Contrôleur d'administration pour gérer les joueurs et consulter les statistiques.
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    private readonly PlayerService _playerService;

    public AdminController(AdminService adminService, PlayerService playerService)
    {
        _adminService = adminService;
        _playerService = playerService;
    }

    /// <summary>
    /// Récupère le classement général des joueurs.
    /// </summary>
    /// <param name="top">Nombre de joueurs à afficher (par défaut 100)</param>
    /// <returns>Classement trié par score total décroissant</returns>
    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<object>>> GetLeaderboard([FromQuery] int top = 100)
    {
        var leaderboard = await _adminService.GetLeaderboardAsync(top);
        return Ok(leaderboard);
    }

    /// <summary>
    /// Récupère toutes les aventures avec filtres optionnels.
    /// </summary>
    /// <param name="playerId">Filtrer par joueur</param>
    /// <param name="status">Filtrer par statut (InProgress, Completed, Dead)</param>
    /// <param name="page">Numéro de page (1-based)</param>
    /// <param name="pageSize">Taille de page</param>
    /// <returns>Liste paginée des aventures</returns>
    [HttpGet("adventures")]
    public async Task<ActionResult<object>> GetAllAdventures(
        [FromQuery] int? playerId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetAllAdventuresAsync(playerId, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Active ou désactive un joueur.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <param name="isActive">Nouveau statut (true = actif, false = désactivé)</param>
    /// <returns>200 si succès, 404 si joueur non trouvé</returns>
    [HttpPut("players/{playerId}/status")]
    public async Task<ActionResult> SetPlayerActiveStatus(int playerId, [FromBody] bool isActive)
    {
        var result = await _adminService.SetPlayerActiveStatusAsync(playerId, isActive);
        if (!result) return NotFound(new { error = "Player not found" });
        return Ok(new { message = "Player status updated", playerId, isActive });
    }

    /// <summary>
    /// Exporte la liste des joueurs au format CSV.
    /// </summary>
    /// <returns>Fichier CSV avec les données des joueurs</returns>
    [HttpGet("players/export")]
    public async Task<IActionResult> ExportPlayers()
    {
        var csv = await _adminService.ExportPlayersToCsvAsync();
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"players_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Récupère tous les joueurs (sans pagination pour usage admin).
    /// </summary>
    /// <returns>Liste complète des joueurs</returns>
    [HttpGet("players")]
    public async Task<ActionResult<object>> GetAllPlayers()
    {
        var result = await _playerService.GetPlayersAsync(1, 1000, null);
        return Ok(result);
    }
}
