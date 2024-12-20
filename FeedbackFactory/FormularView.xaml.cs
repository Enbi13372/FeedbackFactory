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
    /// Interaktionslogik für FormularView.xaml
    /// </summary>
    public partial class FormularView : UserControl
    {
        public FormularView()
        {
            InitializeComponent();
        }
        private void Formular1_Click(object sender, RoutedEventArgs e)
        {

            {
                Window popupWindow = new Window
                {
                    Title = "Unterrichtsbeurteilung",
                    Width = 800,
                    Height = 800,
                    Content = new UnterrichtsBeurteilung(),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current.MainWindow
                };

                popupWindow.ShowDialog();
            }

        }

        private void Formular2_Click(object sender, RoutedEventArgs e)
        {

            {
                Window popupWindow = new Window
                {
                    Title = "Zielscheibe",
                    Width = 800,
                    Height = 800,
                    Content = new Zielscheibe(),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current.MainWindow
                };

                popupWindow.ShowDialog();
            }

        }

        private void Formular3_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
