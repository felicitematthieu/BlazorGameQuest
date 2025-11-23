using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.GameApi.Services;

/// <summary>
/// Service de gestion des joueurs.
/// </summary>
public class PlayerService
{
    private readonly AstaDbContext _db;

    public PlayerService(AstaDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Récupère une liste paginée de joueurs avec recherche optionnelle.
    /// </summary>
    /// <param name="page">Numéro de page (1-based)</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="query">Recherche par nom d'utilisateur</param>
    /// <returns>Objet contenant les données et métadonnées de pagination</returns>
    public async Task<object> GetPlayersAsync(int page, int pageSize, string? query)
    {
        var q = _db.Players.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => EF.Functions.ILike(p.UserName, $"%{query}%"));

        var total = await q.CountAsync();
        var data = await q.OrderBy(p => p.Id)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

        return new { total, page, pageSize, data };
    }

    /// <summary>
    /// Récupère un joueur par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>Le joueur ou null si non trouvé</returns>
    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        return await _db.Players.FindAsync(id);
    }

    /// <summary>
    /// Crée un nouveau joueur.
    /// </summary>
    /// <param name="player">Joueur à créer</param>
    /// <returns>Le joueur créé avec son identifiant</returns>
    public async Task<Player> CreatePlayerAsync(Player player)
    {
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return player;
    }

    /// <summary>
    /// Met à jour un joueur existant.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <param name="player">Nouvelles données</param>
    /// <returns>True si mis à jour, false si non trouvé</returns>
    public async Task<bool> UpdatePlayerAsync(int id, Player player)
    {
        var existing = await _db.Players.FindAsync(id);
        if (existing is null) return false;

        existing.UserName = player.UserName;
        existing.Level = player.Level;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Supprime un joueur.
    /// </summary>
    /// <param name="id">Identifiant du joueur</param>
    /// <returns>True si supprimé, false si non trouvé</returns>
    public async Task<bool> DeletePlayerAsync(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is null) return false;

        _db.Remove(player);
        await _db.SaveChangesAsync();
        return true;
    }
}
