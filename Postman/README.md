# BordspelApp API tests

Import `BordspelApp-API.postman_collection.json` and `BordspelApp-local.postman_environment.json` into Postman.

1. Start the API locally on `http://localhost:5091` or change `baseUrl`.
2. Fill `loginEmail` and `loginPassword` locally or in a private Postman environment. Do not commit credentials.
3. Run `Authentication / Login` first. ASP.NET Core Identity returns a cookie that Postman reuses automatically.
4. Run the remaining folders. `gameNightId` must refer to an existing local game night.

The collection includes public REST reads, authenticated game-night CRUD, GraphQL list and recommendation queries, and expected authentication failures. The API uses an Identity cookie rather than JWT because JWT is not required by the case; Postman cookie persistence provides the automatic authentication flow.

For a remote run, set `baseUrl` to the public API URL and fill the login variables only in a private environment. The temporary game night created by the authenticated folder is deleted again by the final request when creation succeeds. The collection does not delete existing assessment data.

Newman can be used when it is installed locally:

```text
newman run Postman/BordspelApp-API.postman_collection.json -e Postman/BordspelApp-local.postman_environment.json
```
