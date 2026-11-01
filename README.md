# KeldresAuction

## Sealed-Bid Second-Price Auction

One item, one reserve price, multiple bidders, and each bidder can place multiple bids.

- The **winner** is the bidder with the highest bid, if it reaches the reserve price.
- The **price paid** is the highest bid (≥ reserve) from another bidder, otherwise the reserve price.
- If the highest bid is tied, the first submitted bid wins. Bidder names are compared case-insensitively.
- If no bid reaches the reserve price, there is no winner.

The home page lets you upload a **CSV file** (or generate a sample on the front-end and send it to the API): one bidder per line, bidder name first, then bids, separated by commas, with `.` as decimal separator.

```csv
A,110,130
B
C,125
D,105,115,90
E,132,135,140
```

With a reserve price of 100, E wins and pays 130.

API: `POST /api/auctions` (multipart: `File` + `ReservePrice`), `GET /api/auctions/latest`, `GET /api/auctions/{id}`, `GET /api/auctions/{id}/result`.

.NET 10 solution split into two parts, deployed in **a single container**:

- `KeldresAuction.Api`: ASP.NET Core API (controllers, Swagger UI on `/swagger` in Development, plus [Mediator](https://github.com/martinothamar/Mediator), an open-source alternative to MediatR, and [Ardalis.Result](https://github.com/ardalis/Result) for the Result pattern automatically translated to HTTP status codes). Data is stored in PostgreSQL via Entity Framework Core (migrations are applied at startup). OpenTelemetry telemetry (traces, metrics, logs) is exported over OTLP to the collector (Tempo / Prometheus / Loki / Grafana), with health checks at `/health` (including database) and `/alive`.
- `KeldresAuction.Front`: Blazor WebAssembly with [Fluent UI Blazor v5](https://github.com/microsoft/fluentui-blazor) (usage conventions are in `.github/skills/fluentui-blazor-usage`), it only calls the API. Once published, it is served by the API from `wwwroot`.
- `KeldresAuction.Api.Tests`: domain unit tests (second-price auction rules).

## Run the full stack locally

```bash
docker compose up -d --build   # PostgreSQL + app on http://localhost:9093
docker compose down            # stop (add -v to also remove data)
```

## Development

```bash
docker compose up -d postgres                  # local PostgreSQL (port 5433)
dotnet run --project src/KeldresAuction.Api    # http://localhost:5100
dotnet run --project src/KeldresAuction.Front  # http://localhost:5200 (calls API on 5100, CORS open in Development)
dotnet test KeldresAuction.slnx
```

New EF Core migration (requires global tool: `dotnet tool install --global dotnet-ef`):

```bash
dotnet ef migrations add MigrationName --project src/KeldresAuction.Api --output-dir Infrastructure/Persistence/Migrations
```

## Telemetry

Set `OTEL_EXPORTER_OTLP_ENDPOINT` (for example `http://otel-collector:4317`) to export traces (including Npgsql requests), metrics, and logs. Without this variable, nothing is exported.

## Docker / Deployment

The API requires the PostgreSQL connection string (`ConnectionStrings__keldresauction`); the database must already exist, tables are created at startup.

```bash
docker build -t keldresauction/keldresauction .
docker run -p 9093:8080 -e "ConnectionStrings__keldresauction=Host=...;Database=keldresauction;Username=...;Password=..." keldresauction/keldresauction
```

- `deploy.ps1`: build, SSH transfer, and `docker run` on the server (variables: `SERVER_HOST`, `SERVER_USER`, `SERVER_PORT`, `SERVER_SSH_KEY_PATH`, `CONNECTIONSTRING_KELDRESAUCTION`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `DOCKER_NETWORK`).
