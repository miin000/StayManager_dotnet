using staymanager_pj.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace staymanager_pj.Data
{
    public class DbInitializer : IDatabaseInitializer<AppDbContext>
    {
        public void InitializeDatabase(AppDbContext context)
        {
            CreateTables(context);
            Seed(context);
        }

        private static void CreateTables(AppDbContext context)
        {
            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INT NOT NULL AUTO_INCREMENT,
                    FullName VARCHAR(150) NOT NULL,
                    PhoneNumber VARCHAR(30) NULL,
                    Email VARCHAR(150) NULL,
                    IdentityNumber VARCHAR(50) NULL,
                    Address VARCHAR(300) NULL,
                    Username VARCHAR(80) NULL,
                    PasswordHash VARCHAR(200) NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS Employees (
                    Id INT NOT NULL AUTO_INCREMENT,
                    FullName VARCHAR(150) NOT NULL,
                    PhoneNumber VARCHAR(30) NULL,
                    Email VARCHAR(150) NULL,
                    IdentityNumber VARCHAR(50) NULL,
                    Position VARCHAR(80) NULL,
                    Salary DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Username VARCHAR(80) NULL,
                    PasswordHash VARCHAR(200) NULL,
                    Role INT NOT NULL,
                    IsActive TINYINT(1) NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS Rooms (
                    Id INT NOT NULL AUTO_INCREMENT,
                    RoomNumber VARCHAR(30) NOT NULL,
                    RoomType VARCHAR(80) NOT NULL,
                    PricePerNight DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Capacity INT NOT NULL DEFAULT 1,
                    Status INT NOT NULL,
                    Description VARCHAR(500) NULL,
                    ImagePath VARCHAR(300) NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS Reservations (
                    Id INT NOT NULL AUTO_INCREMENT,
                    CustomerId INT NOT NULL,
                    RoomId INT NOT NULL,
                    CheckInDate DATETIME NOT NULL,
                    CheckOutDate DATETIME NOT NULL,
                    NumberOfGuests INT NOT NULL,
                    Status INT NOT NULL,
                    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Note VARCHAR(500) NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id),
                    INDEX IX_Reservations_CustomerId (CustomerId),
                    INDEX IX_Reservations_RoomId (RoomId),
                    CONSTRAINT FK_Reservations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                    CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS Invoices (
                    Id INT NOT NULL AUTO_INCREMENT,
                    InvoiceCode VARCHAR(30) NOT NULL,
                    ReservationId INT NOT NULL,
                    CustomerId INT NOT NULL,
                    RoomId INT NOT NULL,
                    RoomCharge DECIMAL(18,2) NOT NULL DEFAULT 0,
                    ServiceCharge DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
                    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Status INT NOT NULL,
                    PaymentMethod INT NOT NULL,
                    CreatedAt DATETIME NOT NULL,
                    PaidAt DATETIME NULL,
                    CreatedByEmployeeId INT NULL,
                    ConfirmedByEmployeeId INT NULL,
                    PRIMARY KEY (Id),
                    INDEX IX_Invoices_ReservationId (ReservationId),
                    INDEX IX_Invoices_CustomerId (CustomerId),
                    INDEX IX_Invoices_RoomId (RoomId),
                    CONSTRAINT FK_Invoices_Reservations FOREIGN KEY (ReservationId) REFERENCES Reservations(Id),
                    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                    CONSTRAINT FK_Invoices_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS InvoiceItems (
                    Id INT NOT NULL AUTO_INCREMENT,
                    InvoiceId INT NOT NULL,
                    ItemName VARCHAR(150) NOT NULL,
                    Quantity INT NOT NULL,
                    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Note VARCHAR(500) NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id),
                    INDEX IX_InvoiceItems_InvoiceId (InvoiceId),
                    CONSTRAINT FK_InvoiceItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS InventoryItems (
                    Id INT NOT NULL AUTO_INCREMENT,
                    ItemCode VARCHAR(30) NOT NULL,
                    ItemName VARCHAR(150) NOT NULL,
                    Category VARCHAR(80) NULL,
                    Quantity INT NOT NULL DEFAULT 0,
                    MinimumQuantity INT NOT NULL DEFAULT 0,
                    Unit VARCHAR(30) NULL,
                    ImportPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    SellingPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Status INT NOT NULL,
                    Note VARCHAR(500) NULL,
                    CreatedAt DATETIME NOT NULL,
                    PRIMARY KEY (Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");

            context.Database.ExecuteSqlCommand(@"
                CREATE TABLE IF NOT EXISTS InventoryImports (
                    Id INT NOT NULL AUTO_INCREMENT,
                    ImportCode VARCHAR(30) NOT NULL,
                    InventoryItemId INT NOT NULL,
                    Quantity INT NOT NULL,
                    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
                    SupplierName VARCHAR(150) NULL,
                    Note VARCHAR(500) NULL,
                    ImportDate DATETIME NOT NULL,
                    CreatedByEmployeeId INT NOT NULL DEFAULT 0,
                    PRIMARY KEY (Id),
                    INDEX IX_InventoryImports_InventoryItemId (InventoryItemId),
                    CONSTRAINT FK_InventoryImports_InventoryItems FOREIGN KEY (InventoryItemId) REFERENCES InventoryItems(Id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            ");
        }

        private static void Seed(AppDbContext context)
        {
            if (!context.Rooms.Any())
            {
                context.Rooms.Add(new Room { RoomNumber = "101", RoomType = "Deluxe", PricePerNight = 1500000, Capacity = 2, Status = RoomStatus.Available, Description = "Phòng Deluxe hướng vườn" });
                context.Rooms.Add(new Room { RoomNumber = "201", RoomType = "Premium", PricePerNight = 2500000, Capacity = 3, Status = RoomStatus.Available, Description = "Phòng Premium hướng biển" });
                context.Rooms.Add(new Room { RoomNumber = "301", RoomType = "Royal Suite", PricePerNight = 5000000, Capacity = 4, Status = RoomStatus.Available, Description = "Suite cao cấp đầy đủ tiện nghi" });
            }

            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.Add(new InventoryItem { ItemCode = "VT001", ItemName = "Khăn tắm", Category = "Buồng phòng", Unit = "Cái", Quantity = 100, MinimumQuantity = 20, ImportPrice = 60000, SellingPrice = 90000, Status = InventoryStatus.Available });
                context.InventoryItems.Add(new InventoryItem { ItemCode = "VT002", ItemName = "Nước suối", Category = "Minibar", Unit = "Chai", Quantity = 200, MinimumQuantity = 50, ImportPrice = 5000, SellingPrice = 15000, Status = InventoryStatus.Available });
                context.InventoryItems.Add(new InventoryItem { ItemCode = "VT003", ItemName = "Dầu gội", Category = "Tiện nghi", Unit = "Chai", Quantity = 80, MinimumQuantity = 20, ImportPrice = 25000, SellingPrice = 45000, Status = InventoryStatus.Available });
            }

            if (!context.Employees.Any())
            {
                context.Employees.Add(new Employee { FullName = "Quản trị viên", PhoneNumber = "0900000000", Email = "admin@staymanager_pj.local", IdentityNumber = "000000000", Position = "Admin", Username = "admin", PasswordHash = "admin", Role = UserRole.Admin, IsActive = true });
            }

            context.SaveChanges();
        }
    }
}

