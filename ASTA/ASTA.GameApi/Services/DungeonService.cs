using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.GameApi.Services;

/// <summary>
/// Service de gestion des donjons (GameApi).
/// </summary>
public class DungeonService
{
    private readonly AstaDbContext _db;

    public DungeonService(AstaDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Récupère tous les donjons avec leurs salles.
    /// </summary>
    /// <returns>Liste des donjons</returns>
    public async Task<List<Dungeon>> GetAllDungeonsAsync()
    {
        return await _db.Dungeons.Include(d => d.Rooms).AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Récupère un donjon par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <returns>Le donjon ou null si non trouvé</returns>
    public async Task<Dungeon?> GetDungeonByIdAsync(int id)
    {
        return await _db.Dungeons.Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <summary>
    /// Crée un nouveau donjon.
    /// </summary>
    /// <param name="dungeon">Donjon à créer</param>
    /// <returns>Le donjon créé</returns>
    public async Task<Dungeon> CreateDungeonAsync(Dungeon dungeon)
    {
        _db.Dungeons.Add(dungeon);
        await _db.SaveChangesAsync();
        return dungeon;
    }

    /// <summary>
    /// Ajoute une salle à un donjon existant.
    /// </summary>
    /// <param name="dungeonId">Identifiant du donjon</param>
    /// <param name="room">Salle à ajouter</param>
    /// <returns>La salle créée ou null si le donjon n'existe pas</returns>
    public async Task<Room?> AddRoomToDungeonAsync(int dungeonId, Room room)
    {
        if (await _db.Dungeons.FindAsync(dungeonId) is null)
            return null;

        room.DungeonId = dungeonId;
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return room;
    }

    /// <summary>
    /// Supprime un donjon.
    /// </summary>
    /// <param name="id">Identifiant du donjon</param>
    /// <returns>True si supprimé, false si non trouvé</returns>
    public async Task<bool> DeleteDungeonAsync(int id)
    {
        var dungeon = await _db.Dungeons.FindAsync(id);
        if (dungeon is null) return false;

        _db.Remove(dungeon);
        await _db.SaveChangesAsync();
        return true;
    }
}
