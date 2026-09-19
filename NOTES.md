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