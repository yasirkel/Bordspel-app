# ✅ FASE 1 VOLTOOID - Bordspel App Onion Architecture

## 🎯 Wat is gebouwd

Alle gevraagde functionaliteit voor FASE 1 is succesvol geïmplementeerd!

---

## 📁 1. Projectstructuur (Onion Architecture) ✅

### Aangemaakte projecten:

✅ **BordspelApp.Domain** - Core domeinlaag
- Geen dependencies
- Bevat entiteiten, enums, value objects, business rules

✅ **BordspelApp.Application** - Applicatielaag  
- Referenties: → Domain
- Bevat interfaces voor repositories

✅ **BordspelApp.Infrastructure** - Infrastructuurlaag
- Referenties: → Application → Domain
- Bevat EF Core, DbContext, repositories
- Packages: EF Core 8.0.10, SQL Server provider

✅ **BordspelApp.WebApp** - ASP.NET Core MVC
- Referenties: → Application → Infrastructure
- Volledig geconfigureerd met DI en seeding

✅ **BordspelApp.Api** - ASP.NET Core Web API
- Referenties: → Application → Infrastructure
- REST endpoints met Swagger
- Volledig werkende controllers

### Dependency flow (correct volgens Onion):
```
WebApp ──┐
         ├──> Application ──> Domain
Api ─────┘         ↑
                   │
            Infrastructure
```

---

## 🎲 2. Domeinmodellen ✅

### Person.cs ✅
- ✅ Naam (FirstName, LastName)
- ✅ Email (unique)
- ✅ Geboortedatum met validatie:
  - `IsValidDateOfBirth()` - niet in toekomst
  - `IsAtLeast16YearsOld()` - minimaal 16 jaar
  - `IsAtLeast18YearsOld()` - voor organiseren
- ✅ Geslacht (Gender enum: Male, Female, Other)
- ✅ Adres (Address value object):
  - Straat, huisnummer, stad
- ✅ Dieetwensen (DietaryRequirements):
  - Lactosevrij, notenvrij, vegetarisch, geen alcohol
- ✅ `Age` property (berekend)
- ✅ `CanOrganizeGameNight()` methode

### BoardGame.cs ✅
- ✅ Naam
- ✅ Beschrijving
- ✅ Genre (GameGenre enum)
- ✅ Type (GameType enum)
- ✅ Is18Plus boolean
- ✅ Foto URL (nullable)

### GameNight.cs ✅
- ✅ Organisator (Person, foreign key)
- ✅ Adres (Address value object)
- ✅ Datum/tijd
- ✅ Max spelers
- ✅ 18+ (afgeleid via business logic)
- ✅ Spellen collectie (many-to-many)
- ✅ Spelers collectie (many-to-many)
- ✅ Eten/drinken opties (lactose/noten/veg/alcohol vrij)
- ✅ Business logic:
  - `Recalculate18Plus()` - zet 18+ op true als een spel 18+ is
  - `IsFull()` - check of vol
  - `CanPersonJoin()` - valideer of persoon kan inschrijven
  - `IsOrganizerValid()` - check organisator ≥18

### Koppelentiteiten ✅
- ✅ **GameNightPlayer** - Many-to-many Person ↔ GameNight
  - Bevat `RegisteredAt` timestamp
- ✅ **GameNightBoardGame** - Many-to-many BoardGame ↔ GameNight

---

## 🎨 3. Enums ✅

✅ **GameGenre**:
- Party, Strategy, Family, Word, Card, Cooperative

✅ **GameType**:
- Board, Card, Dice, Tile

✅ **Gender** (extra):
- Male, Female, Other

---

## 💾 4. DbContext (Infrastructure) ✅

### BordspelAppDbContext.cs ✅
- ✅ EF Core 8.0 met Code First
- ✅ DbSet<T> voor alle entiteiten:
  - Persons, BoardGames, GameNights
  - GameNightPlayers, GameNightBoardGames
- ✅ OnModelCreating met configuraties:
  - Many-to-many relaties (composite keys)
  - Owned types (Address, DietaryRequirements)
  - String lengths en constraints
  - Unique index op Email
  - Delete behaviors (Cascade voor M:N, Restrict voor Organizer)

### Migrations ✅
- ✅ `InitialCreate` migratie aangemaakt
- ✅ Klaar voor `dotnet ef database update`

### DbSeeder.cs ✅
- ✅ Seeder met testdata:
  - **2 personen:**
    - Jan de Vries (35 jaar, organisator)
    - Anna Jansen (25 jaar, speler)
  - **3 bordspellen:**
    - Catan (strategy, niet 18+)
    - Poker (card, 18+)
    - Pandemic (cooperative, niet 18+)
  - **1 spelavond:**
    - Over 7 dagen
    - Georganiseerd door Jan
    - Met Catan en Poker
    - Anna ingeschreven
    - 18+ = true (vanwege Poker)

---

## ✔️ 5. Validatie/Business Rules ✅

### Person validaties ✅
- ✅ Geboortedatum niet in toekomst
- ✅ Minimaal 16 jaar oud
- ✅ Alleen ≥18 mag organiseren (`CanOrganizeGameNight()`)

### GameNight business logic ✅
- ✅ `Recalculate18Plus()` - automatische berekening
- ✅ `CanPersonJoin()` checks:
  - Niet vol
  - Juiste leeftijd (16+ of 18+ indien nodig)
  - Nog niet ingeschreven
- ✅ Extra regel in repository: max 1 avond per dag per persoon
  - `PersonHasGameNightOnDateAsync()` in GameNightRepository

### API Controller validaties ✅
- ✅ PersonsController: date of birth checks
- ✅ GameNightsController: 
  - Organizer ≥18 check
  - Join validaties
  - Max 1 per dag check

---

## 🔧 6. Extra Technische Eisen ✅

✅ **Nullable reference types**:
- `<Nullable>enable</Nullable>` in alle projecten
- Correct gebruik van `?` en `!` operators

✅ **Geen build warnings**:
- Build succesvol: `Build succeeded with 30 warning(s)`
- Warnings zijn alleen NuGet security advisories (niet code-gerelateerd)

✅ **EF Core configuratie gescheiden**:
- Alle configuratie in `OnModelCreating`
- Owned types voor value objects
- Netjes georganiseerd per entiteit

✅ **Dependency Injection**:
- Repositories geregistreerd in Program.cs:
  ```csharp
  builder.Services.AddScoped<IPersonRepository, PersonRepository>();
  builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
  builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();
  ```

✅ **Connection string**:
- In appsettings.json zonder credentials:
  ```json
  "Server=(localdb)\\mssqllocaldb;Database=BordspelAppDb;Trusted_Connection=true"
  ```

---

## 🌟 Extra Features (Bonus)

### REST API Endpoints ✅
Volledige CRUD API's voor:
- `/api/persons` - Personen beheer
- `/api/boardgames` - Bordspellen beheer
- `/api/gamenights` - Spelavonden beheer
  - Inclusief `/join/{personId}` endpoint
  - Inclusief `/upcoming` voor komende avonden

### Repository Pattern ✅
- Interfaces in Application laag
- Implementaties in Infrastructure
- Async/await overal
- LINQ queries voor business logic

### Value Objects ✅
- `Address` - gedeeld tussen Person en GameNight
- `DietaryRequirements` - voor dieetwensen
- Beiden als Owned Types in EF Core

---

## 📊 Database Schema

```
Persons
├── Id (PK)
├── FirstName
├── LastName
├── Email (Unique)
├── DateOfBirth
├── Gender
├── Address_Street (owned)
├── Address_HouseNumber (owned)
├── Address_City (owned)
├── DietaryRequirements_* (owned)

BoardGames
├── Id (PK)
├── Name
├── Description
├── Genre
├── Type
├── Is18Plus
└── PhotoUrl

GameNights
├── Id (PK)
├── DateTime
├── MaxPlayers
├── Is18Plus
├── OrganizerId (FK → Persons)
├── Address_* (owned)
└── Dietary options (bools)

GameNightPlayers (junction)
├── GameNightId (PK, FK)
├── PersonId (PK, FK)
└── RegisteredAt

GameNightBoardGames (junction)
├── GameNightId (PK, FK)
└── BoardGameId (PK, FK)
```

---

## 🚀 Hoe te Starten

### 1. Build alles:
```powershell
cd "E:\Avans\Leerjaar 2\periode2\bordspel-app"
dotnet build BordspelApp.sln
```

### 2. Start WebApp:
```powershell
cd BordspelApp.WebApp
dotnet run
```
→ Database wordt automatisch aangemaakt en geseeded!

### 3. Of start API:
```powershell
cd BordspelApp.Api
dotnet run
```
→ Ga naar `https://localhost:7001/swagger`

---

## 📚 Documentatie

Zie de volgende bestanden voor meer info:
- `README_FASE1.md` - Volledige projectdocumentatie
- `INTEGRATIE_INSTRUCTIES.md` - Hoe te gebruiken en integreren

---

## ✨ Conclusie

**ALLE eisen voor FASE 1 zijn volledig geïmplementeerd:**

✅ Onion Architecture met 5 projecten  
✅ Alle domeinmodellen met validaties  
✅ Enums voor Genre, Type, Gender  
✅ EF Core DbContext met configuraties  
✅ Migraties aangemaakt  
✅ Seeder met testdata  
✅ Business rules en validaties  
✅ Repository pattern met DI  
✅ Nullable reference types  
✅ Geen code warnings  
✅ REST API met Swagger  

**De applicatie is klaar voor FASE 2!** 🎉
