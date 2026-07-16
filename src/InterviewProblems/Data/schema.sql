-- Developer 2 interview — SQLite schema
-- This database is rebuilt fresh in memory before every SQL test runs.

CREATE TABLE Customers (
    Id        INTEGER PRIMARY KEY,
    Name      TEXT    NOT NULL,
    Email     TEXT    NOT NULL,
    City      TEXT    NOT NULL,
    IsActive  INTEGER NOT NULL,   -- 1 = active, 0 = inactive
    CreatedAt TEXT    NOT NULL    -- ISO-8601 date
);

CREATE TABLE Products (
    Id       INTEGER PRIMARY KEY,
    Name     TEXT    NOT NULL,
    Category TEXT    NOT NULL,
    Price    REAL    NOT NULL
);

CREATE TABLE Orders (
    Id         INTEGER PRIMARY KEY,
    CustomerId INTEGER NOT NULL,
    OrderDate  TEXT    NOT NULL,   -- ISO-8601 date
    Status     TEXT    NOT NULL,   -- 'Completed' | 'Cancelled'
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE OrderItems (
    Id        INTEGER PRIMARY KEY,
    OrderId   INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity  INTEGER NOT NULL,
    UnitPrice REAL    NOT NULL,
    FOREIGN KEY (OrderId)   REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
