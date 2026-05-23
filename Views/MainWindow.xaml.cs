using SupermarketMS.Views.Products;
using SupermarketMS.Views.Sales;
using SupermarketMS.Views.Suppliers;
using SupermarketMS.Views.Employees;
using SupermarketMS.Views.Reports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SupermarketMS.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
        private Button? _activeBtn;

        public MainWindow()
        {
            InitializeComponent();
            _clock.Tick += (_, _) => TxtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            _clock.Start();
            Navigate("Dashboard", BtnDashboard);
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                Navigate(btn.Tag?.ToString() ?? "Dashboard", btn);
        }

        private void Navigate(string page, Button btn)
        {
            // Reset previous active style
            if (_activeBtn != null)
            {
                _activeBtn.Background = System.Windows.Media.Brushes.Transparent;
                _activeBtn.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A8C4E0"));
            }

            // Set active style
            btn.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0A500"));
            btn.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A2535"));
            _activeBtn = btn;

            // Navigate
            Page? view = page switch
            {
                "Dashboard" => new DashboardPage(),
                "Products"  => new ProductsPage(),
                "Sales"     => new SalesPage(),
                "Suppliers" => new SuppliersPage(),
                "Employees" => new EmployeesPage(),
                "Reports"   => new ReportsPage(),
                _           => new DashboardPage()
            };
            MainFrame.Navigate(view);
        }
    }
}
