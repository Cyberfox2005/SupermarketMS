# 🛒 Supermarket Management System
**WPF · C# · .NET 8 · MySQL**

---

## Features
| Module | Description |
|---|---|
| 📊 Dashboard | KPI cards, low-stock alerts, live clock |
| 📦 Products & Inventory | Full CRUD, barcode, categories, low-stock badge |
| 🛒 Point of Sale (POS) | Barcode scan, cart, discount, change calc, stock deduction |
| 🏭 Suppliers | Full CRUD, contact management |
| 👤 Employees & Roles | Staff directory, roles, salary |
| 📈 Reports | Daily sales, top products, low-stock table |

---

## Prerequisites
- Visual Studio 2022 (with .NET Desktop workload)
- .NET 8 SDK
- MySQL Server 8+ (local or remote)

---

## Quick Setup

### 1. Clone / Open in Visual Studio
Open `SupermarketMS.sln` (or add the folder as a project).

### 2. Configure your MySQL connection
Edit **`Data/DatabaseHelper.cs`** — change these 5 constants:
```csharp
private const string Server   = "localhost";
private const string Port     = "3306";
private const string Database = "supermarket_db";
private const string User     = "root";
private const string Password = "your_password";
```

### 3. Restore NuGet packages
Visual Studio will restore them automatically on build, or run:
```
dotnet restore
```

### 4. Run
Press **F5**. The app will:
- Auto-create the `supermarket_db` database
- Create all tables
- Seed a default category and admin employee

---

## NuGet Packages Used
| Package | Purpose |
|---|---|
| `MySql.Data` 9.1 | MySQL driver |
| `LiveCharts.Wpf` 0.9.7 | Charts (ready to use in Reports) |
| `CommunityToolkit.Mvvm` 8.3 | MVVM helpers |
| `iTextSharp` 5.5 | PDF export (ready to wire up) |

---

## Project Structure
```
SupermarketMS/
├── Data/
│   └── DatabaseHelper.cs       ← DB connection & table creation
├── Models/
│   └── Models.cs               ← All entity classes
├── ViewModels/
│   ├── BaseViewModel.cs        ← INotifyPropertyChanged + RelayCommand
│   ├── ProductsViewModel.cs    ← Products CRUD logic
│   ├── SalesViewModel.cs       ← POS / cart logic
│   └── OtherViewModels.cs      ← Employees, Suppliers, Reports
├── Views/
│   ├── MainWindow.xaml         ← Shell with sidebar navigation
│   ├── DashboardPage.xaml
│   ├── Products/ProductsPage.xaml
│   ├── Sales/SalesPage.xaml
│   ├── Suppliers/SuppliersPage.xaml
│   ├── Employees/EmployeesPage.xaml
│   └── Reports/ReportsPage.xaml
├── App.xaml                    ← Global styles & color palette
└── SupermarketMS.csproj
```

---

## Extending the Project
- **Purchase Orders**: The `PurchaseOrders` and `PurchaseOrderItems` tables are already created — add a `PurchasesPage` following the same pattern.
- **PDF Export**: `iTextSharp` is referenced — call it from `ReportsViewModel.ExportPdf()`.
- **Barcode printing**: Add `ZXing.Net` NuGet for barcode generation.
- **Charts**: `LiveCharts.Wpf` is referenced — add `<lvc:CartesianChart>` to `ReportsPage.xaml`.
