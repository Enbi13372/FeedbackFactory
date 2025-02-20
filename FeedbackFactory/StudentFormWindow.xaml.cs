using System;
using System.Windows;

namespace FeedbackFactory
{
    public partial class StudentFormWindow : Window
    {
        // Neuer Konstruktor mit 5 Parametern: key, formName, className, subject und teacher
        public StudentFormWindow(string key, string formName, string className, string subject, string teacher)
        {
            InitializeComponent();

            if (formName == "UnterrichtsBeurteilung")
            {
                // Übergabe von 5 Parametern: key, formName, className, subject, teacher
                var formControl = new UnterrichtsBeurteilung(key, "UnterrichtsBeurteilung", className, subject, teacher);
                this.Content = formControl;
            }
            else if (formName == "Zielscheibe")
            {
                var formControl = new Zielscheibe(key, className, subject, teacher);
                this.Content = formControl;
            }
            else
            {
                MessageBox.Show("Ungültiges Formular.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
