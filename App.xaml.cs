using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            this.StartupUri = new Uri("RegistToast.xaml", UriKind.Relative);
#endif
            base.OnStartup(e);
        }
    }

}
