# BordspelApp API tests

Import `BordspelApp-API.postman_collection.json` and `BordspelApp-local.postman_environment.json` into Postman.

1. Start the API locally on `http://localhost:5091` or change `baseUrl`.
2. Fill `loginEmail` and `loginPassword` locally or in a private Postman environment. Do not commit credentials.
3. Run `Authentication / Login` first. ASP.NET Core Identity returns a cookie that Postman reuses automatically.
4. Run the remaining folders. `gameNightId` must refer to an existing local game night.

The collection includes public REST reads, authenticated mutations, GraphQL list queries, and expected authentication failures. The API uses an Identity cookie rather than JWT because JWT is not required by the case; Newman/Postman cookie persistence provides the automatic authentication flow.
