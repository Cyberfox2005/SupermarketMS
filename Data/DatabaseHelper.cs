using MySql.Data.MySqlClient;
using System.Windows;

namespace SupermarketMS.Data
{
    
    {
        // ── Change these to match your MySQL setup ──────────────────────────────
        private const string Server   = "localhost";
        private const string Port     = "3306";
        private const string Database = "supermarket_db";
        private const string User     = "root";
        private const string Password = "2005";
        // ────────────────────────────────────────────────────────────────────────

        public static string ConnectionString =>
            $"Server={Server};Port={Port};Database={Database};Uid={User};Pwd={Password};CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Creates all tables if they don't exist yet.</summary>
        public static void InitializeDatabase()
        {
            try
            {
                // Create database first
                using var bootstrapConn = new MySqlConnection(
                    $"Server={Server};Port={Port};Uid={User};Pwd={Password};CharSet=utf8mb4;");
                bootstrapConn.Open();
                using var createDb = new MySqlCommand(
                    $"CREATE DATABASE IF NOT EXISTS `{Database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
                    bootstrapConn);
                createDb.ExecuteNonQuery();

                // Now create tables
                using var conn = GetConnection();
                string[] ddl =
                {
                    // ── Employees ──────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS Employees (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        FullName    VARCHAR(120) NOT NULL,
                        Role        VARCHAR(60)  NOT NULL,
                        Email       VARCHAR(120) UNIQUE,
                        Phone       VARCHAR(30),
                        HireDate    DATE         NOT NULL,
                        Salary      DECIMAL(12,2) DEFAULT 0,
                        IsActive    TINYINT(1)   DEFAULT 1
                    );",

                    // ── Suppliers ──────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS Suppliers (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        Name        VARCHAR(120) NOT NULL,
                        Contact     VARCHAR(120),
                        Phone       VARCHAR(30),
                        Email       VARCHAR(120),
                        Address     VARCHAR(255),
                        IsActive    TINYINT(1)   DEFAULT 1
                    );",

                    // ── Categories ─────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS Categories (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        Name        VARCHAR(80)  NOT NULL UNIQUE,
                        Description VARCHAR(255)
                    );",

                    // ── Products ───────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS Products (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        Barcode     VARCHAR(50)  UNIQUE,
                        Name        VARCHAR(150) NOT NULL,
                        CategoryId  INT,
                        SupplierId  INT,
                        PurchasePrice DECIMAL(12,2) DEFAULT 0,
                        SalePrice   DECIMAL(12,2) DEFAULT 0,
                        Stock       INT           DEFAULT 0,
                        MinStock    INT           DEFAULT 5,
                        Unit        VARCHAR(20)   DEFAULT 'pcs',
                        ExpiryDate  DATE,
                        IsActive    TINYINT(1)   DEFAULT 1,
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                        FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                    );",

                    // ── Sales ──────────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS Sales (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        SaleDate    DATETIME     DEFAULT CURRENT_TIMESTAMP,
                        EmployeeId  INT,
                        TotalAmount DECIMAL(12,2) DEFAULT 0,
                        Discount    DECIMAL(12,2) DEFAULT 0,
                        PaidAmount  DECIMAL(12,2) DEFAULT 0,
                        PaymentMethod VARCHAR(30) DEFAULT 'Cash',
                        FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
                    );",

                    // ── Sale Items ─────────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS SaleItems (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        SaleId      INT          NOT NULL,
                        ProductId   INT          NOT NULL,
                        Quantity    INT           DEFAULT 1,
                        UnitPrice   DECIMAL(12,2) DEFAULT 0,
                        Subtotal    DECIMAL(12,2) DEFAULT 0,
                        FOREIGN KEY (SaleId)    REFERENCES Sales(Id),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    );",

                    // ── Purchase Orders ────────────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS PurchaseOrders (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        OrderDate   DATETIME     DEFAULT CURRENT_TIMESTAMP,
                        SupplierId  INT,
                        EmployeeId  INT,
                        TotalAmount DECIMAL(12,2) DEFAULT 0,
                        Status      VARCHAR(30)   DEFAULT 'Pending',
                        FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                        FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
                    );",

                    // ── Purchase Order Items ───────────────────────────────────
                    @"CREATE TABLE IF NOT EXISTS PurchaseOrderItems (
                        Id          INT AUTO_INCREMENT PRIMARY KEY,
                        OrderId     INT          NOT NULL,
                        ProductId   INT          NOT NULL,
                        Quantity    INT           DEFAULT 1,
                        UnitCost    DECIMAL(12,2) DEFAULT 0,
                        Subtotal    DECIMAL(12,2) DEFAULT 0,
                        FOREIGN KEY (OrderId)   REFERENCES PurchaseOrders(Id),
                        FOREIGN KEY (ProductId) REFERENCES Products(Id)
                    );",

                    // ── Seed default category ──────────────────────────────────
                    @"INSERT IGNORE INTO Categories (Name, Description)
                      VALUES ('General','Default category');",

                    // ── Seed admin employee ────────────────────────────────────
                    @"INSERT IGNORE INTO Employees (FullName, Role, Email, Phone, HireDate, Salary)
                      VALUES ('Admin User','Manager','admin@supermarket.com','0000000000', CURDATE(), 0);"
                };

                foreach (var sql in ddl)
                {
                    using var cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed:\n{ex.Message}",
                    "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
