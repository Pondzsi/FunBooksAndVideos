# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Status

Greenfield code kata. The solution is scaffolded. The Domain has the product types, `PurchaseOrder`, `Customer` with its `Membership`s, and `ShippingSlip`, and the Application layer has the processor with BR1 and BR2 and the `PlacePurchaseOrder` use case. Infrastructure persists it all in SQL Server through EF Core, with an initial migration and a seeder. The API exposes it with Controllers under `/api/v1`. Everything is tested, including against a real SQL Server and over HTTP. The kata is feature-complete.

The spec is `API Lead - Code Kata.pdf`. `NOTES.md` is the author's first-person running log of assumptions and design decisions; read it before making modeling decisions and treat what it records as settled unless the user says otherwise. `README.md` is the reviewer-facing guide (how to run, the API, architecture, assumptions); keep it in step with the code.

## Stack and structure

C# on .NET, exposed as a Web API built with **Controllers** (`[ApiController]` classes, not minimal APIs). The code follows Clean Code principles and is laid out as Clean Architecture, where dependencies point inward only:

- **Domain** (innermost): entities and business types such as the `Product` subtypes, `PurchaseOrder`, `Membership`, `ShippingSlip`. References nothing else except `MediatR.Contracts` (interfaces only, for `IDomainEvent`).
- **Application**: the `PlacePurchaseOrder` use case, the Purchase Order Processor and the business rules it runs (BR1, BR2), plus interfaces for anything it needs from outside, such as persistence.
- **Infrastructure**: implementations of those interfaces: EF Core on SQL Server (`AppDbContext`, mappings, repositories, unit of work, migrations, seeder).
- **API** (outermost): Controllers, request/response DTOs, and the DI composition root.

Each layer is its own project in a `FunBooksAndVideos.<Layer>` folder at the repo root (no `src/` or `tests/` folders), and project references enforce the rule (Application → Domain, Infrastructure → Application, Api → Application + Infrastructure), so an outward reference won't compile. Don't add one. Shared settings (`net10.0`, nullable, implicit usings) live in `Directory.Build.props`, so leave them out of `.csproj` files. Application and Infrastructure each expose a `DependencyInjection.cs` extension (`AddApplication()`, `AddInfrastructure()`) that `Program.cs` calls; register new services there, not in the Api.

## Commands

Run from the repo root; the solution is `FunBooksAndVideos.slnx`.

- Build: `dotnet build`
- Test: `dotnet test` (xUnit). Single test: `dotnet test --filter "FullyQualifiedName~<TestOrClassName>"`.
- Start the local database: `docker compose up -d --wait` (SQL Server 2022 on `localhost:1433`; stop it with `docker compose down`).
- Run the API: `dotnet run --project FunBooksAndVideos.Api --launch-profile http` serves `http://localhost:5163`, with the OpenAPI document at `/openapi/v1.json` and interactive docs at `/scalar` (Development only). `FunBooksAndVideos.Api/FunBooksAndVideos.Api.http` has requests for the PDF example. It needs the database running, and on startup it applies pending migrations and seeds.
- Restore the EF tool once: `dotnet tool restore`. Add a migration: `dotnet ef migrations add <Name> --project FunBooksAndVideos.Infrastructure --startup-project FunBooksAndVideos.Api --output-dir Persistence/Migrations`.

All tests live in the single `FunBooksAndVideos.Tests` project, in folders that mirror the layers (`Domain/`, `Application/`, `Infrastructure/`, `Api/`). `TestData` holds the PDF example, and the use-case tests use small in-test fakes for the repository interfaces. The `Infrastructure/` and `Api/` tests run against a real SQL Server started by Testcontainers (one container, a fresh migrated and seeded database per test, through the real DI setup). They need Docker and skip themselves when it isn't running; `MigrationsTests` needs no Docker. The `Api/` tests start the whole app in memory with `ApiFactory` (`Program` is public for that) on a fresh database, and read the raw JSON where the wire format matters, because the test client's own enum converter would accept numbers.

## The task (from the PDF)

FunBooksAndVideos is an e-commerce shop where customers view books, watch online videos, and can hold memberships for the book club, the video club, or both (premium). Deliverables:

1. An object-oriented model of the system, exposed as REST API(s) "utilizing best practices, standards, and guidelines".
2. A *flexible* Purchase Order Processor built with good design principles and patterns.
3. The business rules, applied when a purchase order is processed:
   - **BR1**: if the order contains a membership, activate it on the customer account immediately.
   - **BR2**: if the order contains a physical product, generate a shipping slip.

The PDF lists only "some of" the business rules, so treat the rule set as open-ended: new rules should be addable without editing the processor.

A purchase order has a PO ID, customer ID, total price, and one item line per product purchased (a product or a membership type). The PDF's example works as a test fixture: PO `3344656`, customer `4567890`, total `48.50`, lines = Video "Comprehensive First Aid Training", Book "The Girl on the train", Book Club Membership.

## Domain model decisions

Summary of `NOTES.md` plus later decisions (`NOTES.md` wins if they disagree):

- Customer and User are the same thing.
- `Product` is a base type split by *how it is handled*, not what it is: `MembershipProduct` (activated, BR1), `PhysicalProduct` (shipped, BR2), `DigitalProduct` (nothing extra). Book/video is a separate `ProductCategory` enum, so a DVD (physical video) or ebook (digital book) fits without a new type.
- A membership is two things: a purchasable `MembershipProduct` and a `Membership` held on the customer account.
- Membership definitions are catalog data: a `MembershipProduct` carries a price and the set of categories it grants (seed: Book Club {Book}, Video Club {Video}, Premium {Book, Video}). A new membership is a new row, not code. New *categories* still need code until categories become data.
- Premium is one order line and one account `Membership` covering both categories. Access is the union of the customer's active memberships, so there is no auto-upgrade logic.
- `Customer.ActivateMembership` copies the granted categories from the `MembershipProduct` (so later catalog edits don't change what the customer got), takes the activation time as a parameter (the Domain never reads the clock), and does nothing if the customer already holds that product.
- Aggregates (`Customer`, `ShippingSlip`) derive from `AggregateRoot` and record domain events (`MembershipActivated`, `ShippingSlipGenerated`) rather than calling anything. `PlacePurchaseOrder` publishes them through MediatR's `IPublisher` after saving, and the handlers are thin loggers. MediatR is pinned to 12.5.0, the last Apache-2.0 release (13 and later ship a custom license), so check the license before upgrading it; `MediatR.Contracts` 2.0.1 is Apache-2.0.
- `ShippingSlip` is identified by its order's ID (one slip per order), lists the order's physical products as `ShippingSlipItem(ProductId, ProductName)` lines, and is created with `ShippingSlip.Generate(order, at)`, which needs at least one physical product.
- Prices come only from the catalog: callers send product IDs, never prices. Each `PurchaseOrderItem` keeps the price at order time, and the total is computed. There are no quantities (the PDF has one item line per product purchased).
- IDs are numeric (`long`). The server generates the ID of a purchase order, customer or new product before the object is constructed, from a SQL Server sequence (`PurchaseOrderIds`, `CustomerIds` from 1, `ProductIds` from 100 because the seed uses 1 to 7) through `NextIdAsync` on the repository, so an aggregate always has an identity. When a request refers to a customer, the ID is supplied by the caller and must exist.
- Domain invariants are guard clauses that throw (`Argument*Exception`); there is no Result type.
- Assumed, since the PDF doesn't say: books are physical; videos and memberships are digital. An order of only videos and memberships therefore needs no shipping slip.

## Order processing

`PurchaseOrderProcessor` runs every registered `IPurchaseOrderRule` (BR1 `ActivateMembershipRule`, BR2 `GenerateShippingSlipRule`) against a `PurchaseOrderProcessingContext`: the order, the customer who placed it, the processing time, and any `ShippingSlip` a rule produced. Rules are synchronous and only change domain objects; loading, saving and publishing belong to the use case. Adding a rule is a new `IPurchaseOrderRule` class plus one registration line in `AddApplication()`; rules run in registration order and the processor never changes.

`PlacePurchaseOrder` is the entry point controllers call. It loads the customer and each product through repository interfaces (throwing `CustomerNotFoundException` or `ProductNotFoundException`, both `NotFoundException`, which the API maps to a 422), allocates the PO ID with `IPurchaseOrderRepository.NextIdAsync`, builds the `PurchaseOrder`, runs the processor with the time from `TimeProvider`, stages the order and shipping slip, commits once through `IUnitOfWork` (the customer's new memberships are saved by change tracking, so the order, memberships and slip land together or not at all), and only then publishes the aggregates' domain events.

## API

Routes are `api/v1/<plural-noun>`, and controllers derive from `ApiControllerBase`. Conventions:

- Every error is RFC 9457 Problem Details: `AddProblemDetails`, `UseExceptionHandler` and `UseStatusCodePages` cover unhandled exceptions and bare status codes, and `[ApiController]` covers validation.
- A missing resource in the URL is a 404: the single-item read use cases (`GetPurchaseOrder`, `GetShippingSlip`, `GetCustomer`, `GetProduct`) return null (the list ones return an empty list) and the controller calls `NotFoundProblem`. A customer or product named in the `POST` body that doesn't exist makes `PlacePurchaseOrder` throw a `NotFoundException`, which `NotFoundExceptionHandler` turns into a 422 naming the ID.
- Request and response records live in `Contracts/`, separate from domain objects, and responses are built with a static `From(...)`. Validation attributes on a request record go on the constructor parameters (MVC throws if they are on the properties).
- Enums travel as names. Two lines in `Program.cs` do it and both are needed: `AddJsonOptions` for responses, `ConfigureHttpJsonOptions` for the OpenAPI document. A number where a name belongs is rejected (`allowIntegerValues: false`).
- `POST /purchase-orders` returns `PlacedPurchaseOrderResponse`: the order, the memberships it activated and whether a slip was generated. `PlacePurchaseOrderResult.ActivatedMemberships` is built from the `MembershipActivated` events raised while processing, so a membership the customer already held is not listed. `GET` returns the plainer `PurchaseOrderResponse`.
- `POST /products` and `POST /customers` go through the `CreateProduct` and `CreateCustomer` use cases. `CreateProductRequest` validates in the API (one category for a physical or digital product, one or more for a membership, price above zero), because a failing domain guard would otherwise be a 500. `ProductKind` lives in Application.
- The `///` summaries on controller actions reach OpenAPI and Scalar because `GenerateDocumentationFile` is on in the Api csproj.
- `AddInfrastructure` reads the connection string when the `DbContext` is first created, so a test host can override it after registration.

## Persistence

EF Core on SQL Server; the mappings live in `Infrastructure/Persistence/Configurations`, so Domain stays attribute-free. Conventions worth knowing:

- The Domain types carry private parameterless constructors marked "For EF Core", and their item lists sit in `List<>` fields that the mappings point at.
- EF only maps a read-only property when it is configured (or is a key or foreign key), so a new domain property needs an explicit `Property(...)` line. A miss makes `Migrate` refuse to run and fails `MigrationsTests`.
- Product types share one table with a `Kind` discriminator. Enums and category sets are stored by name (`Book,Video`), not by number.
- `Membership` and the order and slip lines are owned collections. A customer can hold a product's membership once (unique index), and each order line keeps the price at order time.
- The API applies migrations and then runs `Seeder` at startup (`InitializeDatabaseAsync`). The seeder fills an empty database only: products 1 to 7 (1 to 3 are the PDF's example, 4 and 5 the other memberships) and customer `4567890`.
- If SQL Server can't be reached, `InitializeDatabaseAsync` throws `DatabaseInitializationException`, and `Program.cs` logs its one readable line and exits with code 1. Retries (`Database:MaxRetryCount`, default 6) and the command timeout (`Database:CommandTimeoutSeconds`, default 30) are configuration. Keep the retries tolerant: a short retry policy made API tests fail when an emulated SQL Server was slow, and the tests raise the timeout to 120 seconds.
- The connection string is `ConnectionStrings:Default` in `appsettings.json`, pointing at the compose container. Its `sa` password is a throwaway for that local container.
- Generate the migration with the local `dotnet ef` tool, and change an unreleased migration by removing and re-adding it rather than stacking fix-ups.
