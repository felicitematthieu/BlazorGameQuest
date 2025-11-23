using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente une partie d'aventure jouée par un joueur.
/// </summary>
public class Adventure
{
    /// <summary>
    /// Identifiant unique de l'aventure.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifiant du joueur (nullable pour parties non liées).
    /// </summary>
    public int? PlayerId { get; set; }

    /// <summary>
    /// Navigation vers le joueur.
    /// </summary>
    public Player? Player { get; set; }

    /// <summary>
    /// Date et heure de début de l'aventure.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date et heure de fin de l'aventure.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Score total accumulé pendant l'aventure.
    /// </summary>
    public int TotalScore { get; set; } = 0;

    /// <summary>
    /// Statut de l'aventure: InProgress, Completed, Dead.
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "InProgress";

    /// <summary>
    /// Liste des salles de l'aventure.
    /// </summary>
    public List<AdventureRoom> Rooms { get; set; } = [];
}
