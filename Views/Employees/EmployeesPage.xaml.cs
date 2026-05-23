using SupermarketMS.ViewModels;
using System.Windows.Controls;

namespace SupermarketMS.Views.Employees
{
    public partial class EmployeesPage : Page
    {
        public EmployeesPage()
        {
            InitializeComponent();
            DataContext = new EmployeesViewModel();
        }
    }
}
