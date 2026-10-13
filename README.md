# Pokémon Explorer

A small Angular and ASP.NET Core app that maps the first page of PokéAPI's Pokémon list into a frontend-friendly contract. Backend and browser tests use shared scenario fixtures, so automated tests never call PokéAPI.

## Requirements

- .NET 8 SDK
- Node.js 20.19+, 22.12+, or 24+

## Run locally

From the repository root, run this script to open the API and frontend in separate PowerShell windows. It installs frontend dependencies with `npm ci` the first time if needed:

```powershell
.\run-apps.ps1
```

The API is available at http://localhost:5000 and the Angular app at http://localhost:4200. Close each app's PowerShell window to stop it. To start either app manually instead:

```powershell
dotnet run --project backend\Pokemon.Api -- --urls http://localhost:5000
# In another terminal:
Push-Location frontend
npm ci
npm start
Pop-Location
```

The page requests `/api/pokemon`; the API calls `https://pokeapi.co/api/v2/pokemon` with PokéAPI's default pagination and returns `{ "items": [{ "id": 1, "name": "bulbasaur" }], "count": 1302 }`. `count` is the upstream total number of Pokémon, not the number on the current page. IDs are derived from resource URLs and are `null` if a URL has no valid ID.

## Tests

From the repository root, run backend tests:

```powershell
dotnet test backend\Pokemon.sln
```

Run the frontend build and Playwright scenarios:

```powershell
cd frontend
npm ci
npm run build
npx playwright install chromium
npm run test:e2e
```

The Playwright dev server only serves the frontend; tests intercept `/api/pokemon` and load fixtures from `testfixtures`. xUnit supplies raw PokéAPI response fixtures through a fake HTTP handler. Fixture files are test-only and are not read by production code.

## Project layout

- `frontend/` — standalone Angular app and Playwright end-to-end tests
- `backend/Pokemon.Api/` — `GET /api/pokemon` and HTTP error mapping
- `backend/Pokemon.Domain/` — application Pokémon and list contracts
- `backend/Pokemon.Infrastructure/` — PokéAPI client and upstream response mapping
- `backend/Pokemon.Tests/` — fixture-driven xUnit tests
- `testfixtures/` — separate backend and frontend data for each scenario
