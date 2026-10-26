# KeldresAuction

Solution .NET 10 en deux parties, déployées dans **un seul container** :

- `KeldresAuction.Api` : API ASP.NET Core (controllers + [Mediator](https://github.com/martinothamar/Mediator), alternative open source à MediatR, et [Ardalis.Result](https://github.com/ardalis/Result) pour le Result pattern traduit automatiquement en statuts HTTP). Telemetry OpenTelemetry (traces, métriques, logs) exportée en OTLP vers le collector (Tempo / Prometheus / Loki / Grafana), health checks `/health` et `/alive`.
- `KeldresAuction.Front` : Blazor WebAssembly, appelle l'API (`/api/hello`). Une fois publié, il est servi par l'API depuis `wwwroot`.
- `KeldresAuction.Api.Tests` : tests d'intégration de l'API (`WebApplicationFactory`).

## Développement

```bash
dotnet run --project src/KeldresAuction.Api    # http://localhost:5100
dotnet run --project src/KeldresAuction.Front  # http://localhost:5200 (appelle l'API sur 5100, CORS ouvert en Development)
dotnet test KeldresAuction.slnx
```

## Telemetry

Définir `OTEL_EXPORTER_OTLP_ENDPOINT` (ex. `http://otel-collector:4317`) pour exporter traces, métriques et logs. Sans cette variable, rien n'est exporté.

## Docker / déploiement

```bash
docker build -t keldresauction/keldresauction .
docker run -p 9093:8080 keldresauction/keldresauction
```

- `deploy.ps1` : build, transfert SSH et `docker run` sur le serveur (variables `SERVER_HOST`, `SERVER_USER`, `SERVER_PORT`, `SERVER_SSH_KEY_PATH`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `DOCKER_NETWORK`).
- `.github/workflows/deploy.yml` : même chose via GitHub Actions (environnement `HomeTest`, secret `SERVER_SSH_KEY`, variables `SERVER_HOST`, `SERVER_USER`, `SERVER_PORT`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `DOCKER_NETWORK`).
