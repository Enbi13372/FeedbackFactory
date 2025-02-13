using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class ResetPasswordView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public ResetPasswordView()
        {
            InitializeComponent();
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        private void ResetPasswordView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResetBTN_Click(ResetBTN, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void ResetPasswordView_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new RegisterView();
            }
        }

        private void ResetBTN_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTB.Text;
            string newPassword = NewPasswordTB.Password;
            string confirmNewPassword = ConfirmNewPasswordTB.Password;
            string resetKey = ResetKeyTB.Text;

            if (newPassword != confirmNewPassword)
            {
                MessageBox.Show("Passwörter stimmen nicht überein. Bitte versuchen Sie es erneut.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNewPassword) || string.IsNullOrEmpty(resetKey))
            {
                MessageBox.Show("Alle Felder müssen ausgefüllt werden. Bitte versuchen Sie es erneut.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string keyQuery = "SELECT Expiration FROM Registrationkeys WHERE `Key` = @Key";
            var keyParameter = new MySqlParameter("@Key", resetKey);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    using (MySqlCommand cmd = new MySqlCommand(keyQuery, connection))
                    {
                        cmd.Parameters.Add(keyParameter);
                        var expirationDateObj = cmd.ExecuteScalar();

                        if (expirationDateObj == null)
                        {
                            MessageBox.Show("Schlüssel ungültig, bitte kontaktieren Sie den Systemadministrator.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        DateTime expirationDate = Convert.ToDateTime(expirationDateObj);
                        if ((DateTime.Now - expirationDate).TotalDays > 7)
                        {
                            MessageBox.Show("Schlüssel abgelaufen, bitte kontaktieren Sie den Systemadministrator.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username;";
                    var checkParameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@Username", username)
                    };

                    using (MySqlCommand cmd = new MySqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddRange(checkParameters);
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());

                        if (userCount == 0)
                        {
                            MessageBox.Show("Benutzer nicht gefunden.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                string updateQuery = "UPDATE Users SET Password = @Password WHERE Username = @Username;";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Username", username),
                    new MySqlParameter("@Password", hashedPassword)
                };

                bool success = _dbHandler.ExecuteNonQuery(updateQuery, parameters);

                if (success)
                {
                    MessageBox.Show("Passwort erfolgreich zurückgesetzt!");

                    var parentWindow = Window.GetWindow(this) as LoginWindow;
                    if (parentWindow != null)
                    {
                        parentWindow.MainContent.Content = new TeacherView();
                    }
                }
                else
                {
                    MessageBox.Show("Passwort konnte nicht zurückgesetzt werden.", "Zurücksetzen Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
        }
    }
}