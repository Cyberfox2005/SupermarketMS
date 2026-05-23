using MySql.Data.MySqlClient;
using SupermarketMS.Data;
using SupermarketMS.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace SupermarketMS.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        // ── Collections ────────────────────────────────────────────────────────
        public ObservableCollection<Product>  Products   { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Supplier> Suppliers  { get; } = new();

        // ── Selected / Edit ────────────────────────────────────────────────────
        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set { SetProperty(ref _selectedProduct, value); PopulateForm(value); }
        }

        // ── Form fields ────────────────────────────────────────────────────────
        private string _barcode = "", _name = "", _unit = "pcs";
        private decimal _purchasePrice, _salePrice;
        private int _stock, _minStock, _categoryId, _supplierId;
        private DateTime? _expiryDate;
        private string _search = "";

        public string Barcode       { get => _barcode;       set => SetProperty(ref _barcode, value); }
        public string Name          { get => _name;          set => SetProperty(ref _name, value); }
        public string Unit          { get => _unit;          set => SetProperty(ref _unit, value); }
        public decimal PurchasePrice{ get => _purchasePrice; set => SetProperty(ref _purchasePrice, value); }
        public decimal SalePrice    { get => _salePrice;     set => SetProperty(ref _salePrice, value); }
        public int     Stock        { get => _stock;         set => SetProperty(ref _stock, value); }
        public int     MinStock     { get => _minStock;      set => SetProperty(ref _minStock, value); }
        public int     CategoryId   { get => _categoryId;    set => SetProperty(ref _categoryId, value); }
        public int     SupplierId   { get => _supplierId;    set => SetProperty(ref _supplierId, value); }
        public DateTime? ExpiryDate { get => _expiryDate;    set => SetProperty(ref _expiryDate, value); }
        public string Search
        {
            get => _search;
            set { SetProperty(ref _search, value); LoadProducts(); }
        }

        // ── Commands ───────────────────────────────────────────────────────────
        public RelayCommand SaveCommand   { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearCommand  { get; }
        public RelayCommand RefreshCommand{ get; }

        public ProductsViewModel()
        {
            SaveCommand    = new RelayCommand(Save);
            DeleteCommand  = new RelayCommand(Delete, () => SelectedProduct != null);
            ClearCommand   = new RelayCommand(ClearForm);
            RefreshCommand = new RelayCommand(LoadProducts);
            LoadAll();
        }

        // ── Data loading ───────────────────────────────────────────────────────
        public void LoadAll() { LoadCategories(); LoadSuppliers(); LoadProducts(); }

        public void LoadProducts()
        {
            Products.Clear();
            using var conn = DatabaseHelper.GetConnection();
            string sql = @"SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName
                           FROM Products p
                           LEFT JOIN Categories c ON c.Id = p.CategoryId
                           LEFT JOIN Suppliers  s ON s.Id = p.SupplierId
                           WHERE p.IsActive = 1
                             AND (p.Name LIKE @s OR p.Barcode LIKE @s)
                           ORDER BY p.Name";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@s", $"%{Search}%");
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) Products.Add(MapProduct(rdr));
        }

        private void LoadCategories()
        {
            Categories.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("SELECT * FROM Categories ORDER BY Name", conn);
            using var rdr  = cmd.ExecuteReader();
            while (rdr.Read())
                Categories.Add(new Category { Id = rdr.GetInt32("Id"), Name = rdr.GetString("Name") });
        }

        private void LoadSuppliers()
        {
            Suppliers.Clear();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("SELECT * FROM Suppliers WHERE IsActive=1 ORDER BY Name", conn);
            using var rdr  = cmd.ExecuteReader();
            while (rdr.Read())
                Suppliers.Add(new Supplier { Id = rdr.GetInt32("Id"), Name = rdr.GetString("Name") });
        }

        // ── CRUD ───────────────────────────────────────────────────────────────
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name)) { Msg("Product name is required."); return; }
            using var conn = DatabaseHelper.GetConnection();
            if (SelectedProduct == null)
            {
                var sql = @"INSERT INTO Products (Barcode,Name,CategoryId,SupplierId,PurchasePrice,SalePrice,Stock,MinStock,Unit,ExpiryDate)
                            VALUES(@b,@n,@ci,@si,@pp,@sp,@st,@ms,@u,@ed)";
                using var cmd = new MySqlCommand(sql, conn);
                Bind(cmd); cmd.ExecuteNonQuery();
            }
            else
            {
                var sql = @"UPDATE Products SET Barcode=@b,Name=@n,CategoryId=@ci,SupplierId=@si,
                            PurchasePrice=@pp,SalePrice=@sp,Stock=@st,MinStock=@ms,Unit=@u,ExpiryDate=@ed
                            WHERE Id=@id";
                using var cmd = new MySqlCommand(sql, conn);
                Bind(cmd); cmd.Parameters.AddWithValue("@id", SelectedProduct.Id);
                cmd.ExecuteNonQuery();
            }
            ClearForm(); LoadProducts();
        }

        private void Delete()
        {
            if (SelectedProduct == null) return;
            if (MessageBox.Show($"Delete '{SelectedProduct.Name}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            using var conn = DatabaseHelper.GetConnection();
            using var cmd  = new MySqlCommand("UPDATE Products SET IsActive=0 WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", SelectedProduct.Id);
            cmd.ExecuteNonQuery();
            ClearForm(); LoadProducts();
        }

        private void Bind(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@b",  Barcode);
            cmd.Parameters.AddWithValue("@n",  Name);
            cmd.Parameters.AddWithValue("@ci", CategoryId > 0 ? CategoryId : DBNull.Value);
            cmd.Parameters.AddWithValue("@si", SupplierId > 0 ? SupplierId : DBNull.Value);
            cmd.Parameters.AddWithValue("@pp", PurchasePrice);
            cmd.Parameters.AddWithValue("@sp", SalePrice);
            cmd.Parameters.AddWithValue("@st", Stock);
            cmd.Parameters.AddWithValue("@ms", MinStock);
            cmd.Parameters.AddWithValue("@u",  Unit);
            cmd.Parameters.AddWithValue("@ed", ExpiryDate.HasValue ? ExpiryDate.Value : DBNull.Value);
        }

        private void PopulateForm(Product? p)
        {
            if (p == null) { ClearForm(); return; }
            Barcode       = p.Barcode;
            Name          = p.Name;
            CategoryId    = p.CategoryId;
            SupplierId    = p.SupplierId;
            PurchasePrice = p.PurchasePrice;
            SalePrice     = p.SalePrice;
            Stock         = p.Stock;
            MinStock      = p.MinStock;
            Unit          = p.Unit;
            ExpiryDate    = p.ExpiryDate;
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            Barcode = Name = Unit = "";
            PurchasePrice = SalePrice = 0;
            Stock = MinStock = CategoryId = SupplierId = 0;
            ExpiryDate = null;
        }

        private static Product MapProduct(MySqlDataReader r) => new()
        {
            Id            = r.GetInt32("Id"),
            Barcode       = r.IsDBNull(r.GetOrdinal("Barcode"))   ? "" : r.GetString("Barcode"),
            Name          = r.GetString("Name"),
            CategoryId    = r.IsDBNull(r.GetOrdinal("CategoryId")) ? 0 : r.GetInt32("CategoryId"),
            CategoryName  = r.IsDBNull(r.GetOrdinal("CategoryName")) ? "" : r.GetString("CategoryName"),
            SupplierId    = r.IsDBNull(r.GetOrdinal("SupplierId")) ? 0 : r.GetInt32("SupplierId"),
            SupplierName  = r.IsDBNull(r.GetOrdinal("SupplierName")) ? "" : r.GetString("SupplierName"),
            PurchasePrice = r.GetDecimal("PurchasePrice"),
            SalePrice     = r.GetDecimal("SalePrice"),
            Stock         = r.GetInt32("Stock"),
            MinStock      = r.GetInt32("MinStock"),
            Unit          = r.GetString("Unit"),
            ExpiryDate    = r.IsDBNull(r.GetOrdinal("ExpiryDate")) ? null : r.GetDateTime("ExpiryDate"),
            IsActive      = r.GetBoolean("IsActive")
        };

        private static void Msg(string m) =>
            MessageBox.Show(m, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
