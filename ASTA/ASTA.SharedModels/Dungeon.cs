using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente un donjon contenant plusieurs salles.
/// </summary>
public class Dungeon
{
    /// <summary>
    /// Identifiant unique du donjon.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom du donjon.
    /// </summary>
    [Required, MaxLength(128)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Liste des salles du donjon.
    /// </summary>
    public List<Room> Rooms { get; set; } = [];
}
