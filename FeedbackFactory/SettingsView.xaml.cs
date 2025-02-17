using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeedbackFactory
{
    public partial class SettingsView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private string _currentUsername;

        
        /// <param name="currentUsername">Aktueller Benutzername</param>
        public SettingsView(string currentUsername)
        {
            InitializeComponent();

            _currentUsername = currentUsername;

            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPassword.Password;
            string newPassword = NewPassword.Password;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Bitte füllen Sie beide Passwortfelder aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string storedHashedPassword = null;
                string query = "SELECT Password FROM Users WHERE Username = @Username;";
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", _currentUsername);
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
                    MessageBox.Show("Benutzer nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(oldPassword, storedHashedPassword))
                {
                    MessageBox.Show("Das alte Passwort ist falsch.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE Username = @Username;";
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword", newHashedPassword);
                        cmd.Parameters.AddWithValue("@Username", _currentUsername);
                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Passwort wurde erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                            OldPassword.Password = "";
                            NewPassword.Password = "";
                        }
                        else
                        {
                            MessageBox.Show("Änderung des Passworts fehlgeschlagen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ändern des Passworts: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
        private void ChangeUsername_Click(object sender, RoutedEventArgs e)
        {
            string oldUsername = OldUsername.Text.Trim();
            string newUsername = NewUsername.Text.Trim();

            if (string.IsNullOrWhiteSpace(oldUsername) || string.IsNullOrWhiteSpace(newUsername))
            {
                MessageBox.Show("Bitte füllen Sie beide Username-Felder aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.Equals(oldUsername, _currentUsername, StringComparison.Ordinal))
            {
                MessageBox.Show("Das eingegebene alte Username stimmt nicht mit Ihrem aktuellen Benutzernamen überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @NewUsername;";
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewUsername", newUsername);
                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Der neue Benutzername ist bereits vergeben.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                string updateQuery = "UPDATE Users SET Username = @NewUsername WHERE Username = @OldUsername;";
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewUsername", newUsername);
                        cmd.Parameters.AddWithValue("@OldUsername", _currentUsername);
                        int affectedRows = cmd.ExecuteNonQuery();
                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Username wurde erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                            _currentUsername = newUsername;
                            OldUsername.Text = "";
                            NewUsername.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Änderung des Benutzernamens fehlgeschlagen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ändern des Benutzernamens: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}