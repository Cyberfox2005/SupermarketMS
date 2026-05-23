using SupermarketMS.ViewModels;
using System.Windows.Controls;

namespace SupermarketMS.Views.Products
{
    public partial class ProductsPage : Page
    {
        private readonly ProductsViewModel _vm;

        public ProductsPage()
        {
            InitializeComponent();
            _vm = new ProductsViewModel();
            DataContext = _vm;
            _vm.Products.CollectionChanged += (_, _) =>
                TxtCount.Text = $"{_vm.Products.Count} items";
        }
    }
}
