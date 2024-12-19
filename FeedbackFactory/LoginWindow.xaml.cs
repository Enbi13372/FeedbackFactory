using System.Windows;

namespace FeedbackFactory
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Button click event for Lehrer
        private void TeacherBTN_Click(object sender, RoutedEventArgs e)
        {
            // Hide the original login content
            LoginContent.Visibility = Visibility.Collapsed;

            // Load TeacherView when Lehrer button is clicked
            MainContent.Content = new TeacherView();
        }

        // Button click event for Schüler
        private void SchuelerBTN_Click(object sender, RoutedEventArgs e)
        {
            // Hide the original login content
            LoginContent.Visibility = Visibility.Collapsed;

            // Load StudentView when Schüler button is clicked
            MainContent.Content = new StudentView();
        }
    }
}
