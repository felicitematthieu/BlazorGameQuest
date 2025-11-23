using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.GameApi.Services;

/// <summary>
/// Service de gestion des aventures.
/// </summary>
public class AdventureService
{
    private readonly AstaDbContext _db;

    public AdventureService(AstaDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Crée et démarre une nouvelle aventure.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur (nullable)</param>
    /// <returns>Objet contenant l'aventure et la première salle</returns>
    public async Task<object> StartNewAdventureAsync(int? playerId)
    {
        var adventure = AdventureGenerator.GenerateAdventure(playerId);
        _db.Adventures.Add(adventure);
        await _db.SaveChangesAsync();

        var firstRoom = adventure.Rooms.OrderBy(r => r.SequenceIndex).First();
        return new
        {
            AdventureId = adventure.Id,
            TotalRooms = adventure.Rooms.Count,
            CurrentRoom = new
            {
                RoomTitle = firstRoom.RoomTitle,
                RoomType = firstRoom.RoomType,
                Description = firstRoom.Description
            }
        };
    }

    /// <summary>
    /// Soumet le choix du joueur pour la salle courante.
    /// </summary>
    /// <param name="adventureId">Identifiant de l'aventure</param>
    /// <param name="choice">Choix du joueur</param>
    /// <returns>Résultat du choix avec score et prochaine salle</returns>
    public async Task<object?> SubmitChoiceAsync(int adventureId, string choice)
    {
        var adventure = await _db.Adventures.Include(a => a.Rooms)
            .FirstOrDefaultAsync(a => a.Id == adventureId);

        if (adventure is null) return null;
        if (adventure.Status != "InProgress") 
            return new { error = "Adventure already finished" };

        var currentRoom = adventure.Rooms
            .Where(r => r.Choice == null)
            .OrderBy(r => r.SequenceIndex)
            .FirstOrDefault();

        if (currentRoom is null) 
            return new { error = "No room to play" };

        currentRoom.Choice = choice;
        var scoreDelta = AdventureGenerator.CalculateScoreDelta(currentRoom.RoomType, choice);
        currentRoom.ScoreDelta = scoreDelta;
        adventure.TotalScore += scoreDelta;

        var nextRoom = adventure.Rooms
            .Where(r => r.Choice == null)
            .OrderBy(r => r.SequenceIndex)
            .FirstOrDefault();

        if (nextRoom == null || adventure.TotalScore <= 0)
        {
            adventure.Status = adventure.TotalScore <= 0 ? "Dead" : "Completed";
            adventure.EndTime = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new
        {
            NewScore = adventure.TotalScore,
            RoomIndex = currentRoom.SequenceIndex,
            IsComplete = adventure.Status == "Completed",
            IsDead = adventure.Status == "Dead",
            NextRoom = nextRoom == null ? null : new
            {
                RoomTitle = nextRoom.RoomTitle,
                RoomType = nextRoom.RoomType,
                Description = nextRoom.Description
            }
        };
    }

    /// <summary>
    /// Récupère une aventure par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'aventure</param>
    /// <returns>L'aventure complète ou null si non trouvée</returns>
    public async Task<Adventure?> GetAdventureByIdAsync(int id)
    {
        return await _db.Adventures.Include(a => a.Rooms)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Récupère toutes les aventures d'un joueur.
    /// </summary>
    /// <param name="playerId">Identifiant du joueur</param>
    /// <returns>Liste des aventures du joueur</returns>
    public async Task<List<Adventure>> GetAdventuresByPlayerAsync(int playerId)
    {
        return await _db.Adventures
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }
}
