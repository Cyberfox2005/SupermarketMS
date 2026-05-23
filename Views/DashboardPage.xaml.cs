using SupermarketMS.ViewModels;
using System.Windows.Controls;

namespace SupermarketMS.Views
{
    public partial class DashboardPage : Page
    {
        private readonly ReportsViewModel _vm = new();

        public DashboardPage()
        {
            InitializeComponent();
            TxtDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            Bind();
        }

        private void Bind()
        {
            var s = _vm.Stats;
            TxtTodaySales.Text    = s.TodaySales.ToString("C");
            TxtTodayTx.Text       = $"{s.TodayTransactions} transactions";
            TxtMonthSales.Text    = s.MonthSales.ToString("C");
            TxtLowStock.Text      = s.LowStockCount.ToString();
            TxtTotalProducts.Text = s.TotalProducts.ToString();
            TxtEmpSup.Text        = $"{s.TotalEmployees} staff · {s.TotalSuppliers} suppliers";

            var low = _vm.LowStockProducts;
            if (low.Any())
            {
                LowStockGrid.ItemsSource = low;
                LowStockGrid.Visibility  = System.Windows.Visibility.Visible;
                TxtNoLow.Visibility      = System.Windows.Visibility.Collapsed;
            }
            else
            {
                LowStockGrid.Visibility = System.Windows.Visibility.Collapsed;
                TxtNoLow.Visibility     = System.Windows.Visibility.Visible;
            }
        }
    }
}
