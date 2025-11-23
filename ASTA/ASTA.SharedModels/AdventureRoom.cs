using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente une salle dans une aventure jouée.
/// </summary>
public class AdventureRoom
{
    /// <summary>
    /// Identifiant unique de la salle d'aventure.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifiant de l'aventure parente.
    /// </summary>
    public int AdventureId { get; set; }

    /// <summary>
    /// Indice de séquence de la salle dans l'aventure (0, 1, 2...).
    /// </summary>
    public int SequenceIndex { get; set; }

    /// <summary>
    /// Titre de la salle.
    /// </summary>
    [Required, MaxLength(128)]
    public string RoomTitle { get; set; } = default!;

    /// <summary>
    /// Type de salle: Enemy, Treasure, Trap.
    /// </summary>
    [MaxLength(20)]
    public string RoomType { get; set; } = "Enemy";

    /// <summary>
    /// Description de la salle présentée au joueur.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Choix effectué par le joueur (null si pas encore joué).
    /// </summary>
    [MaxLength(50)]
    public string? Choice { get; set; }

    /// <summary>
    /// Points gagnés ou perdus lors de cette salle.
    /// </summary>
    public int ScoreDelta { get; set; } = 0;
}
