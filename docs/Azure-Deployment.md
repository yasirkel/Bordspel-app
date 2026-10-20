# Azure deployment runbook

Dit document beschrijft de veilige configuratie voor de assessmentomgeving. De workflows deployen alleen vanaf `release`; een protected GitHub Environment met naam `production` moet vóór gebruik een verplichte goedkeuring hebben.

## Verwachte resources en instellingen

De workflowbestanden gebruiken deze App Service-namen:

- WebApp: `bordspel-app`
- API: `bordspel-api`

De daadwerkelijke aanwezigheid en configuratie van deze resources is vanuit deze lokale checkout niet geverifieerd.

Configureer op beide App Services de volgende productie-instellingen als App Service configuration values, niet in Git:

```text
ConnectionStrings__DomainConnection=<domain Azure SQL connection string>
ConnectionStrings__IdentityConnection=<identity Azure SQL connection string>
SeedDatabase=false
ASPNETCORE_ENVIRONMENT=Production
```

Gebruik afzonderlijke Azure SQL-databases voor domeindata en Identity. De connection strings moeten minimaal verwijzen naar:

- `DomainConnection`: Persons, BoardGames, GameNights, deelnemers en PotluckItems.
- `IdentityConnection`: Identity users, roles, claims en authenticatietabellen.

De benodigde GitHub Actions secrets zijn:

- `AZURE_WEBAPP_PUBLISH_PROFILE_WEB`
- `AZURE_WEBAPP_PUBLISH_PROFILE_API`
- `AZURE_DOMAIN_SQL_CONNECTION_STRING`
- `AZURE_IDENTITY_SQL_CONNECTION_STRING`

Publish profiles en SQL-wachtwoorden mogen nooit in de repository, workflow-output of screenshots terechtkomen.

## Pipelinegedrag

`ci.yml` restore’t, bouwt en test de volledige solution op pushes naar `master`, `development`, `release` en `fix/**`, en op pull requests naar de drie hoofdbranches. De build gebruikt `--warnaserror`; een foutieve build of test stopt de pipeline.

De twee release-workflows bouwen en testen eerst de volledige solution. Daarna voeren zij de domein- en Identity-migraties als afzonderlijke EF-contexten uit en publiceren pas daarna de betreffende App Service. De migraties worden dus niet vanuit `Program.cs` uitgevoerd. Beide workflows gebruiken dezelfde concurrency group, zodat twee release-deployments niet gelijktijdig databasewijzigingen uitvoeren.

De productie-environment moet approval gates en bij voorkeur branch protection bevatten. Een handmatige `workflow_dispatch` is geen vervanging voor die goedkeuring.

## Veilig migratieplan voor bestaande data

De oorspronkelijke gecombineerde database mag niet worden gereset of verwijderd. Voer vóór een eerste Azure-migratie de volgende gecontroleerde stappen uit:

1. Maak een verifieerbare backup van de bestaande database en leg restore-instructies vast.
2. Inventariseer tabellen, aantallen, Identity-records en de huidige migratiehistorie.
3. Maak de nieuwe domein- en Identity-databases aan zonder de brondatabase te wijzigen.
4. Test de migraties eerst tegen lege databases met `dotnet ef database update`.
5. Maak een expliciet data-migratiescript voor Persons, domeinentiteiten en Identity-records. Controleer mappings en aantallen vóór import.
6. Voer een dry-run/validatie uit in een aparte omgeving en vergelijk de resultaten met de backup.
7. Laat de data-eigenaar de cut-over goedkeuren; voer daarna pas de productie-import uit.
8. Bewaar de oorspronkelijke database en backup totdat registratie, login, domeinflows en rollback zijn gevalideerd.

Dit repository bevat geen script dat bestaande productiegegevens kopieert of verwijdert. De daadwerkelijke productie-migratie blijft daarom een handmatige, goedgekeurde operatie.

## Verificatie na configuratie

Controleer vóór de demo:

1. De secrets bestaan en zijn alleen beschikbaar voor de beschermde `production` environment.
2. Beide App Services gebruiken de juiste connection strings en `SeedDatabase=false`.
3. De twee migratiecommando’s slagen afzonderlijk en de `__EFMigrationsHistory`-tabellen staan in de juiste databases.
4. WebApp en API starten op en tonen geen connection-string of wachtwoord in logs.
5. Controleer `/swagger`, `/swagger/v1/swagger.json` en `/graphql` op de API.
6. Voer de Postman-collectie uit met uitsluitend testaccounts en testdata. Newman was niet beschikbaar in de lokale omgeving tijdens deze fase; installeer Newman of gebruik Postman CLI vóór de Azure-demo.
7. Test registratie, login/logout, profielkoppeling, aanmaken/deelnemen/uitschrijven, leeftijd/capaciteit en potluck.

Azure-resources, bestaande clouddata, secrets, workflowhistorie en een echte Azure-deployment zijn vanuit de lokale omgeving niet geverifieerd. Geen productiegegevens zijn door deze fase gewijzigd.
