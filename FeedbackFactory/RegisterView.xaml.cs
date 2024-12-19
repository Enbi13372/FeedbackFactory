using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;


namespace FeedbackFactory
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
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
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Connection string
            string connectionString = @"Server=10.0.125.31;Database=feedback;Uid=feedbackuser;Pwd=Test123#;";

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


    }
}
