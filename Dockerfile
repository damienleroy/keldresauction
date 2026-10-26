FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY KeldresAuction.Api/KeldresAuction.Api.csproj KeldresAuction.Api/
COPY KeldresAuction.Front/KeldresAuction.Front.csproj KeldresAuction.Front/
RUN dotnet restore KeldresAuction.Api/KeldresAuction.Api.csproj \
 && dotnet restore KeldresAuction.Front/KeldresAuction.Front.csproj

COPY KeldresAuction.Api/ KeldresAuction.Api/
COPY KeldresAuction.Front/ KeldresAuction.Front/
RUN dotnet publish KeldresAuction.Api/KeldresAuction.Api.csproj \
    -c Release -o /app/publish --no-restore
RUN dotnet publish KeldresAuction.Front/KeldresAuction.Front.csproj \
    -c Release -o /app/front --no-restore

# One container: the API host also serves the published Blazor WebAssembly front.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/front/wwwroot ./wwwroot
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "KeldresAuction.Api.dll"]
