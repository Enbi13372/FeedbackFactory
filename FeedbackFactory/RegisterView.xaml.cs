using BCrypt.Net; // Add the BCrypt.Net namespace
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class RegisterView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public RegisterView()
        {
            InitializeComponent();

            // Specify the path to the JSON configuration file
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");

            // Initialize the DB handler
            _dbHandler = new DBConnectionHandler(configPath);
        }

        // Event handler for PreviewKeyDown to capture Enter key press
        private void RegisterView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key is pressed
            if (e.Key == Key.Enter)
            {
                // Trigger the Register button click event
                RegisterBTN_Click(RegisterBTN, new RoutedEventArgs());
                e.Handled = true; // Mark the event as handled
            }
        }

        // Event handler for when the UserControl is loaded
        private void RegisterView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the UsernameTB (TextBox for the username) when the view is loaded
            UsernameTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to TeacherView
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new TeacherView();
            }
        }

        private void RegisterBTN_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the passwords
            string password = PasswordTB.Password;
            string confirmPassword = ConfirmPasswordTB.Password;

            // Check if passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.");
                return;
            }

            // Retrieve the username
            string username = UsernameTB.Text;

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Alle Felder müssen ausgefüllt werden. Bitte versuchen Sie es erneut.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // First, check if the username already exists in the database
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username;";
            var checkParameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            try
            {
                using (SqlConnection connection = new SqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddRange(checkParameters);
                        int userCount = (int)cmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            // User already exists
                            MessageBox.Show("Dieser Nutzer existiert bereits.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // If user doesn't exist, proceed with registration
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // Hash the password using bcrypt
                string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password);";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", hashedPassword) // Save the hashed password
                };

                bool success = _dbHandler.ExecuteNonQuery(query, parameters);

                if (success)
                {
                    // Registration successful
                    MessageBox.Show("User successfully registered!");

                    // Navigate back to TeacherView
                    var parentWindow = Window.GetWindow(this) as LoginWindow;
                    if (parentWindow != null)
                    {
                        parentWindow.MainContent.Content = new TeacherView();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to register user.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
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
