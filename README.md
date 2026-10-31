# KeldresAuction

Solution .NET 10 en deux parties, déployées dans **un seul container** :

- `KeldresAuction.Api` : API ASP.NET Core (controllers, Swagger UI sur `/swagger` en Development, + [Mediator](https://github.com/martinothamar/Mediator), alternative open source à MediatR, et [Ardalis.Result](https://github.com/ardalis/Result) pour le Result pattern traduit automatiquement en statuts HTTP). Données dans PostgreSQL via Entity Framework Core (migrations appliquées au démarrage). Telemetry OpenTelemetry (traces, métriques, logs) exportée en OTLP vers le collector (Tempo / Prometheus / Loki / Grafana), health checks `/health` (avec la base) et `/alive`.
- `KeldresAuction.Front` : Blazor WebAssembly avec [Fluent UI Blazor v5](https://github.com/microsoft/fluentui-blazor) (les conventions d'usage sont dans `.github/skills/fluentui-blazor-usage`), il ne fait qu'appeler l'API. Une fois publié, il est servi par l'API depuis `wwwroot`.
- `KeldresAuction.Api.Tests` : tests unitaires du domaine (règles de l'enchère au second prix).

## Enchère à pli fermé au second prix

Un objet, un prix de réserve, plusieurs enchérisseurs qui peuvent chacun placer plusieurs offres.

- Le **gagnant** est l'enchérisseur de l'offre la plus haute, si elle atteint le prix de réserve.
- Le **prix payé** est la plus haute offre (≥ réserve) d'un *autre* enchérisseur, sinon le prix de réserve.
- Égalité sur l'offre la plus haute : la première offre déposée l'emporte. Les noms sont comparés sans tenir compte de la casse.
- Sans offre atteignant la réserve, il n'y a pas de gagnant.

La page d'accueil permet d'envoyer un **fichier CSV** (ou de générer un exemple, côté front, qui l'envoie à l'API) : un enchérisseur par ligne, son nom puis ses offres, séparés par des virgules, `.` comme séparateur décimal.

```csv
A,110,130
B
C,125
D,105,115,90
E,132,135,140
```

Avec une réserve de 100, E gagne et paie 130.

API : `POST /api/auctions` (multipart : `File` + `ReservePrice`), `GET /api/auctions/latest`, `GET /api/auctions/{id}`, `GET /api/auctions/{id}/result`.

## Lancer toute la stack en local

```bash
docker compose up -d --build   # PostgreSQL + l'application sur http://localhost:9093
docker compose down            # arrêt (ajouter -v pour supprimer aussi les données)
```

## Développement

```bash
docker compose up -d postgres                # PostgreSQL local (port 5433)
dotnet run --project src/KeldresAuction.Api    # http://localhost:5100
dotnet run --project src/KeldresAuction.Front  # http://localhost:5200 (appelle l'API sur 5100, CORS ouvert en Development)
dotnet test KeldresAuction.slnx
```

Nouvelle migration EF Core (l'outil `dotnet-ef` est dans le manifeste d'outils) :

```bash
dotnet tool restore
dotnet ef migrations add NomDeLaMigration --project src/KeldresAuction.Api --output-dir Infrastructure/Persistence/Migrations
```

## Telemetry

Définir `OTEL_EXPORTER_OTLP_ENDPOINT` (ex. `http://otel-collector:4317`) pour exporter traces (dont les requêtes Npgsql), métriques et logs. Sans cette variable, rien n'est exporté.

## Docker / déploiement

L'API a besoin de la chaîne de connexion PostgreSQL (`ConnectionStrings__keldresauction`) ; la base doit exister, les tables sont créées au démarrage.

```bash
docker build -t keldresauction/keldresauction .
docker run -p 9093:8080 -e "ConnectionStrings__keldresauction=Host=...;Database=keldresauction;Username=...;Password=..." keldresauction/keldresauction
```

- `deploy.ps1` : build, transfert SSH et `docker run` sur le serveur (variables `SERVER_HOST`, `SERVER_USER`, `SERVER_PORT`, `SERVER_SSH_KEY_PATH`, `CONNECTIONSTRING_KELDRESAUCTION`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `DOCKER_NETWORK`).
- `.github/workflows/deploy.yml` : même chose via GitHub Actions (environnement `HomeTest`, secrets `SERVER_SSH_KEY` et `CONNECTIONSTRING_KELDRESAUCTION`, variables `SERVER_HOST`, `SERVER_USER`, `SERVER_PORT`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `DOCKER_NETWORK`). Les tests tournent avant le déploiement.
