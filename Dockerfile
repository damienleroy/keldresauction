FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/KeldresAuction.Api/KeldresAuction.Api.csproj src/KeldresAuction.Api/
COPY src/KeldresAuction.Front/KeldresAuction.Front.csproj src/KeldresAuction.Front/
RUN dotnet restore src/KeldresAuction.Api/KeldresAuction.Api.csproj \
 && dotnet restore src/KeldresAuction.Front/KeldresAuction.Front.csproj

COPY src/KeldresAuction.Api/ src/KeldresAuction.Api/
COPY src/KeldresAuction.Front/ src/KeldresAuction.Front/
RUN dotnet publish src/KeldresAuction.Api/KeldresAuction.Api.csproj \
    -c Release -o /app/publish --no-restore
RUN dotnet publish src/KeldresAuction.Front/KeldresAuction.Front.csproj \
    -c Release -o /app/front --no-restore

# One container: the API host also serves the published Blazor WebAssembly front.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/front/wwwroot ./wwwroot
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "KeldresAuction.Api.dll"]
