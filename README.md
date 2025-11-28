# BlazorGameQuest — Version 1
Aissa MEHENNI / Matthieu FELICITE

## Objectif V1
- Structurer la solution .NET (Client Blazor WASM, Web API placeholder, Shared models).
- Mettre en place les pages et le routing (Accueil, Règles, Nouvelle aventure) + composant de salle statique.
- Créer un projet de tests (xUnit + bUnit) et couvrir les bases (modèles + rendu composant).

## Lancer le client
```bash
dotnet run --project BlazorGame.Client
```

## Pages V1
- `Index` (Accueil)
- `Rules` (Règles du jeu)
- `NewAdventure` (Nouvelle aventure) — affiche le composant `RoomCard` statique

## Plan de tests V1 (résumé)
Le plan ci-dessous décrit les cas de tests attendus pour la Version 1. Les tests implémentés dans le projet couvrent déjà quelques cas (models, rendu du composant). Les éléments listés ici sont à implémenter progressivement :

- Unitaires (xUnit)
  - Models: valeurs par défaut (Player, Room) — déjà présents
  - Validation utilitaire: vérification des règles d'attributs (DataAnnotations)

- Composants (bUnit)
  - `RoomCard` : rendu du titre, description et boutons (déjà présent)
  - Interaction : vérifie que l'événement `OnChoiceSelected` est déclenché sur clic

- Intégration minimale
  - API Health: `AuthenticationServices` renvoie OK
  - APIs ASTA (dev) : démarrage avec seed (vérifier `GET /dungeons`, `GET /players` si exposés)

- Scénarios d'acceptance manuelle
  - Navigation: de l'accueil à `Nouvelle aventure`
  - Lancer une partie statique (V1) et vérifier affichage des salles

Pour un plan détaillé et les cas de tests à implémenter (noms, préconditions, étapes, résultat attendu), voir `TEST_PLAN.md`.
## Version 2 – Modélisation & Base de données
- Modèles: Player, Admin, Dungeon, Room (projet ASTA.SharedModels)
- EF Core: InMemory (PostgreSQL prêt à l’emploi)
- Micro-services non sécurisés:
  - ASTA.GameApi → /players [GET (page,pageSize,q), POST, GET {id}, PUT {id}, DELETE {id}]
  - ASTA.WorldApi → /dungeons [GET, POST, GET {id}, DELETE {id}], /dungeons/{id}/rooms [POST]
- Swagger activé + exemples de schémas
- Seed de données en dev
- Tests unitaires (xUnit + EF InMemory) : verts

### Endpoints détaillés
GameApi:
- GET /players?page=&pageSize=&q= : liste paginée + filtre simple (q)
- GET /players/{id} : joueur par Id
- POST /players : création
- PUT /players/{id} : mise à jour (UserName, Level)
- DELETE /players/{id} : suppression
- GET /dungeons : liste des donjons avec rooms
- GET /dungeons/{id} : donjon + rooms
- POST /dungeons : création donjon
- POST /dungeons/{id}/rooms : ajout d’une salle
- DELETE /dungeons/{id} : suppression (cascade rooms)

WorldApi:
- GET /dungeons
- GET /dungeons/{id}
- POST /dungeons
- POST /dungeons/{id}/rooms
- DELETE /dungeons/{id}

### Configuration EF Core
Par défaut InMemory (développement rapide). Pour activer PostgreSQL:
1. Lancer docker (voir docker-compose.yml)
2. Ajouter dans `appsettings.Development.json`:
```json
{
  "UseInMemory": false,
  "ConnectionStrings": { "Default": "Host=localhost;Port=5432;Database=asta;Username=asta;Password=asta" }
}
```
3. Générer migrations (exemple GameApi):
```bash
cd ASTA/ASTA.GameApi
dotnet ef migrations add InitialCreate -o Migrations
dotnet ef database update
```

### Docker Compose (PostgreSQL)
Voir fichier `docker-compose.yml` à la racine pour démarrer Postgres:
```bash
docker compose up -d
```
Arrêt:
```bash
docker compose down
```

### Couverture de code (coverlet)
Commande:
```bash
dotnet test ASTA/ASTA.Tests/ASTA.Tests.csproj -c Debug \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=lcov \
  /p:CoverletOutput=../coverage/
```
Rapport lcov disponible dans `ASTA/coverage/`. Intégrable avec services externes.

### Tests ajoutés V2
- Cascade Dungeon→Rooms: `DungeonTests`
- Pagination & Filtre joueurs: `PlayerQueryTests`
- Endpoints via WebApplicationFactory: `ApiEndpointsTests`

### Prochaines étapes (pré-V3)
- Lier IHM Blazor aux endpoints (HttpClient + affichage dynamique)
- Ajout logique génération aléatoire de donjon et scoring
- Début gestion rôles Keycloak (auth joueur/admin)
- Tests d'intégration supplémentaires + coverage publish

## Version 3 – Génération d'aventures & Gameplay interactif

### Fonctionnalités V3
- **Génération aléatoire de donjons** : 2 à 5 salles générées dynamiquement
- **Types de salles** : Enemy (Combat), Treasure (Trésor), Trap (Piège)
- **Système de scoring** : Points gagnés/perdus selon les choix
- **Sauvegarde de progression** : Persistance en base des aventures avec historique
- **Interface interactive** : UI Blazor connectée aux endpoints avec boucle de jeu

### Nouveaux modèles
- `Adventure` : représente une partie (PlayerId, TotalScore, Status, StartTime, EndTime)
- `AdventureRoom` : une salle dans l'aventure (SequenceIndex, RoomType, Choice, ScoreDelta)

### Endpoints Aventure (GameApi)
- **POST /adventures** : Démarre nouvelle aventure, renvoie première salle
  - Query param: `playerId` (optionnel)
  - Response: `{ AdventureId, TotalRooms, CurrentRoom }`
  
- **POST /adventures/{id}/choices** : Soumet choix joueur
  - Body: `{ Choice: "Combattre" | "Fuir" | "Fouiller" | "Ouvrir" | "Ignorer" }`
  - Response: `{ NewScore, RoomIndex, IsComplete, IsDead, NextRoom }`
  
- **GET /adventures/{id}** : Récupère aventure complète avec toutes salles
  
- **GET /adventures/player/{playerId}** : Liste aventures d'un joueur

### Règles de scoring

#### Salle Enemy (Ennemi)
- **Combattre** : 50% de chance → +10 points ou -5 points
- **Fuir** : +2 points (sûr)
- **Fouiller** : 0 point

#### Salle Treasure (Trésor)
- **Ouvrir** : 60% de chance → +15 points ou -10 points (piège)
- **Ignorer** : +5 points (prudence récompensée)
- **Fouiller** : +3 points

#### Salle Trap (Piège)
- **Fouiller** : +8 points (désactivation)
- **Ignorer** : -3 points
- **Combattre** : -8 points (mauvais choix)

**Condition de mort** : Si TotalScore ≤ 0, l'aventure se termine avec statut "Dead"

### Tests V3
- **AdventureGeneratorTests** : Validation génération 2-5 salles + calculs score
- **AdventureEndpointsTests** : Tests d'intégration pour workflow complet
  - Démarrage aventure
  - Soumission choix avec progression
  - Récupération état aventure
  - Listing par joueur

### UI Blazor connectée
`NewAdventure.razor` implémente :
- Démarrage automatique d'aventure au chargement
- Affichage salle courante avec score
- Boutons de choix interactifs (via RoomCard)
- Progression automatique après chaque choix
- Écran de fin (victoire/mort) avec score final
- Bouton "Nouvelle aventure" pour rejouer

### Lancer l'environnement complet

#### Prérequis
- **.NET 9 SDK** installé ([télécharger ici](https://dotnet.microsoft.com/download/dotnet/9.0))
- Vérifier la version : `dotnet --version` (doit afficher 9.x.x)

#### Démarrage rapide

**Option 1 : Lancement en arrière-plan (recommandé)**
```bash
# Terminal 1 : API
cd ASTA/ASTA.GameApi && dotnet run &

# Terminal 2 : Blazor
cd BlazorGame.Client && dotnet run &
```

**Option 2 : Lancement dans des terminaux séparés**
```bash
# Terminal 1 : Démarrer l'API
cd ASTA/ASTA.GameApi
dotnet run
# Attendre "Now listening on: http://localhost:5198"

# Terminal 2 : Démarrer Blazor (dans un nouveau terminal)
cd BlazorGame.Client
dotnet run
# Attendre "Now listening on: http://localhost:5109"
```

**Accès aux interfaces**
- **Interface de jeu** : http://localhost:5109
- **Admin Dashboard** : http://localhost:5109/admin
- **Swagger API** : http://localhost:5198/swagger

> **Note** : L'API doit être démarrée AVANT le client Blazor

#### Troubleshooting

**Erreur "port already in use"**
```bash
# Tuer tous les processus dotnet
pkill -9 dotnet
# Ou tuer spécifiquement par port
lsof -ti:5198 | xargs kill -9
lsof -ti:5109 | xargs kill -9
```

**Erreur "connection refused" dans le navigateur**
- Vérifier que l'API est bien démarrée : `curl http://localhost:5198/api/players`
- Vérifier les logs de l'API dans le terminal
- Redémarrer l'API puis Blazor

**Le classement est vide**
- Les aventures doivent être jouées **après** avoir démarré les services avec les dernières modifications
- Vérifier qu'une aventure a un `PlayerId`: `curl http://localhost:5198/api/admin/adventures`
- Si `playerId: null`, rejouer une partie complète

**Les tests échouent**
```bash
cd ASTA
dotnet test
```
✅ 43/43 tests doivent passer

### Tests & Couverture V3
```bash
# Tous les tests (38 au total)
cd ASTA
dotnet test
```

**Résultats couverture** :
- **Lignes** : 45.42%
- **Branches** : 30.48%

Tests inclus :
- 3 tests Player (création, validation)
- 1 test Dungeon (cascade delete)
- 2 tests PlayerQuery (pagination, filtre)
- 2 tests ApiEndpoints (GameApi + WorldApi)
- 4 tests AdventureGenerator (génération valide, scoring Enemy/Treasure)
- 4 tests AdventureEndpoints (start, choices, get, list)
- 10 tests ValidationUtil (validation Player, Dungeon, DataAnnotations)
- 3 tests Seed (initialisation base de données)
- 9 tests Services (PlayerService, DungeonService, AdventureService)

**Total : 38 tests ✅**

## Version 4 – Administration & Classements

### Fonctionnalités V4

#### Backend - Endpoints Admin
- **GET /api/admin/leaderboard** : Classement général des joueurs par score total
  - Query param: `top` (nombre de joueurs, défaut 100)
  - Retourne: liste triée par score décroissant avec statistiques
  
- **GET /api/admin/adventures** : Liste complète des parties avec filtres
  - Query params: `playerId`, `status` (InProgress/Completed/Dead), `page`, `pageSize`
  - Pagination intégrée
  
- **GET /api/admin/players** : Liste de tous les joueurs
  - Utilise le service PlayerService avec pagination large (1000 joueurs max)
  
- **PUT /api/admin/players/{id}/status** : Activer/désactiver un joueur
  - Body: `true` (actif) ou `false` (désactivé)
  
- **GET /api/admin/players/export** : Export CSV des joueurs
  - Télécharge automatiquement un fichier CSV avec colonnes: Id, UserName, Level, IsActive, TotalScore, AdventureCount
  
- **GET /api/players/{id}/history** : Historique personnel d'un joueur
  - Retourne toutes les aventures du joueur avec détails des salles

#### Modèles mis à jour
- **Player.IsActive** : Nouveau champ booléen (défaut `true`)
  - Permet la désactivation de comptes sans suppression

#### Frontend - Interfaces Admin Blazor

**Dashboard Admin** (`/admin`)
- Menu avec 4 tuiles cliquables:
  - 👥 Gestion Joueurs → `/admin/players`
  - 🏆 Classement Général → `/admin/leaderboard`
  - 🗺️ Liste des Parties → `/admin/adventures`
  - 🏠 Retour au Jeu → `/`

**Gestion des Joueurs** (`/admin/players`)
- Tableau complet avec colonnes: ID, Nom, Niveau, Statut, Score Total, Nombre de parties
- Actions:
  - Activer/Désactiver un joueur (bouton rouge/vert)
  - Export CSV (bouton en haut)
  - Lien vers historique personnel (clic sur nom)
- Indicateurs visuels:
  - Badge vert "Actif" / rouge "Désactivé"
  - Ligne grisée pour joueurs désactivés

**Classement Général** (`/admin/leaderboard`)
- Top 50 joueurs affichés
- Podium visuel: 🥇🥈🥉 pour les 3 premiers
- Cartes colorées (or, argent, bronze) pour le podium
- Statistiques affichées:
  - Score Total cumulé (somme de toutes les parties)
  - Meilleur Score d'une partie
  - Ratio Parties complétées / Total
- Indicateur "Désactivé" pour joueurs inactifs
- **Important** : Les joueurs apparaissent uniquement s'ils ont des parties terminées (Completed ou Dead) avec un PlayerId valide

**Liste des Parties** (`/admin/adventures`)
- Filtres:
  - Par ID joueur
  - Par statut (InProgress/Completed/Dead)
- Tableau avec colonnes: ID, Joueur, Score, Statut, Nb Salles, Début, Fin, Durée
- Pagination (20 parties par page)
- Badges colorés:
  - Vert : Completed
  - Rouge : Dead
  - Orange : InProgress
- Scores colorés (vert si positif, rouge si négatif)

**Historique Joueur** (`/admin/players/{id}/history`)
- Statistiques en haut:
  - Parties Totales
  - Victoires
  - Score Total cumulé
  - Meilleur Score
- Liste chronologique des aventures avec:
  - Score, statut, nombre de salles
  - Durée de la partie
  - Aperçu des 3 premières salles visitées

### Tests V4

**Tests AdminService ajoutés** :
- `GetLeaderboard_ReturnsTopPlayersByScore` : Vérification tri par score
- `GetPlayerHistory_ReturnsPlayerAdventures` : Historique personnel
- `SetPlayerActiveStatus_UpdatesPlayerStatus` : Activation/désactivation
- `ExportPlayersToCsv_GeneratesValidCsv` : Format CSV valide
- `GetAllAdventures_FiltersCorrectly` : Filtres par joueur et statut

**Total : 43 tests ✅** (vs 38 en V3)

### Couverture de Code V4

```bash
cd ASTA
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

**Résultats couverture V4** :
- **Lignes** : 48.73% (+3.31% vs V3)
- **Branches** : 41.4% (+10.92% vs V3)

**Fichiers bien couverts** :
- `AdventureGenerator.cs` : 87.75%
- `ValidationUtil.cs` : 100%
- `Seed.cs` : 85%
- `AdminService.cs` : 65%
- `PlayerService`, `DungeonService`, `AdventureService` : 60-70%
- Controllers : 55-65%

### Tester toute l'application V4

#### 1. Tests unitaires automatisés
```bash
cd ASTA
dotnet test
# Résultat attendu: 43/43 tests ✅
```

#### 2. Tests manuels via Swagger
Accéder à http://localhost:5198/swagger et tester :

**Endpoints Admin** :
- **GET** `/api/admin/leaderboard?top=10` → Classement des 10 meilleurs joueurs
- **GET** `/api/admin/adventures?status=Completed&page=1&pageSize=20` → Parties terminées
- **GET** `/api/admin/players` → Liste complète des joueurs
- **PUT** `/api/admin/players/1/status` (Body: `false`) → Désactiver le joueur #1
- **GET** `/api/admin/players/export` → Télécharger players.csv

**Endpoints Joueur** :
- **GET** `/api/players/1/history` → Historique des parties du joueur #1
- **POST** `/api/adventures?playerId=1` → Démarrer nouvelle aventure
- **POST** `/api/adventures/{id}/choices` (Body: `{ "Choice": "Combattre" }`) → Faire un choix

#### 3. Tests UI Blazor
Ouvrir http://localhost:5109 et naviguer :

**Interface Joueur** :
1. Page d'accueil → Cliquer "Nouvelle Aventure"
2. Jouer une partie complète en faisant des choix
3. Observer l'évolution du score et progression

**Dashboard Admin** (http://localhost:5109/admin) :
1. **Gestion Joueurs** (`/admin/players`)
   - Vérifier affichage tableau avec stats
   - Désactiver un joueur (bouton rouge)
   - Exporter CSV
   - Cliquer sur un nom pour voir l'historique

2. **Classement Général** (`/admin/leaderboard`)
   - Vérifier le podium (🥇🥈🥉)
   - Observer les statistiques (Score Total, Meilleur Score, Ratio)

3. **Liste des Parties** (`/admin/adventures`)
   - Tester les filtres (par joueur, par statut)
   - Vérifier la pagination
   - Observer les badges de statut colorés

4. **Historique Joueur** (`/admin/players/1/history`)
   - Voir statistiques du joueur
   - Liste chronologique des parties
   - Détails des salles visitées


