using SupermarketMS.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SupermarketMS.Views.Sales
{
    public partial class SalesPage : Page
    {
        public SalesPage()
        {
            InitializeComponent();
            DataContext = new SalesViewModel();
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is SalesViewModel vm)
                vm.AddToCartCommand.Execute(null);
        }
    }
}
