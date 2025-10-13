// FICHIER: SharedModels/Models.cs
// OBJET: Contient des modèles (DTO) simples partagés entre le Client Blazor et les Web APIs.
// V1: On pose les bases minimales sans logique métier (la logique viendra dans les versions suivantes).

namespace SharedModels
{
    /// <summary>
    /// Représente un joueur (compte Keycloak à terme).
    /// V1: on a uniquement des attributs simples pour illustrer le partage de modèles.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Identifiant unique du joueur (GUID sous forme de string pour simplifier).
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Pseudonyme affiché dans l’UI et le classement.
        /// </summary>
        public string Nickname { get; set; } = "Aventurier";

        /// <summary>
        /// Score total cumulé (ex. somme des meilleures parties).
        /// </summary>
        public int TotalScore { get; set; } = 0;

        /// <summary>
        /// Indique si le joueur est actif (administration ultérieure).
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Type de salle — purement décoratif en V1 (UI statique).
    /// </summary>
    public enum RoomType
    {
        Enemy,      // Ex: "Un gobelin apparaît"
        Treasure,   // Ex: "Un coffre mystérieux"
        Trap        // Ex: "Un piège se déclenche"
    }

    /// <summary>
    /// Représente une "salle" du donjon. En V1: données statiques pour UI.
    /// </summary>
    public class Room
    {
        /// <summary> Nom lisible/affiché (ex: "Salle 1"). </summary>
        public string Title { get; set; } = "Salle 1";

        /// <summary> Type de salle (ennemi, trésor, piège). </summary>
        public RoomType Type { get; set; } = RoomType.Enemy;

        /// <summary>
        /// Description textuelle affichée au joueur (scénarisation).
        /// En V1, pas d’effets/choix réels—juste la présentation.
        /// </summary>
        public string Description { get; set; } = "Un gobelin apparaît. Que faites-vous ?";
    }
}
