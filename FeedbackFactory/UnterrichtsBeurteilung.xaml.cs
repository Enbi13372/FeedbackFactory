using System;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    public partial class UnterrichtsBeurteilung : UserControl
    {
        public UnterrichtsBeurteilung(string key, string formName, string className, string subject, string teacher)
        {
            InitializeComponent();
        }

        public UnterrichtsBeurteilung()
            : this("DefaultKey", "UnterrichtsBeurteilung", "DefaultClass", "DefaultSubject", "DefaultTeacher")
        {
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Daten wurden erfasst (Platzhalter)!");
        }
    }
}
