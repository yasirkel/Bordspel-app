# Bordspel App - FASE 1: Onion Architecture

## Projectoverzicht

Dit is een ASP.NET Core 8 applicatie voor het beheren van bordspellenavonden, gebouwd volgens **Onion Architecture** principes voor de casus "Avans Spel Systeem".

## Projectstructuur

```
BordspelApp/
├── BordspelApp.Domain/              # Core domeinlaag (entiteiten, enums, business rules)
│   ├── Entities/
│   │   ├── Person.cs                # Persoon met validaties
│   │   ├── BoardGame.cs             # Bordspel
│   │   ├── GameNight.cs             # Spelavond met business logic
│   │   ├── GameNightPlayer.cs       # Koppeltabel speler-avond
│   │   └── GameNightBoardGame.cs    # Koppeltabel spel-avond
│   ├── Enums/
│   │   ├── GameGenre.cs             # Spel genres
│   │   ├── GameType.cs              # Spel types
│   │   └── Gender.cs                # Geslacht
│   └── ValueObjects/
│       ├── Address.cs               # Adres value object
│       └── DietaryRequirements.cs   # Dieetwensen
│
├── BordspelApp.Application/         # Applicatielaag (interfaces, services)
│   └── Interfaces/
│       ├── IPersonRepository.cs
│       ├── IBoardGameRepository.cs
│       └── IGameNightRepository.cs
│
├── BordspelApp.Infrastructure/      # Infrastructuurlaag (EF Core, DbContext, repositories)
│   ├── Data/
│   │   ├── BordspelAppDbContext.cs  # DbContext met configuraties
│   │   └── DbSeeder.cs              # Database seeder met testdata
│   ├── Repositories/
│   │   ├── PersonRepository.cs
│   │   ├── BoardGameRepository.cs
│   │   └── GameNightRepository.cs
│   └── Migrations/                  # EF Core migraties
│
├── BordspelApp.WebApp/              # ASP.NET Core MVC web applicatie
│   ├── Program.cs                   # Configuratie met DI
│   └── appsettings.json             # Connection string configuratie
│
└── BordspelApp.Api/                 # ASP.NET Core Web API
    ├── Controllers/
    │   ├── PersonsController.cs     # REST API voor personen
    │   ├── BoardGamesController.cs  # REST API voor bordspellen
    │   └── GameNightsController.cs  # REST API voor spelavonden
    ├── Program.cs
    └── appsettings.json
```

## Dependency-structuur (Onion Architecture)

```
WebApp ────┐
           ├──> Application ──> Domain
Api ───────┘         ▲
                     │
              Infrastructure
```

## Technologieën

- **.NET 8.0** met C#
- **Entity Framework Core 8.0.10** (Code First)
- **SQL Server** (LocalDB voor development)
- **Nullable Reference Types** ingeschakeld
- **Repository Pattern**
- **Dependency Injection**

## Installatie & Gebruik

### 1. Vereisten

- .NET 8.0 SDK
- SQL Server LocalDB (of SQL Server)
- Visual Studio 2022 / VS Code / Rider

### 2. Database opzetten

De database wordt automatisch aangemaakt en geseeded bij het opstarten van de WebApp:

```powershell
cd BordspelApp.WebApp
dotnet run
```

Of handmatig de database updaten:

```powershell
# Vanaf de root directory
dotnet ef database update --project BordspelApp.Infrastructure --startup-project BordspelApp.WebApp
```

### 3. Applicaties starten

**WebApp (MVC):**
```powershell
cd BordspelApp.WebApp
dotnet run
```
Navigeer naar: `https://localhost:5001`

**API:**
```powershell
cd BordspelApp.Api
dotnet run
```
Navigeer naar: `https://localhost:7001/swagger` voor Swagger UI

### 4. Testdata

Bij het eerste opstarten wordt de database automatisch gevuld met:
- **2 personen:**
  - Jan de Vries (organisator, 35 jaar)
  - Anna Jansen (speler, 25 jaar)
- **3 bordspellen:**
  - Catan (strategy, niet 18+)
  - Poker (card, 18+)
  - Pandemic (cooperative, niet 18+)
- **1 spelavond:**
  - Over 7 dagen
  - Georganiseerd door Jan
  - Met Catan en Poker
  - Anna is ingeschreven

## Domeinregels & Validaties

### Person
- ✅ Leeftijd minimaal 16 jaar
- ✅ Geboortedatum niet in de toekomst
- ✅ Alleen 18+ mag organiseren
- ✅ Unieke email (database constraint)
- ✅ Age property (berekend)

### GameNight
- ✅ 18+ status automatisch berekend op basis van spellen
- ✅ Organisator moet minimaal 18 zijn
- ✅ Speler kan alleen inschrijven als:
  - Er is nog plek
  - Juiste leeftijd (16+ of 18+ indien nodig)
  - Maximaal 1 spelavond per dag per persoon
- ✅ Datum moet in de toekomst zijn

### BoardGame
- Genres: Party, Strategy, Family, Word, Card, Cooperative
- Types: Board, Card, Dice, Tile
- 18+ status per spel

## API Endpoints

### Persons
- `GET /api/persons` - Alle personen
- `GET /api/persons/{id}` - Persoon per ID
- `POST /api/persons` - Nieuwe persoon
- `PUT /api/persons/{id}` - Update persoon
- `DELETE /api/persons/{id}` - Verwijder persoon

### BoardGames
- `GET /api/boardgames` - Alle bordspellen
- `GET /api/boardgames/{id}` - Bordspel per ID
- `GET /api/boardgames/adult` - Alle 18+ spellen
- `POST /api/boardgames` - Nieuw bordspel
- `PUT /api/boardgames/{id}` - Update bordspel
- `DELETE /api/boardgames/{id}` - Verwijder bordspel

### GameNights
- `GET /api/gamenights` - Alle spelavonden (met details)
- `GET /api/gamenights/{id}` - Spelavond per ID
- `GET /api/gamenights/upcoming` - Komende spelavonden
- `POST /api/gamenights` - Nieuwe spelavond
- `POST /api/gamenights/{id}/join/{personId}` - Inschrijven voor spelavond

## Database Schema

### Tabellen
- `Persons` - Personen (met embedded Address en DietaryRequirements)
- `BoardGames` - Bordspellen
- `GameNights` - Spelavonden (met embedded Address)
- `GameNightPlayers` - Koppeltabel (Many-to-Many)
- `GameNightBoardGames` - Koppeltabel (Many-to-Many)

### Relaties
- Person ← (1:N) → GameNight (als organisator)
- Person ← (M:N) → GameNight (als deelnemer)
- BoardGame ← (M:N) → GameNight

## Connection String

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BordspelAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

Voor productie: vervang door echte SQL Server connection string.

## Volgende Stappen (FASE 2+)

- [ ] ASP.NET Core Identity toevoegen voor authenticatie
- [ ] Authorization (roles: User, Organizer, Admin)
- [ ] ViewModels en DTOs voor API
- [ ] Razor Pages/Views voor WebApp
- [ ] Unit tests en integration tests
- [ ] Logging en error handling
- [ ] Email notificaties
- [ ] File upload voor bordspel foto's

## Auteur

Ontwikkeld voor de casus "Avans Spel Systeem" - Avans Hogeschool
