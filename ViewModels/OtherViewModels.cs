using MySql.Data.MySqlClient;
using SupermarketMS.Data;
using SupermarketMS.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace SupermarketMS.ViewModels
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  EMPLOYEES VIEW MODEL
    // ═══════════════════════════════════════════════════════════════════════════
    public class EmployeesViewModel : BaseViewModel
    {
        public ObservableCollection<Employee> Employees { get; } = new();

        private Employee? _selected;
        public Employee? Selected { get => _selected; set { SetProperty(ref _selected, value); Populate(value); } }

        private string _fullName="", _role="Cashier", _email="", _phone="", _search="";
        private decimal _salary;
        private DateTime _hireDate = DateTime.Today;

        public string   FullName  { get => _fullName;  set => SetProperty(ref _fullName, value); }
        public string   Role      { get => _role;      set => SetProperty(ref _role, value); }
        public string   Email     { get => _email;     set => SetProperty(ref _email, value); }
        public string   Phone     { get => _phone;     set => SetProperty(ref _phone, value); }
        public decimal  Salary    { get => _salary;    set => SetProperty(ref _salary, value); }
        public DateTime HireDate  { get => _hireDate;  set => SetProperty(ref _hireDate, value); }
        public string   Search    { get => _search;    set { SetProperty(ref _search, value); Load(); } }

        public string[] Roles { get; } = { "Manager", "Cashier", "Stock Clerk", "Supervisor", "Accountant" };

        public RelayCommand SaveCommand   { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand  { get; }

        public EmployeesViewModel()
        {
            SaveCommand   = new RelayCommand(Save);
            DeleteCommand = new RelayCommand(Delete, () => Selected != null);
            ClearCommand  = new RelayCommand(Clear);
            Load();
        }

        public void Load()
        {
            Employees.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                "SELECT * FROM Employees WHERE IsActive=1 AND FullName LIKE @s ORDER BY FullName", conn);
            cmd.Parameters.AddWithValue("@s", $"%{Search}%");
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Employees.Add(new Employee
                {
                    Id       = rdr.GetInt32("Id"),
                    FullName = rdr.GetString("FullName"),
                    Role     = rdr.GetString("Role"),
                    Email    = rdr.IsDBNull(rdr.GetOrdinal("Email")) ? "" : rdr.GetString("Email"),
                    Phone    = rdr.IsDBNull(rdr.GetOrdinal("Phone")) ? "" : rdr.GetString("Phone"),
                    HireDate = rdr.GetDateTime("HireDate"),
                    Salary   = rdr.GetDecimal("Salary"),
                    IsActive = rdr.GetBoolean("IsActive")
                });
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(FullName)) { Warn("Full name is required."); return; }
            using var conn = DatabaseHelper.GetConnection();
            if (Selected == null)
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO Employees (FullName,Role,Email,Phone,HireDate,Salary) VALUES(@n,@r,@e,@p,@h,@s)", conn);
                BindEmp(cmd); cmd.ExecuteNonQuery();
            }
            else
            {
                using var cmd = new MySqlCommand(
                    "UPDATE Employees SET FullName=@n,Role=@r,Email=@e,Phone=@p,HireDate=@h,Salary=@s WHERE Id=@id", conn);
                BindEmp(cmd); cmd.Parameters.AddWithValue("@id", Selected.Id);
                cmd.ExecuteNonQuery();
            }
            Clear(); Load();
        }

        private void Delete()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Deactivate '{Selected.FullName}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("UPDATE Employees SET IsActive=0 WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", Selected.Id); cmd.ExecuteNonQuery();
            Clear(); Load();
        }

        private void BindEmp(MySqlCommand c)
        {
            c.Parameters.AddWithValue("@n", FullName);
            c.Parameters.AddWithValue("@r", Role);
            c.Parameters.AddWithValue("@e", Email);
            c.Parameters.AddWithValue("@p", Phone);
            c.Parameters.AddWithValue("@h", HireDate.ToString("yyyy-MM-dd"));
            c.Parameters.AddWithValue("@s", Salary);
        }

        private void Populate(Employee? e)
        {
            if (e == null) { Clear(); return; }
            FullName = e.FullName; Role = e.Role; Email = e.Email;
            Phone = e.Phone; Salary = e.Salary; HireDate = e.HireDate;
        }

        private void Clear()
        {
            Selected = null; FullName = ""; Role = "Cashier";
            Email = Phone = ""; Salary = 0; HireDate = DateTime.Today;
        }

        private static void Warn(string m) =>
            MessageBox.Show(m, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  SUPPLIERS VIEW MODEL
    // ═══════════════════════════════════════════════════════════════════════════
    public class SuppliersViewModel : BaseViewModel
    {
        public ObservableCollection<Supplier> Suppliers { get; } = new();

        private Supplier? _selected;
        public Supplier? Selected { get => _selected; set { SetProperty(ref _selected, value); Populate(value); } }

        private string _name="", _contact="", _phone="", _email="", _address="", _search="";
        public string Name    { get => _name;    set => SetProperty(ref _name, value); }
        public string Contact { get => _contact; set => SetProperty(ref _contact, value); }
        public string Phone   { get => _phone;   set => SetProperty(ref _phone, value); }
        public string Email   { get => _email;   set => SetProperty(ref _email, value); }
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        public string Search  { get => _search;  set { SetProperty(ref _search, value); Load(); } }

        public RelayCommand SaveCommand   { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand  { get; }

        public SuppliersViewModel()
        {
            SaveCommand   = new RelayCommand(Save);
            DeleteCommand = new RelayCommand(Delete, () => Selected != null);
            ClearCommand  = new RelayCommand(Clear);
            Load();
        }

        public void Load()
        {
            Suppliers.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                "SELECT * FROM Suppliers WHERE IsActive=1 AND Name LIKE @s ORDER BY Name", conn);
            cmd.Parameters.AddWithValue("@s", $"%{Search}%");
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Suppliers.Add(new Supplier
                {
                    Id      = rdr.GetInt32("Id"),
                    Name    = rdr.GetString("Name"),
                    Contact = rdr.IsDBNull(rdr.GetOrdinal("Contact")) ? "" : rdr.GetString("Contact"),
                    Phone   = rdr.IsDBNull(rdr.GetOrdinal("Phone"))   ? "" : rdr.GetString("Phone"),
                    Email   = rdr.IsDBNull(rdr.GetOrdinal("Email"))   ? "" : rdr.GetString("Email"),
                    Address = rdr.IsDBNull(rdr.GetOrdinal("Address")) ? "" : rdr.GetString("Address"),
                });
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name)) { MessageBox.Show("Name required."); return; }
            using var conn = DatabaseHelper.GetConnection();
            if (Selected == null)
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO Suppliers (Name,Contact,Phone,Email,Address) VALUES(@n,@c,@p,@e,@a)", conn);
                Bind(cmd); cmd.ExecuteNonQuery();
            }
            else
            {
                using var cmd = new MySqlCommand(
                    "UPDATE Suppliers SET Name=@n,Contact=@c,Phone=@p,Email=@e,Address=@a WHERE Id=@id", conn);
                Bind(cmd); cmd.Parameters.AddWithValue("@id", Selected.Id); cmd.ExecuteNonQuery();
            }
            Clear(); Load();
        }

        private void Delete()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Remove '{Selected.Name}'?", "Confirm",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("UPDATE Suppliers SET IsActive=0 WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", Selected.Id); cmd.ExecuteNonQuery();
            Clear(); Load();
        }

        private void Bind(MySqlCommand c)
        {
            c.Parameters.AddWithValue("@n", Name);
            c.Parameters.AddWithValue("@c", Contact);
            c.Parameters.AddWithValue("@p", Phone);
            c.Parameters.AddWithValue("@e", Email);
            c.Parameters.AddWithValue("@a", Address);
        }

        private void Populate(Supplier? s)
        {
            if (s == null) { Clear(); return; }
            Name = s.Name; Contact = s.Contact; Phone = s.Phone;
            Email = s.Email; Address = s.Address;
        }

        private void Clear()
        {
            Selected = null; Name = Contact = Phone = Email = Address = "";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  REPORTS VIEW MODEL
    // ═══════════════════════════════════════════════════════════════════════════
    public class ReportsViewModel : BaseViewModel
    {
        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to   = DateTime.Today;
        public DateTime From { get => _from; set { SetProperty(ref _from, value); } }
        public DateTime To   { get => _to;   set { SetProperty(ref _to, value); } }

        public DashboardStats Stats { get; private set; } = new();
        public ObservableCollection<(string Label, decimal Value)> DailySales { get; } = new();
        public ObservableCollection<Product> LowStockProducts { get; } = new();
        public ObservableCollection<(string Name, decimal Total)> TopProducts  { get; } = new();

        public RelayCommand GenerateCommand { get; }

        public ReportsViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);
            Generate();
        }

        public void Generate()
        {
            LoadStats();
            LoadDailySales();
            LoadLowStock();
            LoadTopProducts();
            OnPropertyChanged(nameof(Stats));
            OnPropertyChanged(nameof(DailySales));
            OnPropertyChanged(nameof(LowStockProducts));
            OnPropertyChanged(nameof(TopProducts));
        }

        private void LoadStats()
        {
            using var conn = DatabaseHelper.GetConnection();
            Stats = new DashboardStats();
            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Sales WHERE DATE(SaleDate)=CURDATE()", conn))
                Stats.TodayTransactions = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand(
                "SELECT COALESCE(SUM(TotalAmount-Discount),0) FROM Sales WHERE DATE(SaleDate)=CURDATE()", conn))
                Stats.TodaySales = Convert.ToDecimal(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand(
                "SELECT COALESCE(SUM(TotalAmount-Discount),0) FROM Sales WHERE MONTH(SaleDate)=MONTH(CURDATE()) AND YEAR(SaleDate)=YEAR(CURDATE())", conn))
                Stats.MonthSales = Convert.ToDecimal(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Products WHERE IsActive=1 AND Stock<=MinStock", conn))
                Stats.LowStockCount = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Products WHERE IsActive=1", conn))
                Stats.TotalProducts = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Employees WHERE IsActive=1", conn))
                Stats.TotalEmployees = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Suppliers WHERE IsActive=1", conn))
                Stats.TotalSuppliers = Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void LoadDailySales()
        {
            DailySales.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                @"SELECT DATE(SaleDate) AS Day, SUM(TotalAmount-Discount) AS Total
                  FROM Sales WHERE SaleDate BETWEEN @f AND @t
                  GROUP BY DATE(SaleDate) ORDER BY Day", conn);
            cmd.Parameters.AddWithValue("@f", From);
            cmd.Parameters.AddWithValue("@t", To.AddDays(1));
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                DailySales.Add((rdr.GetDateTime("Day").ToString("MM/dd"), rdr.GetDecimal("Total")));
        }

        private void LoadLowStock()
        {
            LowStockProducts.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                "SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName FROM Products p LEFT JOIN Categories c ON c.Id=p.CategoryId LEFT JOIN Suppliers s ON s.Id=p.SupplierId WHERE p.IsActive=1 AND p.Stock<=p.MinStock ORDER BY p.Stock ASC LIMIT 20", conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                LowStockProducts.Add(new Product
                {
                    Id           = rdr.GetInt32("Id"),
                    Name         = rdr.GetString("Name"),
                    Stock        = rdr.GetInt32("Stock"),
                    MinStock     = rdr.GetInt32("MinStock"),
                    CategoryName = rdr.IsDBNull(rdr.GetOrdinal("CategoryName")) ? "" : rdr.GetString("CategoryName"),
                    SupplierName = rdr.IsDBNull(rdr.GetOrdinal("SupplierName")) ? "" : rdr.GetString("SupplierName"),
                });
        }

        private void LoadTopProducts()
        {
            TopProducts.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand(
                @"SELECT p.Name, SUM(si.Subtotal) AS Total
                  FROM SaleItems si
                  JOIN Sales s  ON s.Id  = si.SaleId
                  JOIN Products p ON p.Id = si.ProductId
                  WHERE s.SaleDate BETWEEN @f AND @t
                  GROUP BY p.Id, p.Name ORDER BY Total DESC LIMIT 10", conn);
            cmd.Parameters.AddWithValue("@f", From);
            cmd.Parameters.AddWithValue("@t", To.AddDays(1));
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                TopProducts.Add((rdr.GetString("Name"), rdr.GetDecimal("Total")));
        }
    }
}
