using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente une salle dans un donjon.
/// </summary>
public class Room
{
    /// <summary>
    /// Identifiant unique de la salle.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom de la salle.
    /// </summary>
    [Required, MaxLength(128)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Identifiant du donjon parent.
    /// </summary>
    public int DungeonId { get; set; }
}
