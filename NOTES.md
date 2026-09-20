# Assumptions
- Users and Customers are the same thing
- Users can buy Books, Videos and Memberships using a Purchase Order
- Videos are digital (the PDF says customers "watch online videos"), and memberships don't need shipping either. The PDF doesn't say which products are physical, so I assume Books are physical so that BR2 makes sense to have it now. This means an order with only videos + memberships doesn't need shipping
- Premium Membership = Book Membership + Video Membership. On the order it's one item line (membership type Premium), but on the user's account it gives access to both


# Entities based on first sight at the PDF:
    Customer
    Book
    Video
    Membership
    PurchaseOrder
    PurchaseOrderItem (the PDF has one item line per product purchased)
    ShippingSlip (needed for BR2)

# AKA
BR - Business Rule

# Things I have noticed
- Because memberships can be bought as a product but are also something the user has on their account, I will create a MembershipProduct and a Membership entity

- For the Products I will have a base Product and subtypes for MembershipProduct, PhysicalProduct, DigitalProduct
    - This separates the products based on what needs to be done with them: a Membership has to be activated instantly (BR1), a PhysicalProduct (book) has to be shipped (BR2), and a DigitalProduct (video) needs nothing extra for now
    - This also makes it easier to add new products into the system, and based on the BRs we know at the moment this slicing is easy to use while following them

- I also noticed that the Physical/Digital slicing will not cover every future case, because we might have physical videos like a DVD, which needs shipping but is a video, or an ebook, which doesn't need shipping but is a book. So the Physical/Digital split describes how a product is fulfilled, not what the product is (book or video). Memberships stay separate because they are activated, not fulfilled.

# Decisions
- For product categories (book,video) I will create an enum for now since its a kata, it would become an entity in the database when we would need more categories
- Memberships are products
- Product Subtypes are MembershipProduct, PhysicalProduct, DigitalProduct and they are describing how they are handled

# Way of thinking while building the solution
- I am using AI to help me write code faster
- I am using Clean Arhitecture to separate things out
- I am using .NET 10 with Controllers not Minimal API-s, its just personal preference
- I am building the application layer by layer and at the end I might refactor certain parts if I am not happy with them
- I am using docker for database since its easy to spin it up on a new machine
- I am running AI agents on the code afterwards to check out the developed code against requirement and my notes (this file)
- I am also running an AI agent that creates a small frontend for demonstration purposes
- I gathered all the design patterns I could find that are used in the project and afterwards let AI to spot if I missed any

# Design Patterns Used
## Behavior
- Strategy, in a loose form: IPurchaseOrderRule with ActivateMembershipRule (BR1) and GenerateShippingSlipRule (BR2), run by PurchaseOrderProcessor. A new rule is one class plus one DI line. I run every rule, so it is not Chain of Responsibility
- Parameter object: PurchaseOrderProcessingContext carries order, customer, time and slip, so every rule takes one argument. It also carries the slip out, which is a bit loose
- Application service with a command object: PlacePurchaseOrder takes a PlacePurchaseOrderCommand
- Domain events with Observer, through MediatR: PlacePurchaseOrder publishes MembershipActivated and ShippingSlipGenerated after the commit. Handlers only log for now, and it is in-process, not an outbox
- Injected clock: TimeProvider goes into PlacePurchaseOrder, and the Domain only gets a time as a parameter

## Domain
- Aggregate root: AggregateRoot holds the events for Customer and ShippingSlip. PurchaseOrder is an aggregate too but skips it, since it raises no events
- Encapsulated collections: private lists exposed read-only in the aggregates
- Static factory method: ShippingSlip.Generate hides the constructor and raises the event
- Value object: ShippingSlipItem, an immutable record
- Guard clauses: ThrowIf checks in constructors and factories

## Structure and persistence
- Dependency inversion: repository and IUnitOfWork interfaces live in Application, Infrastructure implements them, references only point inward
- Composition root: Program.cs calls AddApplication() and AddInfrastructure()
- Repository and Unit of Work: ICustomerRepository and the other repositories plus IUnitOfWork. I know both are thin because DbContext already is both, but Application never sees EF
- Data Mapper: EF Core with IEntityTypeConfiguration classes, so the Domain has no EF attributes
- Single-table inheritance (TPH): Product subtypes share one table with a Kind column

## API
- DTOs with static From() mappers: response records in Api/Contracts, so domain objects never go out over HTTP
- Global exception handler: NotFoundExceptionHandler (IExceptionHandler) returns Problem Details, a 422 for a missing customer or product. Only one handler, so no real chain

## Testing
- Object Mother: TestData with NewCustomer() and PdfExampleOrder()
- Hand-written fakes and spies, no mocking library: FakeOrders, FakeUnitOfWork, FakePublisher. A shared call log checks events go out after the commit
- Test fixture with Testcontainers: SqlServerFixture, one SQL Server container with a new database per test. ApiFactory runs the API in memory
