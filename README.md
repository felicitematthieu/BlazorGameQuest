# BlazorGameQuest — Version 1
Aissa MEHENNI / Matthieu FELICITE

## Objectif V1
- Structurer la solution .NET (Client Blazor WASM, Web API placeholder, Shared models).
- Mettre en place les pages et le routing (Accueil, Règles, Nouvelle aventure) + composant de salle statique.
- Créer un projet de tests (xUnit + bUnit) et couvrir les bases (modèles + rendu composant).

## Lancer le client
```bash
dotnet run --project BlazorGame.Client

## Version 2 – Modélisation & Base de données
- Modèles: Player, Admin, Dungeon, Room (projet ASTA.SharedModels)
- EF Core: InMemory (PostgreSQL prêt à l’emploi)
- Micro-services non sécurisés:
  - ASTA.GameApi → /players [GET (page,pageSize,q), POST, GET {id}, PUT {id}, DELETE {id}]
  - ASTA.WorldApi → /dungeons [GET, POST, GET {id}, DELETE {id}], /dungeons/{id}/rooms [POST]
- Swagger activé + exemples de schémas
- Seed de données en dev
- Tests unitaires (xUnit + EF InMemory) : verts

### Lancer la V2 (avec PostgreSQL via Docker)

```bash
# Nettoyer un éventuel conteneur existant
 docker rm -f asta-pg 2>/dev/null || true

# Lancer PostgreSQL en conteneur Docker
 docker run -d --name asta-pg \
   -e POSTGRES_PASSWORD=postgres \
   -e POSTGRES_DB=asta_game \
   -p 5432:5432 \
   -v asta_pgdata:/var/lib/postgresql/data \
   postgres:16

# Lancer les APIs (dans deux terminaux séparés ou en arrière-plan)
cd ASTA.GameApi
 dotnet run

cd ../ASTA.WorldApi
 dotnet run
```

> Vérifiez que la connexion à PostgreSQL fonctionne (les APIs doivent démarrer sans erreur de connexion à la base).

