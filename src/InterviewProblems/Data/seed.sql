-- Developer 2 interview — sample data
-- Expected results for each problem are documented in SqlProblems.cs.

INSERT INTO Customers (Id, Name, Email, City, IsActive, CreatedAt) VALUES
    (1, 'Alice', 'alice@example.com', 'Seattle',  1, '2024-01-05'),
    (2, 'Bob',   'bob@example.com',   'Portland', 1, '2024-02-10'),
    (3, 'Carol', 'carol@example.com', 'Seattle',  0, '2024-03-15'),
    (4, 'Dave',  'dave@example.com',  'Denver',   1, '2024-04-20'),
    (5, 'Eve',   'eve@example.com',   'Portland', 1, '2024-05-25');

INSERT INTO Products (Id, Name, Category, Price) VALUES
    (1, 'Widget',  'Hardware', 10.0),
    (2, 'Gadget',  'Hardware', 25.0),
    (3, 'Gizmo',   'Hardware', 5.0),
    (4, 'eBook',   'Digital',  15.0),
    (5, 'License', 'Digital',  100.0);

INSERT INTO Orders (Id, CustomerId, OrderDate, Status) VALUES
    (1, 1, '2024-06-01', 'Completed'),  -- Alice
    (2, 1, '2024-06-15', 'Completed'),  -- Alice
    (3, 2, '2024-07-10', 'Completed'),  -- Bob
    (4, 3, '2024-07-01', 'Cancelled'),  -- Carol (cancelled)
    (5, 4, '2024-07-05', 'Completed');  -- Dave
-- Eve (customer 5) has no orders.

INSERT INTO OrderItems (Id, OrderId, ProductId, Quantity, UnitPrice) VALUES
    (1, 1, 1, 2, 10.0),   -- Order 1: Widget x2  = 20
    (2, 1, 2, 1, 25.0),   -- Order 1: Gadget x1  = 25   (order total 45)
    (3, 2, 3, 5, 5.0),    -- Order 2: Gizmo x5   = 25
    (4, 3, 4, 4, 15.0),   -- Order 3: eBook x4   = 60
    (5, 3, 5, 1, 100.0),  -- Order 3: License x1 = 100  (order total 160)
    (6, 4, 1, 1, 10.0),   -- Order 4: Widget x1  = 10   (cancelled)
    (7, 5, 2, 2, 25.0),   -- Order 5: Gadget x2  = 50
    (8, 5, 1, 4, 10.0);   -- Order 5: Widget x4  = 40   (order total 90)
