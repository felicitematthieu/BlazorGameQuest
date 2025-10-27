using System.ComponentModel.DataAnnotations;

namespace ASTA.SharedModels;

public class Player
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string UserName { get; set; } = default!;
    [Range(1,100)] public int Level { get; set; } = 1;
}

public class Admin
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Login { get; set; } = default!;
}

public class Dungeon
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Name { get; set; } = default!;
    public List<Room> Rooms { get; set; } = [];
}

public class Room
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Name { get; set; } = default!;
    public int DungeonId { get; set; }
}
