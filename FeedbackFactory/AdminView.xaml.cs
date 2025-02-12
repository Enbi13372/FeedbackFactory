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
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Threading;  // Needed for DispatcherTimer

namespace FeedbackFactory
{
    public partial class AdminView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private DispatcherTimer _timer;
        private DateTime _currentKeyTimestamp = DateTime.MinValue; // Store the key generation timestamp

        public ICommand DeleteUserCommand { get; }
        public ICommand ChangeRoleCommand { get; }

        public ObservableCollection<Benutzer> BenutzerListe { get; set; } = new ObservableCollection<Benutzer>();

        public AdminView()
        {
            InitializeComponent();

            DeleteUserCommand = new RelayCommand<Benutzer>(DeleteUser);
            ChangeRoleCommand = new RelayCommand<Benutzer>(ChangeRole);

            // Set DataContext for binding
            this.DataContext = this;

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            // Load the users and the current registration key
            LadeBenutzerAusDatenbank();
            LoadRegistrationKey();

            // Setup and start the timer to update the validity label every second.
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the validity label every second if a key has been generated.
            if (_currentKeyTimestamp != DateTime.MinValue)
            {
                UpdateValidityLabel(_currentKeyTimestamp);
            }
        }

        private void DeleteUser(Benutzer benutzer)
        {
            if (benutzer == null) return;

            string query = "DELETE FROM Users WHERE Username = @Username";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", benutzer.Name);
                        cmd.ExecuteNonQuery();
                    }
                }

                BenutzerListe.Remove(benutzer);

                MessageBox.Show("Benutzer erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen des Benutzers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeRole(Benutzer benutzer)
        {
            if (benutzer == null) return;

            benutzer.Rolle = benutzer.Rolle == 0 ? 1 : 0;

            string query = "UPDATE Users SET Role = @Role WHERE Username = @Username";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Role", benutzer.Rolle);
                        cmd.Parameters.AddWithValue("@Username", benutzer.Name);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Rolle erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ändern der Rolle: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LadeBenutzerAusDatenbank()
        {
            string query = "SELECT Username, Role FROM Users;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BenutzerListe.Add(new Benutzer
                                {
                                    Name = reader.GetString("Username"),
                                    Rolle = reader.GetInt32("Role")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Benutzer: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Existing AddUserButton_Click (retained as requested)
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;
            var selectedRole = RoleInput.SelectedItem as ComboBoxItem;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || selectedRole == null)
            {
                MessageBox.Show("Bitte geben Sie einen Benutzernamen, ein Passwort und eine Rolle ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int role = int.Parse(selectedRole.Tag.ToString());
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role);";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.ExecuteNonQuery();
                    }
                }

                BenutzerListe.Add(new Benutzer
                {
                    Name = username,
                    Rolle = role
                });

                MessageBox.Show("Benutzer erfolgreich hinzugefügt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                UsernameInput.Text = string.Empty;
                PasswordInput.Password = string.Empty;
                RoleInput.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Benutzers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Copy the generated key to the clipboard.
            if (GeneratedKeyLabel.Content != null)
            {
                Clipboard.SetText(GeneratedKeyLabel.Content.ToString());
            }
        }

        private void GenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Generate a random 8-character key (letters and digits)
            string newKey = GenerateRandomKey(8);
            DateTime now = DateTime.Now;
            _currentKeyTimestamp = now;  // Store the new key's timestamp

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Check if a key already exists in the table.
                    string countQuery = "SELECT COUNT(*) FROM Registrationkeys";
                    int count = 0;
                    using (MySqlCommand cmd = new MySqlCommand(countQuery, connection))
                    {
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (count == 0)
                    {
                        // Insert new key
                        string insertQuery = "INSERT INTO Registrationkeys (`Key`, Expiration) VALUES (@key, @expiration)";
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@key", newKey);
                            cmd.Parameters.AddWithValue("@expiration", now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Update the existing key
                        string updateQuery = "UPDATE Registrationkeys SET `Key` = @key, Expiration = @expiration";
                        using (MySqlCommand cmd = new MySqlCommand(updateQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@key", newKey);
                            cmd.Parameters.AddWithValue("@expiration", now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Update the UI with the new key and its validity.
                GeneratedKeyLabel.Content = newKey;
                UpdateValidityLabel(now);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Generieren des Schlüssels: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the current registration key and its timestamp from the database.
        /// </summary>
        private void LoadRegistrationKey()
        {
            string query = "SELECT `Key`, Expiration FROM Registrationkeys LIMIT 1";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string key = reader.GetString("Key");
                                DateTime timestamp = reader.GetDateTime("Expiration");

                                GeneratedKeyLabel.Content = key;
                                _currentKeyTimestamp = timestamp; // Store the loaded timestamp
                                UpdateValidityLabel(timestamp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Registrierungsschlüssels: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the validity label based on the provided key generation timestamp.
        /// Keys are valid for 7 days. This calculates the remaining time and displays it as "dd [Day/Days] hh [Hour/Hours] mm [Minute/Minutes]".
        /// </summary>
        /// <param name="keyTimestamp">The timestamp when the key was generated.</param>
        private void UpdateValidityLabel(DateTime keyTimestamp)
        {
            // Calculate the expiration as 7 days after key generation.
            DateTime validUntil = keyTimestamp.AddDays(7);
            TimeSpan remaining = validUntil - DateTime.Now;

            // If expired, show zero time remaining.
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            // Determine the correct unit for days, hours, and minutes.
            string dayUnit = remaining.Days == 1 ? "Tag" : "Tage";
            string hourUnit = remaining.Hours == 1 ? "Stunde" : "Stunden";
            string minuteUnit = remaining.Minutes == 1 ? "Minute" : "Minuten";

            // Update the label with the remaining time and units.
            ValidityLabel.Content = $"{remaining.Days} {dayUnit} {remaining.Hours} {hourUnit} {remaining.Minutes} {minuteUnit}";
        }

        /// <summary>
        /// Generates a random alphanumeric key of the specified length.
        /// </summary>
        /// <param name="length">The length of the key.</param>
        /// <returns>A random key string.</returns>
        private string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class Benutzer
    {
        public string Name { get; set; }
        public int Rolle { get; set; }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
