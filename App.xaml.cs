using SupermarketMS.Data;
using System.Windows;
using namespace;

namespace SupermarketMS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseHelper.InitializeDatabase();
        }
    }
}
