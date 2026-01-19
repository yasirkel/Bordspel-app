# SNELSTART GIDS - FASE 2 VOLTOOIEN

## 🎯 DOEL
Deze gids helpt je om in 30 minuten Fase 2 volledig werkend te krijgen.

---

## ⚡ STAP-VOOR-STAP INSTRUCTIES

### STAP 1: Views Aanmaken (15 min)
Open `FASE2_VIEWS_COMPLETE.md` en kopieer alle views naar de juiste locaties:

#### GameNights Views
Maak deze bestanden aan en kopieer de code:

1. `Views/GameNights/Mine.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Mine.cshtml"
2. `Views/GameNights/Participating.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Participating.cshtml"
3. `Views/GameNights/Details.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Details.cshtml"
4. `Views/GameNights/Create.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Create.cshtml"
5. `Views/GameNights/Edit.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Edit.cshtml"
6. `Views/GameNights/Delete.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: Delete.cshtml"
7. `Views/GameNights/ConfirmRegistration.cshtml` - Kopieer van regel "## GAMENIGHT VIEW: ConfirmRegistration.cshtml"

#### BoardGames Views
8. `Views/BoardGames/Index.cshtml` - Kopieer van regel "## BOARDGAME VIEW: Index.cshtml"
9. `Views/BoardGames/Details.cshtml` - Kopieer van regel "## BOARDGAME VIEW: Details.cshtml"

#### Profile Views
10. `Views/Profile/Edit.cshtml` - Kopieer van regel "## PROFILE VIEW: Edit.cshtml"

**TIP:** De Views/Shared partials zijn al aangemaakt! ✅

---

### STAP 2: Layout Updaten (5 min)

Open `Views/Shared/_Layout.cshtml`

**2a. Voeg Font Awesome toe**
Zoek de `<head>` sectie en voeg toe VOOR de </head> tag:
```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
```

**2b. Update de Navigation**
Zoek de `<nav>` sectie en vervang de huidige nav-items door:
```razor
<ul class="navbar-nav flex-grow-1">
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
    </li>
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle text-dark" href="#" id="navbarDropdown" role="button" 
           data-bs-toggle="dropdown" aria-expanded="false">
            <i class="fas fa-calendar"></i> Spelavonden
        </a>
        <ul class="dropdown-menu" aria-labelledby="navbarDropdown">
            <li><a class="dropdown-item" asp-controller="GameNights" asp-action="All">
                <i class="fas fa-list"></i> Alle Spelavonden
            </a></li>
            @if (User.Identity?.IsAuthenticated == true)
            {
                <li><hr class="dropdown-divider"></li>
                <li><a class="dropdown-item" asp-controller="GameNights" asp-action="Mine">
                    <i class="fas fa-user-tie"></i> Mijn Georganiseerde Avonden
                </a></li>
                <li><a class="dropdown-item" asp-controller="GameNights" asp-action="Participating">
                    <i class="fas fa-user-check"></i> Mijn Inschrijvingen
                </a></li>
                <li><hr class="dropdown-divider"></li>
                <li><a class="dropdown-item" asp-controller="GameNights" asp-action="Create">
                    <i class="fas fa-plus"></i> Nieuwe Spelavond
                </a></li>
            }
        </ul>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-controller="BoardGames" asp-action="Index">
            <i class="fas fa-dice"></i> Bordspellen
        </a>
    </li>
    @if (User.Identity?.IsAuthenticated == true)
    {
        <li class="nav-item">
            <a class="nav-link text-dark" asp-controller="Profile" asp-action="Edit">
                <i class="fas fa-user-cog"></i> Profiel
            </a>
        </li>
    }
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
    </li>
</ul>
```

---

### STAP 3: Domain Entities Checken (5 min)

#### 3a. Check GameNight Entity
Open `BordspelApp.Domain/Entities/GameNight.cs`

Zorg dat deze methodes bestaan:
```csharp
// Constructor
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

// AddBoardGame methode
public void AddBoardGame(BoardGame boardGame)
{
    var link = new GameNightBoardGame 
    { 
        GameNightId = this.Id,
        BoardGameId = boardGame.Id,
        BoardGame = boardGame,
        GameNight = this
    };
    BoardGames.Add(link);
}

// Recalculate18Plus methode
public void Recalculate18Plus()
{
    Is18Plus = BoardGames.Any(gnbg => gnbg.BoardGame != null && gnbg.BoardGame.Is18Plus);
}
```

**Als deze niet bestaan:** Voeg ze toe aan de GameNight class.

#### 3b. Check Address Value Object
Open `BordspelApp.Domain/ValueObjects/Address.cs` (of Entities/Address.cs)

Zorg dat deze methode bestaat:
```csharp
public void UpdateAddress(string street, string houseNumber, string city, string postalCode)
{
    Street = street;
    HouseNumber = houseNumber;
    City = city;
    PostalCode = postalCode;
}
```

**Als deze niet bestaat:** Voeg toe aan de Address class.

#### 3c. Check GameNightPlayer Constructor
Open `BordspelApp.Domain/Entities/GameNightPlayer.cs`

Zorg dat deze constructor bestaat:
```csharp
public GameNightPlayer(int gameNightId, int personId)
{
    GameNightId = gameNightId;
    PersonId = personId;
}

// En een parameterloze constructor voor EF
public GameNightPlayer() { }
```

---

### STAP 4: Database Properties Checken (2 min)

Open `BordspelApp.Domain/Entities/GameNight.cs`

Controleer of deze properties bestaan:
```csharp
public bool HasLactoseFree { get; set; }
public bool HasNutFree { get; set; }
public bool HasVegetarian { get; set; }
public bool HasAlcoholFree { get; set; }
```

Open `BordspelApp.Domain/Entities/Person.cs`

Controleer of deze properties bestaan:
```csharp
public bool LactoseFree { get; set; }
public bool NutFree { get; set; }
public bool Vegetarian { get; set; }
public bool NoAlcohol { get; set; }
```

**Als properties ontbreken:** Voeg ze toe en maak een nieuwe migratie (zie Stap 5).

---

### STAP 5: Database Migration (indien nodig) (3 min)

**Alleen doen als je properties hebt toegevoegd in Stap 4!**

Open een terminal in de root van je solution en voer uit:

```powershell
cd BordspelApp.Infrastructure
dotnet ef migrations add Fase2DietaryPreferences --startup-project ../BordspelApp.WebApp
dotnet ef database update --startup-project ../BordspelApp.WebApp
```

---

### STAP 6: Applicatie Starten en Testen (Onbeperkt)

Start de applicatie:
```powershell
cd BordspelApp.WebApp
dotnet run
```

Of gebruik F5 in Visual Studio.

#### Test Checklist:

**Basis navigatie:**
- [ ] Open https://localhost:5001
- [ ] Klik op "Spelavonden" dropdown → werkt
- [ ] Klik op "Bordspellen" → Index laadt
- [ ] Klik op "Alle Spelavonden" → All.cshtml laadt

**Als niet ingelogd:**
- [ ] Ga naar /GameNights/All → lijst zichtbaar
- [ ] Klik op een spelavond → Details zichtbaar
- [ ] "Inloggen" knop zichtbaar (geen inschrijven)

**Registreer en login:**
- [ ] Klik "Register" en maak account
- [ ] Login met je account
- [ ] Koppel je account aan een Person (als dat nog niet automatisch is)

**Test User Stories:**

**US_01: Lijsten bekijken**
- [ ] /GameNights/All → lijst met spelavonden
- [ ] Filter op stad → werkt
- [ ] /GameNights/Mine → jouw georganiseerde avonden (leeg)
- [ ] /GameNights/Participating → jouw inschrijvingen (leeg)

**US_02: Organiseren**
- [ ] Klik "Nieuwe Spelavond"
- [ ] Als <18: foutmelding "moet 18+ zijn"
- [ ] Als 18+: formulier zichtbaar
- [ ] Vul formulier in, selecteer spellen
- [ ] Klik "Aanmaken" → succes!
- [ ] Ga naar "Mijn Georganiseerde Avonden" → je avond staat er

**US_03: 18+ Logica**
- [ ] Maak avond met 18+ spel → 18+ badge verschijnt automatisch
- [ ] Details bekijken → 18+ badge zichtbaar

**US_04: Inschrijven**
- [ ] Ga naar Details van een avond (niet jouw eigen)
- [ ] Klik "Inschrijven" → succes (of dieetwaarschuwing)
- [ ] Ga naar "Mijn Inschrijvingen" → avond staat er
- [ ] Probeer opnieuw in te schrijven → "al ingeschreven" fout
- [ ] Klik "Uitschrijven" → succes

**US_05: Bordspellen**
- [ ] Ga naar /BoardGames → lijst van spellen
- [ ] Klik op een spel → Details met avonden

**US_06: Dieetwensen**
- [ ] Klik "Profiel" in navigatie
- [ ] Vink dieetwensen aan (bijv. Vegetarisch)
- [ ] Klik "Opslaan" → succes
- [ ] Schrijf in voor avond zonder vegetarische optie
- [ ] Waarschuwingsscherm verschijnt met concrete mismatch
- [ ] Klik "Ja, Inschrijven" → ingeschreven
- [ ] Of klik "Annuleren" → niet ingeschreven

---

## ⚠️ VEELVOORKOMENDE PROBLEMEN EN OPLOSSINGEN

### Probleem: "Service not found" bij startup
**Oorzaak:** Services niet geregistreerd in DI
**Oplossing:** 
Check `Program.cs` - moeten deze regels bevatten:
```csharp
builder.Services.AddScoped<IGameNightService, BordspelApp.Infrastructure.Services.GameNightService>();
builder.Services.AddScoped<IRegistrationService, BordspelApp.Infrastructure.Services.RegistrationService>();
builder.Services.AddScoped<IBoardGameService, BordspelApp.Infrastructure.Services.BoardGameService>();
builder.Services.AddScoped<IPersonService, BordspelApp.Infrastructure.Services.PersonService>();
```

### Probleem: NullReferenceException in GetCurrentPersonIdAsync
**Oorzaak:** Geen Person gekoppeld aan Identity User
**Oplossing:**
1. Check of Person.Email overeenkomt met je Identity User email
2. Of maak in de seed data een Person met hetzelfde email als je test-user

### Probleem: Views niet gevonden (404)
**Oorzaak:** View op verkeerde locatie of verkeerde naam
**Oplossing:**
- Views moeten in `Views/{ControllerName}/{ActionName}.cshtml`
- Bijv: `Views/GameNights/All.cshtml` voor action `All` in `GameNightsController`
- Check hoofdletters/kleine letters!

### Probleem: Font Awesome icons niet zichtbaar
**Oorzaak:** CDN link niet toegevoegd
**Oplossing:**
Voeg toe aan `_Layout.cshtml` in `<head>`:
```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
```

### Probleem: Dropdown menu werkt niet
**Oorzaak:** Bootstrap JavaScript niet geladen
**Oplossing:**
Check dat Bootstrap JS in `_Layout.cshtml` staat voor </body>:
```html
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
```

### Probleem: Validatie werkt niet client-side
**Oorzaak:** Validation scripts niet geladen
**Oplossing:**
Check dat je views een @section Scripts hebben:
```razor
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### Probleem: "Cannot insert duplicate key" bij seed
**Oorzaak:** Database bevat al data
**Oplossing:**
```powershell
cd BordspelApp.Infrastructure
dotnet ef database drop --startup-project ../BordspelApp.WebApp
dotnet ef database update --startup-project ../BordspelApp.WebApp
```

---

## ✅ VOLTOOIINGS CHECKLIST

Vink af als voltooid:

**Code:**
- [ ] Alle 10 views aangemaakt uit FASE2_VIEWS_COMPLETE.md
- [ ] _Layout.cshtml geüpdatet (Font Awesome + navigatie)
- [ ] Domain entities hebben juiste methodes (Constructor, AddBoardGame, etc.)
- [ ] Program.cs heeft alle services geregistreerd
- [ ] Database properties aanwezig (dieetwensen)
- [ ] Migratie uitgevoerd (indien nodig)

**Testen:**
- [ ] Applicatie start zonder errors
- [ ] Navigatie werkt volledig
- [ ] Kan registreren en inloggen
- [ ] US_01: Alle lijsten werken
- [ ] US_02: CRUD werkt met validaties
- [ ] US_03: 18+ logica werkt
- [ ] US_04: Inschrijven werkt met alle checks
- [ ] US_05: Bordspellen tonen werkt
- [ ] US_06: Dieetwaarschuwing werkt

**Documentatie:**
- [ ] FASE2_STATUS.md gelezen
- [ ] FASE2_VIEWS_COMPLETE.md gebruikt
- [ ] FASE2_COMPLETE_SUMMARY.md doorgenomen

---

## 🎉 KLAAR!

Als alle checkboxes aangevinkt zijn, is Fase 2 compleet!

**Next steps:**
- Fase 3: Unit tests schrijven voor de services
- Extra features toevoegen (reviews, ratings, etc.)
- UI/UX verbeteren met custom CSS

**Veel succes!** 🚀
