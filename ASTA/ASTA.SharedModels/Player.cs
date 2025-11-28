using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente un joueur du jeu d'aventure.
/// </summary>
public class Player
{
    /// <summary>
    /// Identifiant unique du joueur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom d'utilisateur du joueur (unique).
    /// </summary>
    [Required, MaxLength(64)]
    public string UserName { get; set; } = default!;

    /// <summary>
    /// Niveau actuel du joueur (1-100).
    /// </summary>
    [Range(1, 100)]
    public int Level { get; set; } = 1;

    /// <summary>
    /// Indique si le compte du joueur est actif.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
