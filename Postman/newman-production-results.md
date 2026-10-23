# Newman productieverificatie

Datum: 23 oktober 2026  
API: `https://api.13.60.219.216.sslip.io`  
Muterende scenario's: uitgeschakeld (`allowMutations=false`)

## Resultaat

- Requests in de collectie: 31
- Uitgevoerde requests: 19
- Bewust overgeslagen muterende requests: 12
- Assertions: 22
- Geslaagde assertions: 21
- Failures: 1

De authenticatiecookie, openbare REST-reads, ontbrekende resources, GraphQL-query's en unauthenticated join-route zijn succesvol gecontroleerd. Credentials zijn uitsluitend runtime aangeleverd en staan niet in dit bestand.

## Openstaand probleem

`GET /api/gamenights/upcoming` geeft via curl HTTP 200 terug, maar Newman meldt een voortijdig beëindigde chunked response (`aborted`). Dit is een probleem in de online response/proxyketen en is niet als geslaagd geregistreerd. De overige uitgevoerde assertions slaagden.

De muterende endpoints zijn in deze productierun bewust niet uitgevoerd om bestaande assessmentdata te beschermen. Voer die scenario's alleen uit met `allowMutations=true` tegen een geïsoleerde testdatabase.
