# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Status

Greenfield code kata. There is no source code, solution, or test suite yet, so there are no build/lint/test commands to record. Once the solution is scaffolded, add here its path, the test framework chosen, and the commands to build, run the API, and run a single test.

The spec is `API Lead - Code Kata.pdf`. `NOTES.md` is the author's first-person running log of assumptions and design decisions; read it before making modeling decisions and treat what it records as settled unless the user says otherwise. `README.md` is empty.

## Stack and structure

C# on .NET, exposed as a Web API built with **Controllers** (`[ApiController]` classes, not minimal APIs). The code follows Clean Code principles and is laid out as Clean Architecture, where dependencies point inward only:

- **Domain** (innermost): entities and business types such as the `Product` subtypes, `PurchaseOrder`, `Membership`, `ShippingSlip`. References nothing else.
- **Application**: the Purchase Order Processor and the business rules it runs (BR1, BR2), plus interfaces for anything it needs from outside, such as persistence.
- **Infrastructure**: implementations of those interfaces.
- **API** (outermost): Controllers, request/response DTOs, and the DI composition root.

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

Summary of `NOTES.md` (which wins if they disagree):

- Customer and User are the same thing.
- `Product` is a base type split by *how it is handled*, not what it is: `MembershipProduct` (activated, BR1), `PhysicalProduct` (shipped, BR2), `DigitalProduct` (nothing extra). Book/video is a separate category enum, so a DVD (physical video) or ebook (digital book) fits without a new type.
- A membership is two things: a purchasable `MembershipProduct` and a `Membership` held on the customer account.
- Premium = Book + Video membership: one item line on the order, but it grants both memberships on the account.
- Assumed, since the PDF doesn't say: books are physical; videos and memberships are digital. An order of only videos and memberships therefore needs no shipping slip.
- Other entities: `Customer`, `PurchaseOrder`, `PurchaseOrderItem`, `ShippingSlip`.
