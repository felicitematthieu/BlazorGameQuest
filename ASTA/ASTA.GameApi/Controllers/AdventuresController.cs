using ASTA.GameApi.Services;
using ASTA.SharedModels;
using Microsoft.AspNetCore.Mvc;

namespace ASTA.GameApi.Controllers;

/// <summary>
/// Contrôleur pour la gestion des aventures.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdventuresController : ControllerBase
{
    private readonly AdventureService _adventureService;

    public AdventuresController(AdventureService adventureService)
    {
        _adventureService = adventureService;
    }

    /// <summary>
    /// Démarre une nouvelle aventure.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur (optionnel)</param>
    /// <returns>Détails de l'aventure et première salle</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> StartAdventure([FromQuery] int? playerId = null)
    {
        var result = await _adventureService.StartNewAdventureAsync(playerId);
        return Ok(result);
    }

    /// <summary>
    /// Soumet le choix du joueur pour la salle courante.
    /// </summary>
    /// <param name="id">Identifiant de l'aventure</param>
    /// <param name="request">Choix du joueur</param>
    /// <returns>Résultat du choix et prochaine salle</returns>
    [HttpPost("{id:int}/choices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> SubmitChoice(int id, [FromBody] ChoiceRequest request)
    {
        var result = await _adventureService.SubmitChoiceAsync(id, request.Choice);
        if (result is null) return NotFound();
        
        // Vérifier si c'est une erreur métier
        if (result.GetType().GetProperty("error") != null)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Récupère une aventure complète par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'aventure</param>
    /// <returns>L'aventure complète</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Adventure>> GetAdventure(int id)
    {
        var adventure = await _adventureService.GetAdventureByIdAsync(id);
        if (adventure is null) return NotFound();
        return Ok(adventure);
    }

    /// <summary>
    /// Récupère toutes les aventures d'un joueur.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <returns>Liste des aventures du joueur</returns>
    [HttpGet("player/{playerId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Adventure>>> GetPlayerAdventures(int playerId)
    {
        var adventures = await _adventureService.GetAdventuresByPlayerAsync(playerId);
        return Ok(adventures);
    }
}

/// <summary>
/// DTO pour la soumission d'un choix.
/// </summary>
public record ChoiceRequest(string Choice);
