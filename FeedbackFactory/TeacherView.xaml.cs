using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class TeacherView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public TeacherView()
        {
            InitializeComponent();

            // Initialize the DB handler (assuming the correct path to your config file)
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        private void TeacherView_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset keyboard state to avoid handling residual key presses
            Keyboard.ClearFocus();

            // Optionally, delay focus setup slightly to prevent immediate key handling
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UsernameTB.Focus();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }


        private void TeacherView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginBTN_Click(LoginBTN, new RoutedEventArgs());
            }
        }

        private void LoginBTN_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTB.Text;
            string password = PasswordTB.Password;

            // Check if username and password are filled
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Bitte geben Sie sowohl Benutzernamen als auch Passwort ein.", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Query to retrieve the hashed password from the database
            string query = "SELECT Password FROM Users WHERE Username = @Username;";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", username)
            };

            try
            {
                string storedHashedPassword = null;

                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddRange(parameters);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                storedHashedPassword = reader["Password"].ToString();
                            }
                        }
                    }
                }

                if (storedHashedPassword == null)
                {
                    // Username not found in the database
                    MessageBox.Show("Benutzername nicht gefunden.", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verify the entered password with the hashed password in the database using bcrypt
                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                {
                    // Open the main application window
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Close the login window (this view)
                    Window.GetWindow(this).Close();
                }
                else
                {
                    // Incorrect password
                    MessageBox.Show("Ungültiger Benutzername oder Passwort. Bitte versuchen Sie es erneut.", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Login: {ex.Message}", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            Window loginWindow = new LoginWindow();
            loginWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        private void RegisterLBL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new RegisterView();
            }
        }
    }
}
