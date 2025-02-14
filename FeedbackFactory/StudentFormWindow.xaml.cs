using System;
using System.Windows;

namespace FeedbackFactory
{
    public partial class StudentFormWindow : Window
    {
        public StudentFormWindow(string key, string formName)
        {
            InitializeComponent();

            // Set the key and form name if needed, and load the correct UserControl
            if (formName == "UnterrichtsBeurteilung")
            {
                // Load UnterrichtsBeurteilung.xaml
                var formControl = new UnterrichtsBeurteilung(); // Make sure to instantiate the correct UserControl
                this.Content = formControl;
            }
            else if (formName == "Zielscheibe")
            {
                // Load Zielscheibe.xaml
                var formControl = new Zielscheibe(); // Make sure to instantiate the correct UserControl
                this.Content = formControl;
            }
            else
            {
                MessageBox.Show("Ungültiges Formular.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
