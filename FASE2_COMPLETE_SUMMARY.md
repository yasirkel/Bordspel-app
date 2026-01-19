# FASE 2 - VOLLEDIGE IMPLEMENTATIE SAMENVATTING

## ✅ WAT IS GEÏMPLEMENTEERD

### 1. Application Layer (BordspelApp.Application)
**Locatie:** `BordspelApp.Application/`

#### DTOs
- ✅ `DTOs/GameNightListItemDto.cs` - Voor lijstweergave
- ✅ `DTOs/GameNightDetailsDto.cs` - Voor detailweergave
- ✅ `DTOs/BoardGameDto.cs` - Voor bordspel info
- ✅ `DTOs/PersonDto.cs` - Voor gebruikersinfo

#### Models
- ✅ `Models/RegistrationResponse.cs` - Inschrijvingsresultaten

#### Interfaces
- ✅ `Interfaces/IGameNightService.cs`
- ✅ `Interfaces/IRegistrationService.cs`
- ✅ `Interfaces/IBoardGameService.cs`
- ✅ `Interfaces/IPersonService.cs`

### 2. Infrastructure Layer (BordspelApp.Infrastructure)
**Locatie:** `BordspelApp.Infrastructure/Services/`

- ✅ `Services/GameNightService.cs` - Volledige CRUD + business logic
- ✅ `Services/RegistrationService.cs` - Registratie met alle validaties
- ✅ `Services/BoardGameService.cs` - Bordspellen ophalen
- ✅ `Services/PersonService.cs` - Profielbeheer

### 3. WebApp Layer (BordspelApp.WebApp)
**Locatie:** `BordspelApp.WebApp/`

#### ViewModels
- ✅ `Models/GameNightListViewModel.cs`
- ✅ `Models/GameNightDetailsViewModel.cs`
- ✅ `Models/GameNightCreateViewModel.cs` (met custom validatie)
- ✅ `Models/RegistrationConfirmViewModel.cs`
- ✅ `Models/BoardGameViewModel.cs`
- ✅ `Models/ProfileEditViewModel.cs`

#### Controllers
- ✅ `Controllers/GameNightsController.cs` - Volledig met alle US_01-US_06 logica
- ✅ `Controllers/BoardGamesController.cs`
- ✅ `Controllers/ProfileController.cs`

#### Views - VOLLEDIG GEDOCUMENTEERD
**Zie:** `FASE2_VIEWS_COMPLETE.md` voor complete code

GameNights Views:
- ✅ `Views/GameNights/All.cshtml` - Met zoeken, filteren, paginatie
- 📝 `Views/GameNights/Mine.cshtml` - Template in documentatie
- 📝 `Views/GameNights/Participating.cshtml` - Template in documentatie
- 📝 `Views/GameNights/Details.cshtml` - Template in documentatie
- 📝 `Views/GameNights/Create.cshtml` - Template in documentatie
- 📝 `Views/GameNights/Edit.cshtml` - Template in documentatie
- 📝 `Views/GameNights/Delete.cshtml` - Template in documentatie
- 📝 `Views/GameNights/ConfirmRegistration.cshtml` - Template in documentatie

BoardGames Views:
- 📝 `Views/BoardGames/Index.cshtml` - Template in documentatie
- 📝 `Views/BoardGames/Details.cshtml` - Template in documentatie

Profile Views:
- 📝 `Views/Profile/Edit.cshtml` - Template in documentatie

Shared Partials:
- ✅ `Views/Shared/_GameNightCard.cshtml` - AANGEMAAKT
- ✅ `Views/Shared/_BoardGameCard.cshtml` - AANGEMAAKT

#### Program.cs
- ✅ Services geregistreerd in DI container

---

## 🔨 WAT JE NOG MOET DOEN

### Stap 1: Views Kopiëren
Kopieer alle view templates uit `FASE2_VIEWS_COMPLETE.md` naar de juiste locaties.

**Tip:** Volg de "Locatie:" aangave onder elke view in het documentatiebestand.

### Stap 2: Layout Updaten
Update `Views/Shared/_Layout.cshtml`:

1. **Voeg Font Awesome toe in `<head>` sectie:**
```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
```

2. **Update de navbar** (zie FASE2_VIEWS_COMPLETE.md voor volledige code)
   - Dropdown voor Spelavonden
   - Link naar Bordspellen
   - Link naar Profiel

### Stap 3: Entities Controleren
Zorg dat deze methodes/constructors bestaan in Domain entiteiten:

#### GameNight Entity
```csharp
public GameNight(string title, string? description, DateTime startAt, Address address, int maxPlayers, int organizerId)
{
    Title = title;
    Description = description;
    StartAt = startAt;
    Address = address;
    MaxPlayers = maxPlayers;
    OrganizerId = organizerId;
    BoardGames = new List<GameNightBoardGame>();
    Players = new List<GameNightPlayer>();
}

public void AddBoardGame(BoardGame boardGame)
{
    BoardGames.Add(new GameNightBoardGame 
    { 
        GameNightId = this.Id,
        BoardGameId = boardGame.Id,
        BoardGame = boardGame
    });
}

public void Recalculate18Plus()
{
    Is18Plus = BoardGames.Any(gnbg => gnbg.BoardGame.Is18Plus);
}
```

#### Address Value Object
```csharp
public void UpdateAddress(string street, string houseNumber, string city, string postalCode)
{
    Street = street;
    HouseNumber = houseNumber;
    City = city;
    PostalCode = postalCode;
}
```

#### GameNightPlayer Entity
```csharp
public GameNightPlayer(int gameNightId, int personId)
{
    GameNightId = gameNightId;
    PersonId = personId;
}
```

### Stap 4: Database Migration
Als je nieuwe properties hebt toegevoegd (zoals HasLactoseFree, etc.), maak dan een nieuwe migratie:

```powershell
cd BordspelApp.Infrastructure
dotnet ef migrations add Phase2Updates --startup-project ../BordspelApp.WebApp
dotnet ef database update --startup-project ../BordspelApp.WebApp
```

### Stap 5: Testen
Start de applicatie en test alle User Stories:

**US_01: Overzichten**
- [ ] Ga naar /GameNights/All → zie lijst met filter/zoek
- [ ] Login en ga naar /GameNights/Mine → zie jouw avonden
- [ ] Ga naar /GameNights/Participating → zie inschrijvingen
- [ ] Klik op Details → zie volledige info

**US_02: Organiseren**
- [ ] Maak account aan en login
- [ ] Probeer avond te maken als <18 → fout
- [ ] Maak avond aan als 18+ → succes
- [ ] Probeer te bewerken na inschrijvingen → geblokkeerd
- [ ] Probeer te verwijderen na inschrijvingen → geblokkeerd

**US_03: 18+ Logica**
- [ ] Maak avond met 18+ spel → automatisch 18+ badge
- [ ] Probeer in te schrijven als <18 → geblokkeerd

**US_04: Inschrijven**
- [ ] Schrijf in voor avond → succes
- [ ] Probeer dubbel in te schrijven → fout
- [ ] Schrijf in voor volle avond → fout
- [ ] Schrijf in voor 2 avonden op zelfde dag → fout

**US_05: Spellen**
- [ ] Ga naar /BoardGames → zie lijst
- [ ] Klik op spel → zie details + avonden

**US_06: Dieetwensen**
- [ ] Stel dieetwensen in profiel in
- [ ] Schrijf in voor avond zonder matching opties → waarschuwing
- [ ] Bevestig → ingeschreven
- [ ] Annuleer → niet ingeschreven

---

## 📁 BESTANDSSTRUCTUUR OVERZICHT

```
BordspelApp.Application/
├── DTOs/
│   ├── GameNightListItemDto.cs ✅
│   ├── GameNightDetailsDto.cs ✅
│   ├── BoardGameDto.cs ✅
│   └── PersonDto.cs ✅
├── Models/
│   └── RegistrationResponse.cs ✅
└── Interfaces/
    ├── IGameNightService.cs ✅
    ├── IRegistrationService.cs ✅
    ├── IBoardGameService.cs ✅
    └── IPersonService.cs ✅

BordspelApp.Infrastructure/
└── Services/
    ├── GameNightService.cs ✅
    ├── RegistrationService.cs ✅
    ├── BoardGameService.cs ✅
    └── PersonService.cs ✅

BordspelApp.WebApp/
├── Controllers/
│   ├── GameNightsController.cs ✅
│   ├── BoardGamesController.cs ✅
│   └── ProfileController.cs ✅
├── Models/
│   ├── GameNightListViewModel.cs ✅
│   ├── GameNightDetailsViewModel.cs ✅
│   ├── GameNightCreateViewModel.cs ✅
│   ├── RegistrationConfirmViewModel.cs ✅
│   ├── BoardGameViewModel.cs ✅
│   └── ProfileEditViewModel.cs ✅
├── Views/
│   ├── GameNights/
│   │   ├── All.cshtml ✅
│   │   ├── Mine.cshtml 📝
│   │   ├── Participating.cshtml 📝
│   │   ├── Details.cshtml 📝
│   │   ├── Create.cshtml 📝
│   │   ├── Edit.cshtml 📝
│   │   ├── Delete.cshtml 📝
│   │   └── ConfirmRegistration.cshtml 📝
│   ├── BoardGames/
│   │   ├── Index.cshtml 📝
│   │   └── Details.cshtml 📝
│   ├── Profile/
│   │   └── Edit.cshtml 📝
│   └── Shared/
│       ├── _GameNightCard.cshtml ✅
│       ├── _BoardGameCard.cshtml ✅
│       └── _Layout.cshtml ⚠️ (update nodig)
├── Program.cs ✅
└── wwwroot/
    └── css/
        └── site.css ⚠️ (optionele updates)
```

**Legenda:**
- ✅ Volledig geïmplementeerd
- 📝 Template beschikbaar in FASE2_VIEWS_COMPLETE.md
- ⚠️ Moet worden geüpdatet

---

## 🎯 BUSINESS RULES CHECKLIST

### Autorisatie
- [x] Iedereen kan lijsten bekijken
- [x] Alleen ingelogd: inschrijven/uitschrijven
- [x] Alleen 18+: organiseren (create/edit/delete)

### Organiseren
- [x] Leeftijd organisator >= 18
- [x] Edit/Delete alleen zonder spelers
- [x] Datum/tijd in toekomst
- [x] MaxPlayers > 0
- [x] Auto-herberekening 18+ na spellen toevoegen

### 18+ Logica
- [x] Auto 18+ als een spel 18+ is
- [x] 18+ badge overal zichtbaar
- [x] Blokkeer <18 voor 18+ avonden

### Inschrijven
- [x] Niet als vol (CurrentPlayers >= MaxPlayers)
- [x] Niet als <18 bij 18+ avond
- [x] Niet als al ingeschreven
- [x] Niet als andere avond op zelfde dag (DateOnly vergelijking)
- [x] Waarschuwing bij dieetwens mismatch
- [x] Mogelijkheid om door te gaan na waarschuwing

### Dieetwensen
- [x] Opslaan in Person profiel
- [x] Vergelijken met GameNight opties
- [x] Concrete mismatches tonen
- [x] Gebruiker kan kiezen om door te gaan

---

## 🧪 UNIT TEST VOORBEREIDING (FASE 3)

De services zijn al klaar voor unit testing:
- Dependency Injection via interfaces
- Business logic in services (niet controllers)
- Repositories kunnen gemocked worden
- Duidelijke return types en statuscodes

Test scenarios om later te implementeren:
```csharp
// GameNightService
- CreateAsync met 18+ spel → Is18Plus = true
- UpdateAsync met spelers → false
- DeleteAsync met spelers → false

// RegistrationService
- RegisterAsync als vol → RegistrationResult.GameNightFull
- RegisterAsync als <18 bij 18+ → RegistrationResult.AgeRestriction
- RegisterAsync zelfde dag → RegistrationResult.AlreadyRegisteredSameDay
- RegisterAsync dieet mismatch → RegistrationResult.DietaryMismatch
- RegisterAsync met forceRegister → RegistrationResult.Success

// PersonService
- GetAgeAsync → correcte leeftijd berekenen
- UpdateProfileAsync → success
```

---

## 🚀 START INSTRUCTIES

1. **Open project in Visual Studio / VS Code**
2. **Kopieer alle views** uit FASE2_VIEWS_COMPLETE.md
3. **Update _Layout.cshtml** met navigatie en Font Awesome
4. **Check Domain entities** voor ontbrekende methodes
5. **Run migrations** als needed
6. **Start applicatie:**
   ```powershell
   cd BordspelApp.WebApp
   dotnet run
   ```
7. **Test alle User Stories** volgens checklist hierboven

---

## 📞 TROUBLESHOOTING

### Compile Errors
**Probleem:** Services niet gevonden
**Oplossing:** Check of alle using statements kloppen en services in Program.cs staan

**Probleem:** ViewModels niet gevonden in views
**Oplossing:** Check namespace in @model directive

### Runtime Errors
**Probleem:** NullReferenceException bij GetCurrentPersonIdAsync
**Oplossing:** Zorg dat Person.Email overeenkomt met Identity User email

**Probleem:** Database errors bij Create/Update
**Oplossing:** Run migrations opnieuw, check dat alle navigatie properties correct zijn

**Probleem:** Validatie werkt niet
**Oplossing:** Check dat _ValidationScriptsPartial in @section Scripts staat

### UI Issues
**Probleem:** Icons niet zichtbaar
**Oplossing:** Check Font Awesome CDN link in _Layout.cshtml

**Probleem:** Bootstrap styling niet correct
**Oplossing:** Check dat Bootstrap 5 wordt gebruikt (niet 4)

---

## 🎉 KLAAR VOOR FASE 3!

Na het voltooien van bovenstaande stappen heb je:
- ✅ Volledige MVC implementatie voor US_01 t/m US_06
- ✅ Clean Architecture met Onion pattern
- ✅ Alle business rules geïmplementeerd
- ✅ User-friendly UI met Bootstrap en Font Awesome
- ✅ Validatie (client & server-side)
- ✅ Authorization en Authentication
- ✅ Unit-testbare code klaar voor Fase 3

**Veel succes!** 🚀
