using System;
using System.Collections.ObjectModel;

namespace SupermarketMS.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = "";
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; }
        public string Unit { get; set; } = "pcs";
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLowStock => Stock <= MinStock;
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime HireDate { get; set; } = DateTime.Today;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class Sale
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal Change => PaidAmount - (TotalAmount - Discount);
        public ObservableCollection<SaleItem> Items { get; set; } = new();
    }

    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Barcode { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public ObservableCollection<PurchaseOrderItem> Items { get; set; } = new();
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public decimal UnitCost { get; set; }
        public decimal Subtotal => Quantity * UnitCost;
    }

    public class DashboardStats
    {
        public decimal TodaySales { get; set; }
        public int TodayTransactions { get; set; }
        public int LowStockCount { get; set; }
        public int TotalProducts { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalSuppliers { get; set; }
        public decimal MonthSales { get; set; }
    }
}
