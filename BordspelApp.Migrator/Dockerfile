FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore BordspelApp.Migrator/BordspelApp.Migrator.csproj
RUN dotnet publish BordspelApp.Migrator/BordspelApp.Migrator.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BordspelApp.Migrator.dll"]
