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

#### Étapes de lancement

**1. Lancer GameApi** (port 5198)
```bash
cd ASTA/ASTA.GameApi
dotnet run
```
✅ Attendre le message : `Now listening on: http://localhost:5198`

**2. Lancer WorldApi** (port 5002) — **Nouveau terminal**
```bash
cd ASTA/ASTA.WorldApi
dotnet run
```
✅ Attendre le message : `Now listening on: http://localhost:5002`

**3. Lancer Client Blazor** (port 5109) — **Nouveau terminal**
```bash
cd BlazorGame.Client
dotnet run
```
✅ Attendre le message : `Now listening on: http://localhost:5109`

**4. Ouvrir l'application**
- **Interface de jeu** : http://localhost:5109
- **Swagger GameApi** : http://localhost:5198/swagger
- **Swagger WorldApi** : http://localhost:5002/swagger

#### Troubleshooting

**Erreur "port already in use"**
```bash
# Trouver le processus utilisant le port (exemple 5198)
lsof -i :5198
# Tuer le processus
kill -9 <PID>
```

**Erreur CORS lors de l'appel API**
- Vérifier que les 3 services tournent bien sur leurs ports respectifs
- Les ports 4200, 5173 et 5109 sont autorisés dans Program.cs

**Erreur PostgreSQL "could not connect"**
- Par défaut, l'application utilise **InMemory** (pas besoin de Docker)
- Si vous avez modifié `appsettings.Development.json` pour utiliser PostgreSQL, lancer `docker compose up -d`

**Les tests échouent**
```bash
# Depuis la racine du projet
cd ASTA
dotnet test ASTA.sln
```
✅ Tous les tests doivent passer (16/16)

### Tests & Couverture V3
```bash
# Tous les tests (16 au total)
dotnet test ASTA/ASTA.sln

# Couverture de code avec XPlat Code Coverage
cd ASTA
dotnet test ASTA.Tests/ASTA.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

**Résultats couverture** :
- **Lignes** : 39.2%
- **Branches** : 25%

*Note* : La couverture actuelle se concentre sur la logique métier critique (génération aventures, scoring, endpoints aventure). Les fichiers utilitaires (Seed, Validation, Swagger) ne sont pas couverts. Pour atteindre 80%, il faudrait ajouter des tests unitaires sur `Seed.cs`, `Validation.cs`, `SwaggerExamples.cs` et les helpers.

Tests inclus :
- 3 tests Player (création, validation)
- 1 test Dungeon (cascade delete)
- 2 tests PlayerQuery (pagination, filtre)
- 2 tests ApiEndpoints (GameApi + WorldApi)
- 4 tests AdventureGenerator (génération valide, scoring Enemy/Treasure)
- 4 tests AdventureEndpoints (start, choices, get, list)

**Total : 16 tests ✅**

