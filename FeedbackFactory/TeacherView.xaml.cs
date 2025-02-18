using MySql.Data.MySqlClient;
using System;
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

            // Initialisieren des DB-Handlers (korrekter Pfad zur Konfigurationsdatei)
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        private void TeacherView_Loaded(object sender, RoutedEventArgs e)
        {
            // Tastaturfokus zurücksetzen, um unerwünschte Key-Events zu vermeiden
            Keyboard.ClearFocus();

            // Fokussierung des Benutzernamen-Feldes mit leichter Verzögerung
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

            // Überprüfen, ob beide Felder ausgefüllt sind
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Bitte geben Sie sowohl Benutzernamen als auch Passwort ein.", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // SQL-Abfrage, um das gehashte Passwort und die Rolle aus der Datenbank abzurufen
            string query = "SELECT Password, Role FROM Users WHERE Username = @Username;";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", username)
            };

            try
            {
                string storedHashedPassword = null;
                int? role = null;

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
                                role = reader["Role"] != DBNull.Value ? Convert.ToInt32(reader["Role"]) : (int?)null;
                            }
                        }
                    }
                }

                if (storedHashedPassword == null || role == null)
                {
                    // Benutzername nicht in der Datenbank gefunden
                    MessageBox.Show("Benutzername nicht gefunden.", "Login Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Überprüfen des eingegebenen Passworts mittels bcrypt
                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                {
                    // Globalen Property-Wert setzen, sodass der angemeldete Lehrer in der gesamten Anwendung verfügbar ist
                    App.Current.Properties["LoggedInTeacher"] = username;

                    // Öffnen des Hauptfensters (MainWindow) unter Übergabe des Benutzernamens und der Rolle
                    MainWindow mainWindow = new MainWindow(username, role.Value);
                    mainWindow.Show();

                    // Schließen des Login-Fensters (das Fenster, das diese View enthält)
                    Window.GetWindow(this).Close();
                }
                else
                {
                    // Falsches Passwort
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

        private void ResetLBL_MouseDOwn(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new ResetPasswordView();
            }
        }
    }
}
