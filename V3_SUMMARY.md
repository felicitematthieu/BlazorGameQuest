# BlazorGameQuest - Résumé implémentation V3

## 📋 Objectif V3
Implémenter un système complet de génération d'aventures aléatoires avec gameplay interactif, scoring dynamique et persistance en base de données.

## ✅ Fonctionnalités implémentées

### 1. Modèles de données
**Fichier** : `ASTA/ASTA.SharedModels/Models.cs`

Nouveaux modèles ajoutés :
- `Adventure` : Représente une partie complète
  - `Id`, `PlayerId` (nullable), `Player`
  - `StartTime`, `EndTime`, `TotalScore`, `Status`
  - Relation 1-N avec `AdventureRoom`
  
- `AdventureRoom` : Une salle dans l'aventure
  - `Id`, `AdventureId`, `SequenceIndex`
  - `RoomTitle`, `RoomType`, `Description`
  - `Choice` (choix du joueur), `ScoreDelta` (points gagnés/perdus)

### 2. Base de données
**Fichier** : `ASTA/ASTA.GameApi/AstaDbContext.cs`

Extensions :
- `DbSet<Adventure> Adventures`
- `DbSet<AdventureRoom> AdventureRooms`
- Configuration cascade delete : suppression d'une `Adventure` supprime ses `AdventureRoom`

### 3. Générateur d'aventures
**Fichier** : `ASTA/ASTA.GameApi/AdventureGenerator.cs`

Logique implémentée :
- Génération aléatoire de 2 à 5 salles
- 8 templates de salles (3 Enemy, 3 Treasure, 2 Trap)
- Méthode `GenerateAdventure(playerId?)` : crée une aventure avec salles aléatoires
- Méthode `CalculateScoreDelta(roomType, choice)` : calcul du score selon les règles

**Règles de scoring** :
| Type de salle | Choix | Résultat |
|---------------|-------|----------|
| Enemy | Combattre | 50% → +10 points, 50% → -5 points |
| Enemy | Fuir | +2 points (sûr) |
| Treasure | Ouvrir | 60% → +15 points, 40% → -10 points |
| Treasure | Ignorer | +5 points |
| Trap | Fouiller | +8 points |
| Trap | Ignorer | -3 points |

**Condition de mort** : Score ≤ 0 → Status = "Dead"

### 4. Endpoints API
**Fichier** : `ASTA/ASTA.GameApi/Program.cs`

Groupe `/adventures` :

#### POST /adventures
- Démarre une nouvelle aventure
- Query param : `playerId` (optionnel, nullable)
- **Response** :
```json
{
  "AdventureId": 1,
  "TotalRooms": 4,
  "CurrentRoom": {
    "RoomTitle": "Couloir sombre",
    "RoomType": "Enemy",
    "Description": "Un gobelin vous bloque le passage"
  }
}
```

#### POST /adventures/{id}/choices
- Soumet le choix du joueur pour la salle courante
- **Body** : `{ "Choice": "Combattre" }`
- **Response** :
```json
{
  "NewScore": 10,
  "RoomIndex": 0,
  "IsComplete": false,
  "IsDead": false,
  "NextRoom": {
    "RoomTitle": "Salle du trésor",
    "RoomType": "Treasure",
    "Description": "Un coffre brillant au centre"
  }
}
```

#### GET /adventures/{id}
- Récupère l'aventure complète avec toutes ses salles
- **Response** : Objet `Adventure` avec liste `Rooms`

#### GET /adventures/player/{playerId}
- Liste toutes les aventures d'un joueur
- **Response** : Array de `Adventure`

### 5. Interface utilisateur Blazor
**Fichier** : `BlazorGame.Client/Pages/NewAdventure.razor`

Implémentation complète :
- Démarrage automatique d'aventure au chargement (`OnInitializedAsync`)
- Affichage dynamique de la salle courante
- Composant `RoomCard` pour les boutons de choix
- Gestion états :
  - `loading` : Affichage loader pendant requêtes
  - `error` : Gestion et affichage des erreurs
  - `adventureCompleted` : Écran de fin avec score final
  - `isDead` : Distinction victoire/défaite
- Affichage score en temps réel et progression (Salle X/Y)
- Bouton "Nouvelle aventure" pour rejouer

**Fichier CSS** : `BlazorGame.Client/wwwroot/css/app.css`
- Classes `.score-display`, `.alert`, `.alert-success`, `.alert-danger`
- Styles pour feedback visuel (couleurs, bordures)

### 6. Tests
**Fichiers** :
- `ASTA/ASTA.Tests/AdventureGeneratorTests.cs` (4 tests)
- `ASTA/ASTA.Tests/AdventureEndpointsTests.cs` (4 tests)

#### Tests générateur :
1. `GenerateAdventure_ProducesValidAdventure` : Vérifie 2-5 salles, status InProgress, score 0
2. `CalculateScoreDelta_Enemy_Combattre` : Retourne 10 ou -5
3. `CalculateScoreDelta_Enemy_Fuir` : Retourne 2
4. `CalculateScoreDelta_Treasure_Ouvrir` : Retourne 15 ou -10

#### Tests endpoints :
1. `POST_Adventures_StartsNewAdventure` : Démarre aventure et renvoie première salle
2. `POST_Choices_AdvancesAdventure` : Soumet choix et progresse
3. `GET_Adventure_ReturnsCompleteAdventure` : Récupère aventure par ID
4. `GET_Adventures_ByPlayer_ReturnsPlayerAdventures` : Liste aventures par joueur

**Résultats** : 16/16 tests ✅

### 7. Documentation
**Fichier** : `README.md`
- Section V3 complète avec :
  - Description fonctionnalités
  - Nouveaux modèles
  - Endpoints avec exemples
  - Règles de scoring détaillées
  - Instructions de lancement
  - Commandes de test et couverture

## 📊 Couverture de code
- **Lignes** : 39.2%
- **Branches** : 25%

Commande :
```bash
cd ASTA
dotnet test ASTA.Tests/ASTA.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

*Note* : Couverture focalisée sur logique métier critique (génération, scoring, endpoints aventure). Fichiers utilitaires (Seed, Validation, Swagger) non couverts.

## 🚀 Comment tester

### 1. Lancer les APIs
```bash
# Terminal 1 : GameApi (port 5000)
cd ASTA/ASTA.GameApi
dotnet run

# Terminal 2 : WorldApi (port 5001)
cd ASTA/ASTA.WorldApi
dotnet run
```

### 2. Tester via Swagger
Accéder à http://localhost:5000/swagger
1. POST `/adventures` (sans paramètres) → Note l'`AdventureId`
2. POST `/adventures/{id}/choices` avec body `{"Choice":"Combattre"}`
3. Répéter step 2 jusqu'à `IsComplete: true`
4. GET `/adventures/{id}` pour voir l'aventure complète

### 3. Lancer l'interface Blazor
```bash
# Terminal 3 : Client Blazor
cd BlazorGame.Client
dotnet run
```

Accéder à l'URL affichée (ex: http://localhost:5173)
- Aller sur "Nouvelle aventure"
- Jouer en cliquant sur les boutons de choix
- Observer le score évoluer
- Voir l'écran de fin (victoire ou mort)

### 4. Lancer les tests
```bash
# Tous les tests
dotnet test ASTA/ASTA.sln

# Tests V3 uniquement
dotnet test ASTA/ASTA.Tests/ASTA.Tests.csproj --filter "FullyQualifiedName~Adventure"
```

## 📁 Fichiers modifiés/créés

### Créés
- `ASTA/ASTA.GameApi/AdventureGenerator.cs`
- `ASTA/ASTA.Tests/AdventureGeneratorTests.cs`
- `ASTA/ASTA.Tests/AdventureEndpointsTests.cs`
- `V3_SUMMARY.md` (ce fichier)

### Modifiés
- `ASTA/ASTA.SharedModels/Models.cs` (ajout Adventure, AdventureRoom)
- `ASTA/ASTA.GameApi/AstaDbContext.cs` (ajout DbSets)
- `ASTA/ASTA.GameApi/Program.cs` (4 nouveaux endpoints + DTO ChoiceRequest)
- `BlazorGame.Client/Pages/NewAdventure.razor` (UI interactive complète)
- `BlazorGame.Client/wwwroot/css/app.css` (styles score/alerts)
- `README.md` (section V3 complète)
- `ASTA/ASTA.Tests/PlayerQueryTests.cs` (fix test filter)

## 🎯 Conformité avec les exigences

### ✅ Génération aléatoire
- 2 à 5 salles générées dynamiquement ✓
- Types variés (Enemy, Treasure, Trap) ✓
- Templates randomisés ✓

### ✅ Interface interactive
- Boucle de jeu avec choix joueur ✓
- Affichage dynamique des salles ✓
- Feedback visuel (score, progression) ✓
- Écrans de fin (victoire/mort) ✓

### ✅ Calcul du score
- Règles par type de salle ✓
- Effets aléatoires (Combat, Trésor) ✓
- Condition de mort (score ≤ 0) ✓

### ✅ Sauvegarde de partie
- Persistance en base (EF Core) ✓
- Historique des salles jouées ✓
- Récupération partie complète ✓
- Listing par joueur ✓

### ✅ Tests
- Tests unitaires (générateur + scoring) ✓
- Tests d'intégration (endpoints) ✓
- Couverture mesurée et documentée ✓

## 🏆 Conclusion
**V3 entièrement implémentée et fonctionnelle** avec :
- Architecture microservices (GameApi/WorldApi)
- Génération procédurale d'aventures
- Gameplay interactif complet
- Persistance robuste
- Tests automatisés (16 tests passants)
- Documentation complète

**Prêt pour démonstration et évaluation académique ! 🎓**
