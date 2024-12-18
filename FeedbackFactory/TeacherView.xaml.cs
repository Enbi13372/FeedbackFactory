using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the username and password
            string username = UsernameTB.Text;
            string password = PasswordTB.Password;

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both Username and Password.");
                return;
            }

            // Connection string
            string connectionString = @"Server=10.0.125.31;Database=feedback;Uid=Root;Pwd=feedback;";

            // SQL Query
            string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password);";

            // Connect to the database and execute the query
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        // Execute the query
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User successfully registered!");
                        }
                        else
                        {
                            MessageBox.Show("Failed to register user.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        // Back button click handler (optional)
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the LoginWindow
            Window loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close current view's window if it’s standalone
            Window.GetWindow(this)?.Close();
        }
    }
}
