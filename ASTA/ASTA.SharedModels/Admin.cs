using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

/// <summary>
/// Représente un administrateur du système.
/// </summary>
public class Admin
{
    /// <summary>
    /// Identifiant unique de l'administrateur.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Login de l'administrateur.
    /// </summary>
    [Required, MaxLength(64)]
    public string Login { get; set; } = default!;
}
