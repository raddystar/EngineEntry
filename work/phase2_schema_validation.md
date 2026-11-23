# Phase 2: Database Schema Validation and Recovery

## Extracted Database Schema

Based on the analysis of `SmartpartsModel.csdl` and the SQL stored procedures, the database schema appears to be structured as follows:

### Core Tables

#### **Entries**
- **ID**: Primary key (int)
- **EntryType**: Character field (char) 
- **ChassisNumber**: Character field (char)
- **EngineNumber**: Character field (char)
- **SupplierID**: Character field (char)
- **ContainerNumber**: Character field (char)
- **C78EntryDate**: Date field (date)
- **DateEntered**: Date field (date)

#### **Suppliers**
- **ID**: Primary key (int)
- **SupplierName**: Character field (char)
- **Address1**: Character field (char)
- **Address2**: Character field (char)
- **PostalCode**: Character field (char)
- **City**: Character field (char)
- **Country**: Character field (char)
- **Telephone**: Character field (char)
- **Fax**: Character field (char)

#### **Engines**
- **ID**: Primary key (int)
- **EngineNumber**: Character field (char)
- **EntryType**: Character field (char)
- **ChassisNumber**: Character field (char)
- **EntryNumber**: Character field (char)
- **SupplierID**: Character field (char)
- **ContainerNumber**: Character field (char)
- **C78EntryDate**: Date field (date)
- **DateEntered**: Date field (date)

#### **Invoices**
- **ID**: Primary key (int)
- **InvoiceNumber**: Character field (char)
- **InvoiceType**: Character field (char)
- **CustomerID**: Character field (char)
- **CustomerName**: Character field (char)
- **Address1**: Character field (char)
- **Address2**: Character field (char)
- **Parish**: Character field (char)
- **Country**: Character field (char)
- **SalesPersonID**: Character field (char)
- **InvoiceAmount**: Decimal field (decimal)
- **InvoiceComments**: NText field (nvarchar)
- **DateSold**: Date field (date)
- **InvoiceCreationTerminal**: Character field (char)
- **CreatedBy**: Character field (char)
- **CreatedDate**: Date field (date)

#### **InvoiceLineItems**
- **ID**: Primary key (int)
- **InvoiceID**: Foreign key (int, references Invoices)
- **LineNumber**: Integer field (int)
- **ItemNo**: Character field (char)
- **Description**: NText field (nvarchar)
- **Quantity**: Integer field (int)
- **Price**: Decimal field (decimal)
- **Discount**: Decimal field (decimal)
- **Amount**: Decimal field (decimal)
- **Tax**: Decimal field (decimal)
- **LineRef**: Character field (char)
- **LineRefType**: Character field (char)
- **Location**: Character field (char)
- **LocationID**: Integer field (int)
- **LocationName**: Character field (char)
- **LocationType**: Character field (char)
- **PriceIndex**: Character field (char)
- **JobNo**: Character field (char)
- **AlternateDescription**: NText field (nvarchar)
- **Picked**: Character field (char)
- **Commited**: Character field (char)
- **Returned**: Character field (char)
- **Reason**: Character field (char)
- **ParentID**: Integer field (int)

#### **Locations**
- **ID**: Primary key (int)
- **LocationName**: Character field (char)
- **LocationType**: Character field (char)
- **LocationTree**: Character field (char)
- **ServerName**: Character field (char)
- **DateLastUpdated**: Date field (date)
- **PostalCode**: Character field (char)
- **City**: Character field (char)
- **Telephone**: Character field (char)
- **Fax**: Character field (char)

#### **Stocks**
- **ID**: Primary key (int)
- **ItemNo**: Character field (char)
- **Description**: NText field (nvarchar)
- **Price**: Decimal field (decimal)
- **Cost**: Decimal field (decimal)
- **TaxCode**: Character field (char)
- **AlternateItemNo**: Character field (char)
- **SubstituteItemNo**: Character field (char)
- **LastPurchasePrice**: Decimal field (decimal)
- **QtyOnOrder**: Integer field (int)
- **YtdSalesDollar**: Decimal field (decimal)
- **YtdPurDollar**: Decimal field (decimal)
- **DateLastPurchased**: Date field (date)
- **DateLastSale**: Date field (date)
- **DeliveryDate**: Date field (date)
- **MaximumDiscount**: Decimal field (decimal)
- **AutoDiscount**: Decimal field (decimal)
- **AutoDiscountExpires**: Date field (date)
- **AutoDiscountEffective**: Date field (date)
- **PriceB**: Decimal field (decimal)
- **PriceC**: Decimal field (decimal)
- **DescChangesAtSale**: Character field (char)
- **PriceChagesAtSale**: Character field (char)
- **ExRate**: Decimal field (decimal)
- **Currency**: Character field (char)
- **NonStockItem**: Character field (char)
- **StockLeadTime**: Integer field (int)

#### **Users**
- **ID**: Primary key (int)
- **UserName**: Character field (char)
- **Password**: Character field (char)
- **LoginStatus**: Character field (char)
- **LoginDate**: Date field (date)
- **UserSessionNo**: Character field (char)
- **PassWord**: Character field (char)
- **FingerPrint**: Character field (char)
- **Last_Updated**: Date field (date)
- **Last_Updated_By**: Character field (char)
- **PinNo**: Character field (char)
- **Zipcode**: Character field (char)
- **CreditLimit**: Decimal field (decimal)
- **ResellerDiscount**: Decimal field (decimal)
- **ArBalance**: Decimal field (decimal)
- **Message**: Character field (char)
- **IsBlocked**: Character field (char)
- **Delayed**: Character field (char)
- **Terms**: Character field (char)
- **LoyaltyCardNo**: Character field (char)

#### **TransactionTypes**
- **ID**: Primary key (int)
- **TrnNo**: Character field (char)
- **TrnType**: Character field (char)
- **Transtype**: Character field (char)
- **LocationID**: Integer field (int)
- **LocationName**: Character field (char)
- **DateLastUpdated**: Date field (date)
- **CashierSystem**: Character field (char)
- **TerminalId**: Character field (char)
- **IsArTransaction**: Character field (char)
- **IsArPayment**: Character field (char)
- **IsArInvoice**: Character field (char)
- **IsReversalType**: Character field (char)
- **IsaPayment**: Character field (char)
- **IsaCashierPayment**: Character field (char)
- **UID**: Character field (char)
- **UserID**: Character field (char)
- **SalesAgentName**: Character field (char)
- **Telephone_1**: Character field (char)
- **Telephone_2**: Character field (char)
- **Fax_no**: Character field (char)
- **Email**: Character field (char)
- **Cell**: Character field (char)
- **PrimaryContact**: Character field (char)

## Relationships

1. **Entries → Suppliers**: The original application seems to use a foreign key relationship `FK_Entries_Suppliers` (likely `SupplierID` in `Entries` references `ID` in `Suppliers`).
2. **Engines → Entries**: Engines are linked to entries via `EntryNumber`/`ChassisNumber`/`SupplierID`.
3. **Invoices → Customers**: Likely via `CustomerID` referencing `ID` in `Customers` (not explicitly listed in CSDL but inferred from `CustomerID` in `Invoices`).
4. **InvoiceLineItems → Invoices**: Via `InvoiceID` (foreign key).
5. **InvoiceLineItems → Locations**: Via `LocationID` (foreign key).
6. **Stocks → Locations**: Via `FK_Stocks_Locations` (not explicitly listed in CSDL but inferred from business logic).

## Additional Findings

- **SQL Stored Procedures**: The #US heap contains stored procedures like `spCreateInvoiceTransactionV2`, which creates invoices and updates stock.
- **UI References**: The application uses XAML forms like `InvoiceNew.xaml`, `CustomerSearch.xaml`, and `EngineAdd.xaml`.
- **Connection Strings**: Both `EngineEntities` and `SmartpartsEntities` connect to SQL Server instances (`Engines` and `smartparts`).

## Normalization

The schema appears to be **partially normalized** with some tables (like `Engines` and `Entries`) sharing common fields (e.g., `SupplierID`, `EntryNumber`).

### Proposed Normalization
- **Flatten Entries/Engines**: If `Entries` and `Engines` are linked via `SupplierID`/`EntryNumber`, consider merging them into a single `Entries` table with a `Type` field to distinguish between engines and regular entries.
- **Customers**: If `CustomerID` is a foreign key in `Invoices`, ensure `Customers` table exists.
- **Locations**: Ensure `Locations` table is properly linked to `Stocks` and `InvoiceLineItems`.

## Next Steps

1. **Confirm Schema**: Please review the schema above and confirm if any extra fields or features were missed in the .exe.
2. **Proceed to Phase 3**: Once confirmed, I will propose a modern rebuild plan using Python (CustomTkinter/PySide6) or C# (.NET 8 WPF/WinForms) with SQLite.

--- 

## SQL Schema for SQLite

Below is a proposed SQLite schema based on the extracted schema:

```sql
-- Entries table
CREATE TABLE Entries (
    ID INTEGER PRIMARY KEY,
    EntryType CHAR(1),
    ChassisNumber CHAR(50),
    EngineNumber CHAR(50),
    SupplierID CHAR(50),
    ContainerNumber CHAR(50),
    C78EntryDate DATE,
    DateEntered DATE
);

-- Suppliers table
CREATE TABLE Suppliers (
    ID INTEGER PRIMARY KEY,
    SupplierName CHAR(50),
    Address1 CHAR(50),
    Address2 CHAR(50),
    PostalCode CHAR(10),
    City CHAR(50),
    Country CHAR(50),
    Telephone CHAR(50),
    Fax CHAR(50)
);

-- Engines table
CREATE TABLE Engines (
    ID INTEGER PRIMARY KEY,
    EngineNumber CHAR(50),
    EntryType CHAR(1),
    ChassisNumber CHAR(50),
    SupplierID CHAR(50),
    ContainerNumber CHAR(50),
    C78EntryDate DATE,
    DateEntered DATE
);

-- Invoices table
CREATE TABLE Invoices (
    ID INTEGER PRIMARY KEY,
    InvoiceNumber CHAR(50),
    InvoiceType CHAR(10),
    CustomerID CHAR(50),
    CustomerName CHAR(50),
    Address1 CHAR(50),
    Address2 CHAR(50),
    Parish CHAR(50),
    Country CHAR(50),
    SalesPersonID CHAR(50),
    InvoiceAmount DECIMAL(18,2),
    InvoiceComments TEXT,
    DateSold DATE,
    InvoiceCreationTerminal CHAR(50),
    CreatedBy CHAR(50),
    CreatedDate DATE
);

-- InvoiceLineItems table
CREATE TABLE InvoiceLineItems (
    ID INTEGER PRIMARY KEY,
    InvoiceID INTEGER,
    LineNumber INTEGER,
    ItemNo CHAR(50),
    Description TEXT,
    Quantity INTEGER,
    Price DECIMAL(18,2),
    Discount DECIMAL(18,2),
    Amount DECIMAL(18,2),
    Tax DECIMAL(18,2),
    LineRef CHAR(50),
    LineRefType CHAR(50),
    Location CHAR(50),
    LocationID INTEGER,
    LocationName CHAR(50),
    LocationType CHAR(50),
    PriceIndex CHAR(50),
    JobNo CHAR(50),
    AlternateDescription TEXT,
    Picked CHAR(1),
    Commited CHAR(1),
    Returned CHAR(1),
    Reason CHAR(50),
    ParentID INTEGER,
    FOREIGN KEY (InvoiceID) REFERENCES Invoices(ID),
    FOREIGN KEY (LocationID) REFERENCES Locations(ID)
);

-- Locations table
CREATE TABLE Locations (
    ID INTEGER PRIMARY KEY,
    LocationName CHAR(50),
    LocationType CHAR(50),
    LocationTree CHAR(50),
    ServerName CHAR(50),
    DateLastUpdated DATE,
    PostalCode CHAR(10),
    City CHAR(50),
    Telephone CHAR(50),
    Fax CHAR(50)
);

-- Stocks table
CREATE TABLE Stocks (
    ID INTEGER PRIMARY KEY,
    ItemNo CHAR(50),
    Description TEXT,
    Price DECIMAL(18,2),
    Cost DECIMAL(18,2),
    TaxCode CHAR(50),
    AlternateItemNo CHAR(50),
    SubstituteItemNo CHAR(50),
    LastPurchasePrice DECIMAL(18,2),
    QtyOnOrder INTEGER,
    YtdSalesDollar DECIMAL(18,2),
    YtdPurDollar DECIMAL(18,2),
    DateLastPurchased DATE,
    DateLastSale DATE,
    DeliveryDate DATE,
    MaximumDiscount DECIMAL(18,2),
    AutoDiscount DECIMAL(18,2),
    AutoDiscountExpires DATE,
    AutoDiscountEffective DATE,
    PriceB DECIMAL(18,2),
    PriceC DECIMAL(18,2),
    DescChangesAtSale CHAR(1),
    PriceChagesAtSale CHAR(1),
    ExRate DECIMAL(18,2),
    Currency CHAR(50),
    NonStockItem CHAR(1),
    StockLeadTime INTEGER
);

-- Users table
CREATE TABLE Users (
    ID INTEGER PRIMARY KEY,
    UserName CHAR(50),
    Password CHAR(50),
    LoginStatus CHAR(1),
    LoginDate DATE,
    UserSessionNo CHAR(50),
    PassWord CHAR(50),
    FingerPrint CHAR(50),
    Last_Updated DATE,
    Last_Updated_By CHAR(50),
    PinNo CHAR(50),
    Zipcode CHAR(50),
    CreditLimit DECIMAL(18,2),
    ResellerDiscount DECIMAL(18,2),
    ArBalance DECIMAL(18,2),
    Message CHAR(50),
    IsBlocked CHAR(1),
    Delayed CHAR(1),
    Terms CHAR(50),
    LoyaltyCardNo CHAR(50)
);

-- TransactionTypes table
CREATE TABLE TransactionTypes (
    ID INTEGER PRIMARY KEY,
    TrnNo CHAR(50),
    TrnType CHAR(50),
    Transtype CHAR(50),
    LocationID INTEGER,
    LocationName CHAR(50),
    DateLastUpdated DATE,
    CashierSystem CHAR(50),
    TerminalId CHAR(50),
    IsArTransaction CHAR(1),
    IsArPayment CHAR(1),
    IsArInvoice CHAR(1),
    IsReversalType CHAR(1),
    IsaPayment CHAR(1),
    IsaCashierPayment CHAR(1),
    UID CHAR(50),
    UserID CHAR(50),
    SalesAgentName CHAR(50),
    Telephone_1 CHAR(50),
    Telephone_2 CHAR(50),
    Fax_no CHAR(50),
    Email CHAR(50),
    Cell CHAR(50),
    PrimaryContact CHAR(50),
    FOREIGN KEY (LocationID) REFERENCES Locations(ID)
);

--- 

## SQL Initialization Script

```sql
-- Create database tables
CREATE TABLE IF NOT EXISTS Entries (
    ID INTEGER PRIMARY KEY,
    EntryType CHAR(1),
    ChassisNumber CHAR(50),
    EngineNumber CHAR(50),
    SupplierID CHAR(50),
    ContainerNumber CHAR(50),
    C78EntryDate DATE,
    DateEntered DATE
);

-- Insert sample data (if needed)
INSERT INTO Entries (EntryType, ChassisNumber, EngineNumber, SupplierID, ContainerNumber, C78EntryDate, DateEntered) VALUES 
('A', 'ABC123', 'ENG001', 'SUP001', 'CON001', '2023-01-01', '2023-01-15');

-- Repeat for other tables as needed
```