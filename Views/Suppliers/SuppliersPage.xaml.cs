using SupermarketMS.ViewModels;
using System.Windows.Controls;

namespace SupermarketMS.Views.Suppliers
{
    public partial class SuppliersPage : Page
    {
        public SuppliersPage()
        {
            InitializeComponent();
            DataContext = new SuppliersViewModel();
        }
    }
}
