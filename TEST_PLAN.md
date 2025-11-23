# TEST_PLAN — BlazorGameQuest (Version 1)

Ce fichier décrit les cas de tests (textuel) attendus pour la V1. Il sert de référence pédagogique pour la mise en place des tests automatisés et manuels.

## Objectifs
- Définir les tests unitaires et d'intégration minimaux pour valider la structure V1.
- Donner des scénarios d'acceptation manuelle pour la démonstration.

## Environnement
- .NET 9 (net9.0)
- xUnit pour les tests unitaires
- bUnit pour les tests de composants Blazor
- EF Core InMemory pour les tests liés à la persistence

## Cas unitaires (xUnit)

1) Models.Defaults
- Préconditions: aucune
- Étapes: instancier `Player` et `Room` depuis `SharedModels`
- Attendu: valeurs par défaut non nulles / attendues (Id non vide, Nickname `Aventurier`, TotalScore 0, IsActive true; Room.Title `Salle 1`)

2) ValidationUtil (utilitaire)
- Préconditions: construire un `Player`/`Room` invalide (ex: UserName null ou Name trop long)
- Étapes: appeler `ValidationUtil.Validate(...)`
- Attendu: renvoie `IsValid=false` et dictionnaire d'erreurs contenant la propriété en erreur

## Tests de composants (bUnit)

1) RoomCard.Renders
- Préconditions: un `Room` de test
- Étapes: render `RoomCard` avec le `Room`
- Attendu: HTML contient `Title`, `Description`, boutons `Combattre`, `Fuir`, `Fouiller`

2) RoomCard.OnChoiceSelected
- Préconditions: même component
- Étapes: simuler un clic sur un bouton
- Attendu: l'EventCallback est invoqué avec la valeur attendue

## Tests d'intégration (démarrage minimal)

1) AuthenticationServices.Health
- Préconditions: démarrer l'API (dotnet run)
- Étapes: GET `/api/health`
- Attendu: 200 OK et payload `{ status: "ok", service: "auth" }`

2) ASTA.GameApi.Seed
- Préconditions: démarrer ASTA.GameApi en mode InMemory (config par défaut)
- Étapes: GET `/dungeons`; GET `/players` (selon routes exposées)
- Attendu: les ressources seedées existent (2 joueurs, >0 dungeons)

## Scénarios d'acceptation manuelle

1) Navigation de base
- Étapes: ouvrir l'app Blazor, cliquer `Nouvelle aventure`
- Attendu: la page `NewAdventure` s'affiche et montre un `RoomCard`

2) Démo composant statique
- Étapes: sur `NewAdventure`, cliquer sur un choix
- Attendu: l'action est reçue (log console ou comportement visible)

## Priorisation
- P0 (à implémenter en priorité): Models.Defaults, RoomCard.Renders, AuthenticationServices.Health
- P1: ValidationUtil, RoomCard.OnChoiceSelected, ASTA.GameApi.Seed

## Conventions
- Nommer les tests `ClassName_MethodName_State_Expected`.
- Chaque test doit être indépendant.
- Utiliser des bases InMemory pour les tests reliant EF Core.

---

Pour aide: je peux générer des tests xUnit/bUnit pour ces cas (implémentation), veux-tu que je crée les fichiers de tests automatiquement ?
