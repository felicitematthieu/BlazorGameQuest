using ASTA.SharedModels;

namespace ASTA.GameApi;

public static class AdventureGenerator
{
    private static readonly Random _rnd = new();

    private static readonly List<(string Title, string Type, string Desc)> RoomTemplates =
    [
        ("Entrée sombre", "Enemy", "Un gobelin surgit des ténèbres !"),
        ("Salle au trésor", "Treasure", "Un coffre mystérieux brille dans la pénombre."),
        ("Couloir piégé", "Trap", "Des dalles suspectes... Danger !"),
        ("Crypte abandonnée", "Enemy", "Un squelette vous défie !"),
        ("Bibliothèque ancienne", "Treasure", "Des grimoires précieux..."),
        ("Pont instable", "Trap", "Le pont grince dangereusement."),
        ("Salle du boss", "Enemy", "Un dragon miniature vous attend !"),
        ("Jardin enchanté", "Treasure", "Des herbes rares poussent ici."),
    ];

    public static Adventure GenerateAdventure(int? playerId = null)
    {
        var roomCount = _rnd.Next(2, 6); // 2 à 5 salles
        var adventure = new Adventure
        {
            PlayerId = playerId,
            Status = "InProgress",
            TotalScore = 0
        };

        var picked = RoomTemplates.OrderBy(x => _rnd.Next()).Take(roomCount).ToList();
        for (int i = 0; i < picked.Count; i++)
        {
            var (title, type, desc) = picked[i];
            adventure.Rooms.Add(new AdventureRoom
            {
                SequenceIndex = i,
                RoomTitle = title,
                RoomType = type,
                Description = desc
            });
        }

        return adventure;
    }

    public static int CalculateScoreDelta(string roomType, string choice)
    {
        // Règles simples pour V3
        return (roomType, choice) switch
        {
            ("Enemy", "Combattre") => _rnd.Next(0, 2) == 0 ? 10 : -5, // 50% succès
            ("Enemy", "Fuir") => 2,
            ("Enemy", "Fouiller") => _rnd.Next(-5, 8),
            ("Treasure", "Ouvrir") => _rnd.Next(0, 3) == 0 ? -10 : 15, // 66% succès
            ("Treasure", "Ignorer") => 0,
            ("Treasure", "Fouiller") => _rnd.Next(5, 16),
            ("Trap", "Combattre") => -3,
            ("Trap", "Fuir") => 5,
            ("Trap", "Fouiller") => _rnd.Next(-15, 10),
            _ => 0
        };
    }
}
