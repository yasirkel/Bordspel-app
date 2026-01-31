Configuration for API base URL

- Files added for developer convenience:
  - `Environments/environment.ts` : development-friendly TS config (local use)
  - `Environments/environment.production.ts` : production-friendly TS config (reference)

- Runtime behaviour (Razor webapp):
  - The app exposes a client-side object `window.environment` (in the layout). It contains:
    - `production` (boolean)
    - `apiBaseUrl` (string)
  - The value of `apiBaseUrl` is chosen by priority:
    1. Azure App Setting / environment variable `API_BASE_URL` (recommended for production)
    2. `Api:BaseUrl` from `appsettings.json` / `appsettings.Production.json`
    3. Fallback `http://localhost:5247/` for development

- DI / server-side usage:
  - `ApiOptions` binds `Api:BaseUrl` from configuration.
  - `IApiBaseUrlProvider` reads the effective base URL (respecting `API_BASE_URL`).
  - The named HttpClient `api` uses this provider for its `BaseAddress`.

Azure App Settings to set (in Portal -> Configuration):

- `API_BASE_URL` : https://<your-api-app>.azurewebsites.net/

Local run:

1. Ensure your API runs on the expected local port (default `http://localhost:5247/`).
2. Start the WebApp (from solution root):

```powershell
Push-Location 'e:/Avans/Leerjaar 2/periode2/bordspel-app/BordspelApp.WebApp'
dotnet run
Pop-Location
```

Notes:
- If you use a separate frontend build with a bundler, you can copy/consume the `Environments/*.ts` files into your build and configure your bundler to replace them for production similarly to Angular's `fileReplacements`. For this Razor app the runtime `window.environment` is populated for client scripts.
