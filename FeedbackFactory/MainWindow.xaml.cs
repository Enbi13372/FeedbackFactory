using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new DashboardView();
        }


        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView();
        }

        private void BtnFormulare_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new FormularView();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            {
                MainContent.Content = new SettingsView();
            }
        }

        private void BtnAdminView_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AdminView();
        }

    }
}
