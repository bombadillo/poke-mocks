# Pokémon Explorer MVP Plan

## 1. Overview

Build a small application with an Angular frontend, a C# ASP.NET Core API, and automated tests using Playwright and xUnit.

The API calls PokéAPI to retrieve the first page of Pokémon results, maps the upstream response into the application's own domain model, and returns that model to Angular.

Both test suites use scenario-based fixtures stored in a shared `testfixtures` directory:

- **Backend fixtures** contain raw PokéAPI response data and are used by xUnit tests.
- **Frontend fixtures** contain the mapped application API response and are used by Playwright tests.

The fixture files are shared through the repository, not as a runtime dependency. Production code should not read from `testfixtures`.

## 2. Architecture

The main request flow is:

1. Angular requests the Pokémon list from the C# API.
2. The API requests the first page from PokéAPI.
3. The API deserialises the upstream response and maps it into the application's domain model.
4. The API returns its own response contract to Angular.
5. Angular renders the Pokémon list.

The tests run independently of PokéAPI:

- xUnit tests supply raw PokéAPI responses through a fake HTTP handler.
- Playwright tests intercept the application's API endpoint and return frontend fixtures.

This keeps tests deterministic and avoids relying on the availability of the external service.

## 3. Repository structure

Use a monorepo so both test suites can access the same fixture library without duplicating files or publishing a separate package.

- `frontend/`
    - Angular application
    - Playwright tests and configuration
- `backend/`
    - `Pokemon.Api/` — HTTP endpoints and application composition
    - `Pokemon.Domain/` — domain models
    - `Pokemon.Infrastructure/` — PokéAPI client and external API DTOs
    - `Pokemon.Tests/` — xUnit tests
    - Solution file
- `testfixtures/`
    - `initial-load/`
        - `backend/test-data.json`
        - `frontend/test-data.json`
    - `empty-results/`
        - `backend/test-data.json`
        - `frontend/test-data.json`
    - `api-error/`
        - `backend/test-data.json`
        - `frontend/test-data.json`
- `README.md`

The intended fixture naming convention is `scenario/backend/test-data.json` and `scenario/frontend/test-data.json`.

Each scenario contains separate backend and frontend fixtures. For example, `initial-load` contains a raw PokéAPI list response for backend tests and the expected mapped API response for frontend tests.

## 4. Data contracts

Keep three concepts separate:

- **Upstream DTO:** represents the JSON returned by PokéAPI.
- **Domain model:** represents a Pokémon within the application.
- **Frontend API contract:** represents the JSON returned by the C# API and consumed by Angular.

This prevents changes in PokéAPI's response format from leaking directly into the frontend.

For the MVP, a Pokémon domain model should include:

- `id`
- `name`

The list response should include:

- `items`: the Pokémon returned by the application
- `count`: a clearly defined count value

Decide whether `count` means the total number of Pokémon available from PokéAPI or the number of items returned in the current response. For a list endpoint, using the upstream total is generally more useful, but the meaning must remain consistent across the API contract and fixtures.

The PokéAPI list endpoint provides names and URLs, not sprites, types, heights, or stats. For the MVP, display names and optionally derive Pokédex IDs from the resource URLs. Richer details can be added later if required.

The first-page request should use PokéAPI's default pagination behaviour. The MVP does not need to follow the `next` link or implement pagination in the application.

## 5. Test strategy

### Backend — xUnit

Test the mapping from raw PokéAPI JSON to the application's domain model. Use a fake or mocked HTTP handler so tests never need to call the real internet.

Initial tests should cover:

- Deserialising a valid first-page response.
- Mapping Pokémon names and IDs correctly.
- Handling a valid response with an empty results array.
- Handling malformed JSON.
- Handling upstream HTTP errors.
- Verifying the expected endpoint and pagination parameters.

Load fixture files through a shared test helper. Avoid duplicating file-reading logic across individual tests.

### Frontend — Playwright

Use the frontend fixture to intercept the application's API endpoint, rather than intercepting PokéAPI directly.

Initial tests should cover:

- Rendering Pokémon names from the API response.
- Displaying a loading state.
- Displaying an empty state.
- Displaying an API error state.
- Showing the expected number of Pokémon entries.

The fixture should reflect the actual response contract returned by the C# API.

## 6. Scenario design

Start with three scenarios.

### `initial-load`

- **Backend fixture:** a successful first-page PokéAPI response.
- **Frontend fixture:** a populated list using the application's domain contract.
- **Purpose:** verify normal mapping and list rendering.

### `empty-results`

- **Backend fixture:** a valid PokéAPI-shaped response with an empty results array.
- **Frontend fixture:** an empty list response.
- **Purpose:** verify that both layers handle an empty collection correctly.

### `api-error`

- **Backend fixture:** an optional payload if useful for the particular error case.
- **Backend test setup:** configure the fake HTTP handler to return an HTTP failure, timeout, or connection error as appropriate.
- **Frontend fixture:** the expected error-state response only if the frontend contract represents errors as JSON. Otherwise, configure the Playwright route with the relevant HTTP status.
- **Purpose:** verify that failures are handled predictably.

JSON fixtures cannot represent every test condition. HTTP status codes, timeouts, and connection failures should be configured by the test itself. A fixture describes the payload; the test configures how the dependency behaves.

As scenarios become more complex, add dedicated files or scenario metadata rather than turning `test-data.json` into a wrapper for every possible response and failure mode.

## 7. Fixture loading and maintenance

Both test suites should resolve fixtures from a known fixture root. Do not depend on relative paths that only work when tests are run from a particular working directory.

Keep fixture data small and focused on the behaviour being tested. Tests should make clear which scenario they use and why.

To reduce contract drift, keep frontend fixtures representative of the actual C# API response. Consider a contract test that serialises a domain response and checks its JSON shape against the expected frontend contract.

## 8. Implementation order

1. **Create the monorepo and fixture loading helpers.** Set up Angular, Playwright, ASP.NET Core, and xUnit. Verify that both test suites can find fixtures regardless of their working directory.
2. **Define the domain and API contracts.** Agree on the Pokémon model, list response, and meaning of `count`.
3. **Implement the backend.** Add a PokéAPI client, upstream DTOs, mapping logic, and an endpoint such as `GET /api/pokemon`.
4. **Add xUnit tests.** Verify mapping and failure behaviour using fixture-driven HTTP responses.
5. **Build the Angular page.** Add the typed API service, models, list rendering, loading state, empty state, and error state.
6. **Add Playwright scenarios.** Stub the application API with frontend fixtures and assert the visible behaviour.
7. **Configure CI.** Run both test suites on every pull request without making live external API calls during automated tests.

## 9. MVP completion criteria

The MVP is ready when:

- The C# API retrieves the first page from PokéAPI.
- The API maps upstream data into the application's own domain model.
- Angular renders the API's domain response.
- Backend tests use raw PokéAPI fixtures.
- Frontend tests use mapped frontend fixtures.
- Empty results and relevant failure states are tested.
- Automated tests do not depend on PokéAPI being available.
- Fixture paths work consistently in local development and CI.

## 10. Key design principles

- Keep upstream DTOs, domain models, and frontend contracts separate.
- Use scenario-based fixtures shared through the repository, with distinct backend and frontend data.
- Keep production code independent of the fixture directory.
- Mock external HTTP calls in xUnit tests.
- Intercept the application's API in Playwright tests.
- Make count and pagination semantics explicit.
- Start with one end-to-end vertical slice before adding richer Pokémon details, pagination, or caching.
