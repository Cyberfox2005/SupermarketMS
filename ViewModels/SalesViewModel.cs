using MySql.Data.MySqlClient;
using SupermarketMS.Data;
using SupermarketMS.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace SupermarketMS.ViewModels
{
    public class SalesViewModel : BaseViewModel
    {
        public ObservableCollection<SaleItem> CartItems  { get; } = new();
        public ObservableCollection<Employee> Employees  { get; } = new();
        public ObservableCollection<Sale>     RecentSales{ get; } = new();

        private string _barcodeInput = "";
        private decimal _discount;
        private decimal _paidAmount;
        private string _paymentMethod = "Cash";
        private int _employeeId;
        private string _statusMessage = "Ready";

        public string  BarcodeInput   { get => _barcodeInput;  set => SetProperty(ref _barcodeInput, value); }
        public decimal Discount       { get => _discount;      set { SetProperty(ref _discount, value); OnPropertyChanged(nameof(NetTotal)); OnPropertyChanged(nameof(Change)); } }
        public decimal PaidAmount     { get => _paidAmount;    set { SetProperty(ref _paidAmount, value); OnPropertyChanged(nameof(Change)); } }
        public string  PaymentMethod  { get => _paymentMethod; set => SetProperty(ref _paymentMethod, value); }
        public int     EmployeeId     { get => _employeeId;    set => SetProperty(ref _employeeId, value); }
        public string  StatusMessage  { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public decimal Subtotal  => CartItems.Sum(i => i.Subtotal);
        public decimal NetTotal  => Subtotal - Discount;
        public decimal Change    => PaidAmount - NetTotal;

        public RelayCommand AddToCartCommand   { get; }
        public RelayCommand RemoveItemCommand  { get; }
        public RelayCommand CompleteSaleCommand{ get; }
        public RelayCommand ClearCartCommand   { get; }

        public SalesViewModel()
        {
            AddToCartCommand    = new RelayCommand(AddToCart);
            RemoveItemCommand   = new RelayCommand<SaleItem>(RemoveItem);
            CompleteSaleCommand = new RelayCommand(CompleteSale, () => CartItems.Count > 0 && PaidAmount >= NetTotal);
            ClearCartCommand    = new RelayCommand(ClearCart);
            LoadEmployees();
            LoadRecentSales();
        }

        private void LoadEmployees()
        {
            Employees.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("SELECT * FROM Employees WHERE IsActive=1 ORDER BY FullName", conn);
            using var rdr  = cmd.ExecuteReader();
            while (rdr.Read())
                Employees.Add(new Employee { Id = rdr.GetInt32("Id"), FullName = rdr.GetString("FullName") });
            if (Employees.Any()) EmployeeId = Employees.First().Id;
        }

        public void LoadRecentSales()
        {
            RecentSales.Clear();
            using var conn = DatabaseHelper.GetConnection();
            string sql = @"SELECT s.*, e.FullName AS EmployeeName
                           FROM Sales s LEFT JOIN Employees e ON e.Id = s.EmployeeId
                           ORDER BY s.SaleDate DESC LIMIT 50";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                RecentSales.Add(new Sale
                {
                    Id            = rdr.GetInt32("Id"),
                    SaleDate      = rdr.GetDateTime("SaleDate"),
                    EmployeeName  = rdr.IsDBNull(rdr.GetOrdinal("EmployeeName")) ? "" : rdr.GetString("EmployeeName"),
                    TotalAmount   = rdr.GetDecimal("TotalAmount"),
                    Discount      = rdr.GetDecimal("Discount"),
                    PaidAmount    = rdr.GetDecimal("PaidAmount"),
                    PaymentMethod = rdr.GetString("PaymentMethod")
                });
        }

        private void AddToCart()
        {
            if (string.IsNullOrWhiteSpace(BarcodeInput)) return;
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                "SELECT * FROM Products WHERE (Barcode=@b OR Name LIKE @n) AND IsActive=1 LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@b", BarcodeInput);
            cmd.Parameters.AddWithValue("@n", $"%{BarcodeInput}%");
            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read()) { StatusMessage = "Product not found."; return; }

            int stock = rdr.GetInt32("Stock");
            var existing = CartItems.FirstOrDefault(i => i.Barcode == rdr.GetString("Barcode"));
            if (existing != null)
            {
                if (existing.Quantity + 1 > stock) { StatusMessage = "Insufficient stock."; return; }
                existing.Quantity++;
            }
            else
            {
                CartItems.Add(new SaleItem
                {
                    ProductId   = rdr.GetInt32("Id"),
                    Barcode     = rdr.IsDBNull(rdr.GetOrdinal("Barcode")) ? "" : rdr.GetString("Barcode"),
                    ProductName = rdr.GetString("Name"),
                    Quantity    = 1,
                    UnitPrice   = rdr.GetDecimal("SalePrice")
                });
            }
            BarcodeInput = "";
            Notify();
            StatusMessage = $"Added: {(existing?.ProductName ?? CartItems.Last().ProductName)}";
        }

        private void RemoveItem(SaleItem? item)
        {
            if (item != null) { CartItems.Remove(item); Notify(); }
        }

        private void CompleteSale()
        {
            if (EmployeeId == 0) { StatusMessage = "Please select a cashier."; return; }
            using var conn = DatabaseHelper.GetConnection();
            using var tx   = conn.BeginTransaction();
            try
            {
                // Insert sale header
                var saleCmd = new MySqlCommand(
                    @"INSERT INTO Sales (EmployeeId,TotalAmount,Discount,PaidAmount,PaymentMethod)
                      VALUES(@eid,@ta,@d,@pa,@pm)", conn, tx);
                saleCmd.Parameters.AddWithValue("@eid", EmployeeId);
                saleCmd.Parameters.AddWithValue("@ta",  Subtotal);
                saleCmd.Parameters.AddWithValue("@d",   Discount);
                saleCmd.Parameters.AddWithValue("@pa",  PaidAmount);
                saleCmd.Parameters.AddWithValue("@pm",  PaymentMethod);
                saleCmd.ExecuteNonQuery();
                int saleId = (int)saleCmd.LastInsertedId;

                // Insert items & update stock
                foreach (var item in CartItems)
                {
                    var itemCmd = new MySqlCommand(
                        "INSERT INTO SaleItems (SaleId,ProductId,Quantity,UnitPrice,Subtotal) VALUES(@sid,@pid,@q,@up,@sub)",
                        conn, tx);
                    itemCmd.Parameters.AddWithValue("@sid", saleId);
                    itemCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@q",   item.Quantity);
                    itemCmd.Parameters.AddWithValue("@up",  item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@sub", item.Subtotal);
                    itemCmd.ExecuteNonQuery();

                    var stockCmd = new MySqlCommand(
                        "UPDATE Products SET Stock = Stock - @q WHERE Id = @id", conn, tx);
                    stockCmd.Parameters.AddWithValue("@q",  item.Quantity);
                    stockCmd.Parameters.AddWithValue("@id", item.ProductId);
                    stockCmd.ExecuteNonQuery();
                }
                tx.Commit();
                StatusMessage = $"Sale #{saleId} completed! Change: {Change:C}";
                ClearCart();
                LoadRecentSales();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void ClearCart()
        {
            CartItems.Clear();
            Discount = PaidAmount = 0;
            BarcodeInput = "";
            Notify();
        }

        private void Notify()
        {
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(NetTotal));
            OnPropertyChanged(nameof(Change));
        }
    }

    // Generic RelayCommand<T>
    public class RelayCommand<T> : System.Windows.Input.ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        { _execute = execute; _canExecute = canExecute; }
        public event EventHandler? CanExecuteChanged
        { add => System.Windows.Input.CommandManager.RequerySuggested += value;
          remove => System.Windows.Input.CommandManager.RequerySuggested -= value; }
        public bool CanExecute(object? p) => _canExecute?.Invoke(p is T t ? t : default) ?? true;
        public void Execute(object? p) => _execute(p is T t ? t : default);
    }
}
