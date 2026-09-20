# FunBooksAndVideos

An e-commerce shop where customers buy books, videos and club memberships with a purchase order, built as a REST API for the *API Lead - Code Kata*. C# on .NET 10, ASP.NET Core Controllers, Clean Architecture, EF Core on SQL Server.

## Run it locally

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker](https://www.docker.com/) (SQL Server runs in a container).

```bash
git clone https://github.com/Pondzsi/FunBooksAndVideos.git
cd FunBooksAndVideos

docker compose up -d --wait                                            # SQL Server on localhost:1433
dotnet run --project FunBooksAndVideos.Api --launch-profile http       # the API on http://localhost:5163
```

That is all the setup. On startup the API creates the database, applies the EF migrations and seeds it: seven products (the PDF's example is products 1 to 3) and customer `4567890`. Then open **http://localhost:5163/scalar** for interactive docs, or use `FunBooksAndVideos.Api/FunBooksAndVideos.Api.http`.

- **Run the tests** with `dotnet test`. The SQL Server tests start their own container, so Docker must be running (they skip themselves if it isn't).
- **Stop** the API with Ctrl+C and the database with `docker compose down`.
- The `sa` password in `docker-compose.yml` and `appsettings.json` is a throwaway for that local container.

Try the PDF's example order, where customer `4567890` buys products 1, 2 and 3:

```bash
curl -i -X POST http://localhost:5163/api/v1/purchase-orders \
  -H 'Content-Type: application/json' \
  -d '{"customerId": 4567890, "productIds": [1, 2, 3]}'
```

The response is `201 Created` with a `Location` header and the priced order (total `48.50`). Then `GET /api/v1/customers/4567890` shows the Book Club membership, and `GET /api/v1/purchase-orders/{id}/shipping-slip` shows the slip with the book on it.

## What it does

Processing a purchase order applies the two business rules from the brief:

- **BR1**: a membership in the order is activated on the customer's account immediately.
- **BR2**: an order with a physical product gets a shipping slip.

`NOTES.md` has my assumptions and working notes.

## API

Base path `/api/v1`, JSON, camelCase, enums as names (`"Book"`).

| Method and path | Purpose | Success | Errors |
|---|---|---|---|
| `POST /purchase-orders` | Place an order: `{ "customerId", "productIds": [] }` | `201` + `Location` | `400` invalid body, `422` unknown customer or product |
| `GET /purchase-orders/{id}` | An order, with the price each item had when placed | `200` | `404` |
| `GET /purchase-orders/{id}/shipping-slip` | The slip BR2 generated | `200` | `404` (also when the order needed none) |
| `GET /customers/{id}` | Memberships and the categories they give access to | `200` | `404` |
| `GET /products`, `GET /products/{id}` | The catalog | `200` | `404` |

- **Errors** are always [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) Problem Details (`application/problem+json`): validation, missing resources, unknown routes and unhandled exceptions alike.
- **`422` versus `404`**: a missing resource in the URL is `404`. A customer or product named in a request body that doesn't exist is `422`, because the request is well formed but can't be processed. The body names the missing ID.
- **Prices are never sent.** Callers send product IDs, and prices come from the catalog. Each order line keeps the price at order time, and the total is computed.

## How the brief is covered

| Brief | Where |
|---|---|
| Object-oriented model | `FunBooksAndVideos.Domain`: `Product` (Physical, Digital, Membership), `PurchaseOrder`, `Customer`, `Membership`, `ShippingSlip` |
| Exposed as REST APIs | `FunBooksAndVideos.Api`: Controllers, DTOs, Problem Details, OpenAPI |
| Flexible Purchase Order Processor | `PurchaseOrderProcessor` runs every registered `IPurchaseOrderRule`. A new rule is one class plus one line in `AddApplication()`, and the processor never changes |
| BR1 | `ActivateMembershipRule` |
| BR2 | `GenerateShippingSlipRule` |

## Architecture

```
Api ───────────► Application ───────► Domain
 └──► Infrastructure ──┘
      (implements Application's interfaces)
```

Dependencies point inward only, and project references enforce it. Each layer is a project at the repo root:

- **Domain**: entities and rules that don't depend on anything. It never reads the clock or touches a database.
- **Application**: the `PlacePurchaseOrder` use case, the processor and rules, and the repository and unit-of-work interfaces.
- **Infrastructure**: EF Core on SQL Server (mappings, repositories, migrations, seeder).
- **Api**: Controllers and the composition root.

Patterns in use: Strategy (the rules), Aggregate root with domain events (published through MediatR after the commit), Repository and Unit of Work, Factory method, DTOs separate from domain objects, injected clock.

Placing an order: load the customer and products, take the next ID from a SQL sequence, build the `PurchaseOrder`, run the rules, stage the order and slip, commit once (the order, the new membership and the slip land together or not at all), then publish the domain events.

## Assumptions and decisions

- Books are physical; videos and memberships are digital. The PDF doesn't say, so an order of only videos and memberships needs no shipping slip.
- Memberships are catalog products carrying the categories they grant (Book Club, Video Club, Premium). **Premium** is one order line and one membership covering both categories, and access is the union of a customer's memberships. A new membership is a new catalog row, not code.
- Buying a membership the customer already holds changes nothing (no renewal or expiry in the brief).
- One item line per product purchased, so there are no quantities.
- IDs are numeric. Order IDs come from a SQL Server sequence, so an order has its ID before it is saved.
- Domain rules throw on invalid state; there is no Result type.

## Tests

`dotnet test` runs 90 tests:

- **Domain and Application**: unit tests, including the PDF's example order through both rules and the whole use case against small fakes.
- **Infrastructure**: round-trip tests against a real SQL Server started by [Testcontainers](https://testcontainers.com/), with a fresh migrated and seeded database per test.
- **API**: the whole app in memory over HTTP: the PDF example end to end, and every error path.

The SQL Server tests need Docker and skip themselves when it isn't running. The tests were also checked by deliberately breaking the code and confirming a test failed.

## Working with the database

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project FunBooksAndVideos.Infrastructure --startup-project FunBooksAndVideos.Api --output-dir Persistence/Migrations
```

A test fails if the model and the migrations disagree, so a forgotten migration is caught.

## Not included

Authentication, pagination (the catalog is tiny), idempotency keys (retrying a `POST` places a second order), endpoints to create customers or products (the seed data covers them), and any real reaction to the domain events beyond logging them.
