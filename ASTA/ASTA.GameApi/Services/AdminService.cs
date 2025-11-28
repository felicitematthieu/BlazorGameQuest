using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ASTA.GameApi.Services;

/// <summary>
/// Service d'administration pour gérer les joueurs, les aventures et les statistiques.
/// </summary>
public class AdminService
{
    private readonly AstaDbContext _db;

    public AdminService(AstaDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Récupère le classement général des joueurs basé sur leurs scores totaux.
    /// </summary>
    /// <param name="top">Nombre de joueurs à retourner (par défaut 100)</param>
    /// <returns>Liste des joueurs triés par score total décroissant</returns>
    public async Task<List<object>> GetLeaderboardAsync(int top = 100)
    {
        var leaderboard = await _db.Adventures
            .Where(a => a.PlayerId != null && (a.Status == "Completed" || a.Status == "Dead"))
            .GroupBy(a => a.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key!.Value,
                TotalScore = g.Sum(a => a.TotalScore),
                CompletedAdventures = g.Count(a => a.Status == "Completed"),
                TotalAdventures = g.Count(),
                BestScore = g.Max(a => a.TotalScore)
            })
            .OrderByDescending(x => x.TotalScore)
            .Take(top)
            .ToListAsync();

        // Enrichir avec les infos joueurs
        var playerIds = leaderboard.Select(l => l.PlayerId).ToList();
        var players = await _db.Players.Where(p => playerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return leaderboard.Select(l => new
        {
            l.PlayerId,
            PlayerName = players.ContainsKey(l.PlayerId) ? players[l.PlayerId].UserName : "Unknown",
            IsActive = players.ContainsKey(l.PlayerId) ? players[l.PlayerId].IsActive : false,
            l.TotalScore,
            l.CompletedAdventures,
            l.TotalAdventures,
            l.BestScore
        }).Cast<object>().ToList();
    }

    /// <summary>
    /// Récupère toutes les aventures avec filtres optionnels.
    /// </summary>
    /// <param name="playerId">Filtrer par joueur</param>
    /// <param name="status">Filtrer par statut</param>
    /// <param name="page">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <returns>Liste paginée des aventures</returns>
    public async Task<object> GetAllAdventuresAsync(int? playerId = null, string? status = null, int page = 1, int pageSize = 20)
    {
        var query = _db.Adventures.Include(a => a.Rooms).AsNoTracking();

        if (playerId.HasValue)
            query = query.Where(a => a.PlayerId == playerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new { total, page, pageSize, data };
    }

    /// <summary>
    /// Récupère l'historique des aventures d'un joueur.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <returns>Liste des aventures du joueur</returns>
    public async Task<List<Adventure>> GetPlayerHistoryAsync(int playerId)
    {
        return await _db.Adventures
            .Include(a => a.Rooms)
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Active ou désactive un joueur.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <param name="isActive">Nouveau statut</param>
    /// <returns>True si mis à jour, false si joueur non trouvé</returns>
    public async Task<bool> SetPlayerActiveStatusAsync(int playerId, bool isActive)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player is null) return false;

        player.IsActive = isActive;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Exporte la liste des joueurs au format CSV.
    /// </summary>
    /// <returns>Contenu CSV</returns>
    public async Task<string> ExportPlayersToCsvAsync()
    {
        var players = await _db.Players.ToListAsync();
        var adventures = await _db.Adventures
            .Where(a => a.PlayerId != null)
            .GroupBy(a => a.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key!.Value,
                TotalScore = g.Sum(a => a.TotalScore),
                AdventureCount = g.Count()
            })
            .ToListAsync();

        var statsDict = adventures.ToDictionary(a => a.PlayerId);

        var csv = new StringBuilder();
        csv.AppendLine("Id,UserName,Level,IsActive,TotalScore,AdventureCount");

        foreach (var player in players)
        {
            var stats = statsDict.ContainsKey(player.Id) ? statsDict[player.Id] : null;
            csv.AppendLine($"{player.Id},{player.UserName},{player.Level},{player.IsActive}," +
                          $"{stats?.TotalScore ?? 0},{stats?.AdventureCount ?? 0}");
        }

        return csv.ToString();
    }
}
