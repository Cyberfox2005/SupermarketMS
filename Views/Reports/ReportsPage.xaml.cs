using SupermarketMS.ViewModels;
using System.Windows.Controls;

namespace SupermarketMS.Views.Reports
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            DataContext = new ReportsViewModel();
        }
    }
}
