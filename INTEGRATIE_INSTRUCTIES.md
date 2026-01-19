# Integratie Instructies - Bordspel App FASE 1

## Overzicht van wat er is aangemaakt

Je hebt nu een complete Onion Architecture setup met:

✅ **5 projecten:**
1. `BordspelApp.Domain` - Domeinlaag met entiteiten en business logic
2. `BordspelApp.Application` - Interfaces en contracts
3. `BordspelApp.Infrastructure` - EF Core implementatie
4. `BordspelApp.WebApp` - ASP.NET Core MVC applicatie
5. `BordspelApp.Api` - ASP.NET Core Web API

✅ **Database:**
- EF Core migrations aangemaakt (`InitialCreate`)
- Database seeder met testdata
- Many-to-many relaties geconfigureerd
- Owned types voor Address en DietaryRequirements

✅ **Business Rules:**
- Leeftijdsvalidaties (16+, 18+)
- 18+ berekening voor GameNights
- Maximaal 1 spelavond per dag per persoon
- Validaties voor inschrijvingen

## Hoe te gebruiken

### Optie 1: Gebruik de nieuwe solution (AANBEVOLEN)

De nieuwe solution `BordspelApp.sln` is volledig geconfigureerd en klaar voor gebruik.

1. **Open de solution:**
   ```powershell
   cd "E:\Avans\Leerjaar 2\periode2\bordspel-app"
   start BordspelApp.sln
   ```

2. **Kies je startup project:**
   
   In Visual Studio:
   - Right-click op `BordspelApp.WebApp` → **Set as Startup Project** (voor MVC)
   - OF right-click op `BordspelApp.Api` → **Set as Startup Project** (voor API)
   
   Of beide tegelijk:
   - Right-click op Solution → **Properties** → **Multiple startup projects**
   - Zet beide op "Start"

3. **Run de applicatie:**
   - Druk op F5
   - Database wordt automatisch aangemaakt en geseeded

### Optie 2: Via command line

**WebApp starten:**
```powershell
cd BordspelApp.WebApp
dotnet run
```
Navigeer naar: https://localhost:5001

**API starten:**
```powershell
cd BordspelApp.Api
dotnet run
```
Navigeer naar: https://localhost:7001/swagger

### Optie 3: Integreren met je bestaande project

Als je de nieuwe code wilt integreren in je bestaande `Bordspel-app` project:

1. **Kopieer de domeinmodellen:**
   - Kopieer alles van `BordspelApp.Domain/Entities` naar je bestaande Models folder
   - Kopieer de Enums en ValueObjects

2. **Update je DbContext:**
   - Vervang je `ApplicationDbContext` met de configuratie uit `BordspelAppDbContext.cs`
   - Of merge de `OnModelCreating` configuraties

3. **Voeg repositories toe:**
   - Kopieer de repository interfaces naar een nieuwe `Interfaces` folder
   - Kopieer de repository implementaties naar een `Repositories` folder

4. **Update Program.cs:**
   - Registreer de repositories in DI container
   - Voeg de seeder toe

## Database beheren

### Nieuwe migratie toevoegen

```powershell
dotnet ef migrations add [MigrationName] --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

### Database updaten

```powershell
dotnet ef database update --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

### Database verwijderen en opnieuw aanmaken

```powershell
dotnet ef database drop --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
dotnet ef database update --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

### Migratie verwijderen

```powershell
dotnet ef migrations remove --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

## Testen

### API testen met Swagger

1. Start de API:
   ```powershell
   cd BordspelApp.Api
   dotnet run
   ```

2. Open browser: `https://localhost:7001/swagger`

3. Test de endpoints:
   - GET `/api/persons` - Zie alle personen (inclusief Jan en Anna)
   - GET `/api/boardgames` - Zie alle spellen
   - GET `/api/gamenights` - Zie de spelavond met alle details

### Voorbeeld API calls

**Nieuwe persoon toevoegen:**
```json
POST /api/persons
{
  "firstName": "Pieter",
  "lastName": "Bakker",
  "email": "pieter@example.com",
  "dateOfBirth": "1995-03-15",
  "gender": 0,
  "address": {
    "street": "Hoofdstraat",
    "houseNumber": "10",
    "city": "Breda"
  },
  "dietaryRequirements": {
    "isLactoseFree": false,
    "isNutFree": false,
    "isVegetarian": true,
    "isAlcoholFree": false
  }
}
```

**Inschrijven voor spelavond:**
```
POST /api/gamenights/1/join/2
```

## Project Dependencies

De projecten hebben de volgende referenties:

```
Domain:           geen externe referenties
Application:      → Domain
Infrastructure:   → Application → Domain
                  + EF Core packages
WebApp:           → Application → Infrastructure
                  + EF Core Design
Api:              → Application → Infrastructure
```

## Connection String aanpassen

Als je een andere database wilt gebruiken, pas dan de connection string aan in:
- `BordspelApp.WebApp/appsettings.json`
- `BordspelApp.Api/appsettings.json`

Bijvoorbeeld voor SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BordspelAppDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

## Build warnings

De build warnings over `Azure.Identity` en `Microsoft.Identity.Client` zijn bekend en komen van EF Core dependencies. Ze hebben geen invloed op de functionaliteit voor development. Voor productie kun je deze packages updaten naar nieuwere versies.

## Veelvoorkomende problemen

### "Table already exists"

Als je de migratie opnieuw wilt toepassen:
```powershell
dotnet ef database drop --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp --force
dotnet ef database update --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

### Port is al in gebruik

Pas de poorten aan in `Properties/launchSettings.json` van je project.

### DbContext niet gevonden

Zorg dat je de juiste startup project gebruikt bij EF Core commands met `--startup-project`.

## Volgende stappen voor ontwikkeling

1. **Views toevoegen aan WebApp:**
   - Maak Razor views voor Person, BoardGame, GameNight
   - Gebruik de repositories via controllers

2. **DTOs toevoegen:**
   - Maak Data Transfer Objects voor API
   - Gebruik AutoMapper voor mapping

3. **Validatie uitbreiden:**
   - Gebruik FluentValidation
   - Voeg DataAnnotations toe

4. **Tests schrijven:**
   - Unit tests voor domeinlogica
   - Integration tests voor repositories
   - API tests met WebApplicationFactory

5. **Identity toevoegen:**
   - ASP.NET Core Identity integreren
   - Link met Person entiteit

Succes met je project! 🎲🎮
