# FASE 2 - IMPLEMENTATIE OVERZICHT

## ✅ VOLTOO

ID: Services en DTOs zijn aangemaakt
## ✅ VOLTOOID: Application Layer
- DTOs (GameNightListItemDto, GameNightDetailsDto, BoardGameDto, PersonDto)
- Models (RegistrationResponse, RegistrationResult)
- Interfaces (IGameNightService, IRegistrationService, IBoardGameService, IPersonService)

## ✅ VOLTOOID: Infrastructure Layer
- Services (GameNightService, RegistrationService, BoardGameService, PersonService)

## ✅ VOLTOOID: WebApp - ViewModels
- GameNightListViewModel
- GameNightDetailsViewModel
- GameNightCreateViewModel (met FutureDateAttribute)
- RegistrationConfirmViewModel
- BoardGameViewModel
- ProfileEditViewModel

## ✅ VOLTOOID: WebApp - Controllers
- GameNightsController (met alle CRUD + registratie logica)
- BoardGamesController
- ProfileController

## 🔨 NOG TE DOEN: Razor Views

### GameNights Views (in Views/GameNights/)
1. **All.cshtml** - Lijst van alle komende spelavonden met filter/zoek/paginatie
2. **Mine.cshtml** - Mijn georganiseerde avonden
3. **Participating.cshtml** - Spelavonden waaraan ik deelneem
4. **Details.cshtml** - Detailpagina met inschrijven/uitschrijven knoppen
5. **Create.cshtml** - Formulier om spelavond aan te maken
6. **Edit.cshtml** - Formulier om spelavond te bewerken
7. **Delete.cshtml** - Bevestigingspagina voor verwijderen
8. **ConfirmRegistration.cshtml** - Waarschuwing dieetwensen

### BoardGames Views (in Views/BoardGames/)
1. **Index.cshtml** - Lijst van alle bordspellen (kaarten)
2. **Details.cshtml** - Detailpagina met spelavonden waar spel speelt

### Profile Views (in Views/Profile/)
1. **Edit.cshtml** - Profiel bewerken (adres + dieetwensen)

### Partial Views (in Views/Shared/)
1. **_GameNightCard.cshtml** - Herbruikbare kaart voor spelavond
2. **_BoardGameCard.cshtml** - Herbruikbare kaart voor bordspel
3. **_Navigation.cshtml** - Update navigatie met nieuwe links

## 🔨 NOG TE DOEN: Program.cs - Service Registration

Voeg toe aan Program.cs:
```csharp
// Services registreren
builder.Services.AddScoped<IGameNightService, GameNightService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IBoardGameService, BoardGameService>();
builder.Services.AddScoped<IPersonService, PersonService>();
```

## 🔨 NOG TE DOEN: Domain Entiteiten Updates

Mogelijk zijn er nog aanpassingen nodig aan entiteiten:

### Address Value Object
Zorg dat `UpdateAddress` methode bestaat:
```csharp
public void UpdateAddress(string street, string houseNumber, string city, string postalCode)
{
    Street = street;
    HouseNumber = houseNumber;
    City = city;
    PostalCode = postalCode;
}
```

### GameNight Entiteit
- Constructor met alle parameters
- `AddBoardGame(BoardGame boardGame)` methode
- `Recalculate18Plus()` methode

## 📝 BELANGRIJKE TECHNISCHE DETAILS

### Authenticatie Flow
De huidige implementatie gebruikt `ClaimTypes.Email` om de gebruiker te identificeren en dan via `IPersonService.GetByEmailAsync()` de Person op te halen. Dit veronderstelt dat:
1. Identity User email overeenkomt met Person.Email
2. Bij registratie wordt een Person aangemaakt met hetzelfde email

### Business Rules Implementatie
✅ 18+ organisator check - in Create/Edit acties
✅ 18+ avond check - automatisch via Recalculate18Plus()
✅ Vol check - in RegistrationService
✅ Leeftijd check - in RegistrationService
✅ Zelfde dag check - in RegistrationService (DateOnly vergelijking)
✅ Dieetwensen check - in RegistrationService met DietaryMismatch flow
✅ Edit/Delete alleen zonder spelers - in GameNightService.CanEditOrDeleteAsync()

### Validatie
✅ Client-side validatie via DataAnnotations
✅ Server-side validatie via ModelState
✅ Custom validatie attribute voor toekomstige datum
✅ AntiForgeryToken op alle POST acties

### Error Handling
✅ TempData voor success/error messages
✅ ModelState errors voor validatie
✅ NotFound() voor niet-bestaande entiteiten
✅ Redirect naar veilige pagina's bij fouten

## 🎨 UI/UX VEREISTEN VOOR VIEWS

### Algemeen
- Bootstrap 5 voor styling
- Font Awesome voor iconen
- Responsive design
- Validation summary op formulieren
- Flash messages met TempData

### Badges
- 🔞 18+ badge (rood/oranje)
- 🎯 Vol badge (grijs)
- ✅ Ingeschreven badge (groen)
- 🍃 Vegetarisch, 🥜 Notenvrij, 🥛 Lactosevrij, 🍷 Alcoholvrij

### Call-to-Actions
- Primaire knoppen voor belangrijke acties (Inschrijven, Aanmaken)
- Secondary knoppen voor neutrale acties (Annuleren, Terug)
- Danger knoppen voor destructieve acties (Verwijderen, Uitschrijven)

### Lege Staten
- Friendly bericht bij lege lijsten
- Call-to-action om te beginnen
- Illustratie of icoon (optioneel)

### Formulieren
- Labels boven inputs
- Placeholder tekst waar zinvol
- Help text onder velden waar nodig
- Multi-select voor bordspellen (select2 of Bootstrap select)
- Datepicker voor datum/tijd

## 🧪 TESTING SCENARIOS

1. **Organiseren**
   - 18+ gebruiker kan avond aanmaken
   - <18 gebruiker krijgt foutmelding
   - Bewerken lukt alleen zonder spelers
   - Verwijderen lukt alleen zonder spelers

2. **Inschrijven**
   - Normale inschrijving lukt
   - Vol avond blokkeert inschrijving
   - <18 bij 18+ avond blokkeert
   - Zelfde dag blokkeert
   - Dieetwensen tonen waarschuwing
   - Na waarschuwing kan doorgaan

3. **Zoeken/Filteren**
   - Zoeken op stad werkt
   - Zoeken op organisator werkt
   - Paginatie werkt
   - Alleen toekomstige avonden tonen

4. **18+ Logica**
   - Avond met 18+ spel wordt automatisch 18+
   - Badge toont correct
   - Inschrijving blokkeert voor minderjarigen

## 📦 DEPENDENCIES

Controleer of deze packages geïnstalleerd zijn:
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer (of je database provider)
- Microsoft.EntityFrameworkCore.Tools

## 🚀 DEPLOYMENT CHECKLIST

- [ ] Alle services geregistreerd in DI
- [ ] Alle views aangemaakt
- [ ] Partial views herbruikbaar
- [ ] Navigation menu updated
- [ ] Seed data aanwezig
- [ ] Migrations up-to-date
- [ ] Identity geconfigureerd
- [ ] Authorization policies ingesteld
- [ ] Error handling en logging
- [ ] Client-side validatie scripts

## 📚 DOCUMENTATIE VOOR FASE 3

Voor Fase 3 (Unit Testing) zijn de services al unit-testbaar omdat:
- Business logic zit in services (niet in controllers)
- Dependencies zijn geïnjecteerd via interfaces
- Services gebruiken repositories (kunnen gemocked worden)
- Geen static methods of tight coupling

Test scenarios voor Fase 3:
- GameNightService.CreateAsync met verschillende scenario's
- RegistrationService.RegisterAsync met alle RegistrationResult types
- 18+ logica in verschillende contexten
- Datum validatie en zelfde-dag checks
- Dieetwensen mismatch detectie
