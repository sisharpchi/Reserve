FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/Tools/ReserveFlow.MigrationService/ReserveFlow.MigrationService.csproj
RUN dotnet publish src/Tools/ReserveFlow.MigrationService/ReserveFlow.MigrationService.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "ReserveFlow.MigrationService.dll"]
