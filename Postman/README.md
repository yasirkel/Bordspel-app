# BordspelApp API tests

Import `BordspelApp-API.postman_collection.json` and `BordspelApp-local.postman_environment.json` into Postman.

1. Start the API locally on `http://localhost:5091` or change `baseUrl`.
2. Fill `loginEmail` and `loginPassword` locally or in a private Postman environment. Do not commit credentials.
3. Run `Authentication / Login` first. ASP.NET Core Identity returns a cookie that Postman reuses automatically.
4. Run the remaining folders. `gameNightId` must refer to an existing local game night.

The collection covers all 22 REST operations exposed by the API and all three GraphQL queries. It includes public reads, authenticated game-night CRUD and participation routes, admin board-game/person operations, Identity login/logout, validation failures and authorization failures. The API uses an Identity cookie rather than JWT because JWT is not required by the case; Postman/Newman cookie persistence provides the automatic authentication flow.

For a remote run, set `baseUrl` to the public API URL and fill the login variables only in a private environment. The temporary game night created by the authenticated folder is deleted again by the final request when creation succeeds. The collection does not delete existing assessment data.

Mutating requests have a pre-request guard and are skipped when `allowMutations=false` (the safe production default). Set it to `true` only against an isolated database or a deliberately prepared disposable environment. Those requests create uniquely named temporary records and contain cleanup requests; they must not be enabled against the assessment database without checking the run order and cleanup result.

Newman can be used when it is installed locally:

```text
newman run Postman/BordspelApp-API.postman_collection.json -e Postman/BordspelApp-local.postman_environment.json
```

Provide `loginEmail`, `loginPassword`, `adminEmail` and `adminPassword` as runtime variables or in a private environment. Never commit those values.
