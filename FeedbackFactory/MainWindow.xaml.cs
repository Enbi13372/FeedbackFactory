using System.Text;
using System.ComponentModel;
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
        private readonly string _username;

        public MainWindow(string username)
        {
            InitializeComponent();

            MainContent.Content = new DashboardView();
            _username = username;
            TxtWelcome.Text = $"Willkommen, {_username}";

            // Überwache die Content-Eigenschaft des ContentControls
            DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl))
                .AddValueChanged(MainContent, MainContent_ContentChanged);
        }

        // Event-Handler für Änderungen der Content-Eigenschaft
        private void MainContent_ContentChanged(object sender, EventArgs e)
        {
            AktualisiereButtonSichtbarkeit();
        }

        // Aktualisiert die Sichtbarkeit des Formular erstellen Buttons
        private void AktualisiereButtonSichtbarkeit()
        {
            if (MainContent.Content is FormularView)
            {
                BtnFormularErstellen.Visibility = Visibility.Visible;
            }
            else
            {
                BtnFormularErstellen.Visibility = Visibility.Collapsed;
            }
        }

        // Event-Handler für Button-Klicks
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
            MainContent.Content = new SettingsView();
        }

        private void BtnAdminView_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AdminView();
        }

        private void BtnFormularErstellen_Click(object sender, RoutedEventArgs e)
        {
            // Öffne das KeyGeneratorWindow als Modal-Fenster
            KeyGeneratorWindow keyGeneratorWindow = new KeyGeneratorWindow();
            keyGeneratorWindow.ShowDialog();  // Mit ShowDialog wird das Fenster als Modal-Fenster geöffnet.
        }

    }
}
