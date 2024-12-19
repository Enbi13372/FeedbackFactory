using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaktionslogik für UnterrichtsBeurteilung.xaml
    /// </summary>
    public partial class UnterrichtsBeurteilung : UserControl
    {
        public UnterrichtsBeurteilung()
        {
            InitializeComponent();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Hier könnte Logik zum Speichern stehen.
            // Derzeit nur ein Platzhalter, um den Fehler zu vermeiden.
            MessageBox.Show("Daten wurden erfasst (Platzhalter)!");
        }
    }
}
