using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class TeacherView : UserControl
    {
        // Constructor
        public TeacherView()
        {
            InitializeComponent();
        }

        // Click event handler for Login button
        private void LoginBTN_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the username and password
            string username = UsernameTB.Text;
            string password = PasswordTB.Password;

            // Check if username and password are both "test"
            if (username == "test" && password == "test")
            {
                // Show the MainWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Close the LoginWindow (the parent window)
                Window.GetWindow(this).Close(); // This closes the LoginWindow, not the UserControl
            }
            else
            {
                // Display an error message
                MessageBox.Show("Invalid username or password. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Back button click handler (optional)
        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the LoginWindow
            Window loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close current view's window if it’s standalone
            Window.GetWindow(this)?.Close();
        }

        private void RegisterLBL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Find the parent LoginWindow and update its MainContent
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new RegisterView();
            }
        }

    }
}
