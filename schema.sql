PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Suppliers (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Address TEXT,
    Telephone TEXT
);

CREATE TABLE IF NOT EXISTS Customer (
    CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerType INTEGER,
    First_Name TEXT,
    Last_Name TEXT NOT NULL,
    Middle_Initial TEXT,
    Tax_Registration_No TEXT,
    Title TEXT,
    Date_Open TEXT,
    Date_Closed TEXT,
    Address1 TEXT,
    Address2 TEXT,
    Parish TEXT,
    Country TEXT DEFAULT 'Jamaica',
    Telephone_1 TEXT,
    Telephone_2 TEXT,
    Fax_no TEXT,
    Email TEXT,
    Cell TEXT,
    PrimaryContact TEXT,
    Client_Password NUMERIC,
    Last_Updated TEXT,
    Last_Updated_By TEXT,
    PinNo BLOB,
    Zipcode TEXT,
    CreditLimit REAL DEFAULT 0,
    ResellerDiscount REAL DEFAULT 0,
    ArBalance REAL DEFAULT 0,
    Message TEXT,
    IsBlocked INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Entries (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    EngineNumber TEXT NOT NULL,
    EntryType TEXT DEFAULT 'C78',
    ChassisNumber TEXT,
    EntryNumber TEXT,
    SupplierID INTEGER,
    ContainerNumber TEXT,
    C78EntryDate TEXT,
    DateEntered TEXT DEFAULT (datetime('now','localtime')),
    InvoiceNumber INTEGER,
    InvoiceType INTEGER,
    PartNumber TEXT,
    Description TEXT,
    CustomerID INTEGER,
    CustomerTypeID INTEGER,
    InvoiceAmount REAL DEFAULT 0,
    LineNumber INTEGER,
    CustomerName TEXT,
    DateSold TEXT,
    InvoiceCreationTerminal TEXT,
    SalesPersonID INTEGER,
    Discount REAL DEFAULT 0,
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(ID) ON DELETE SET NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_entries_engine ON Entries(EngineNumber);
CREATE INDEX IF NOT EXISTS idx_entries_container ON Entries(ContainerNumber);
CREATE INDEX IF NOT EXISTS idx_entries_entrynum ON Entries(EntryNumber);
