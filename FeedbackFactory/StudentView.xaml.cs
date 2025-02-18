using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class StudentView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private readonly string usedKeysFile;

        public StudentView()
        {
            InitializeComponent();
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
            usedKeysFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "usedKeys.json");
        }

        private void StudentView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ContinueBTN_Click(ContinueBTN, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        private void StudentView_Loaded(object sender, RoutedEventArgs e)
        {
            KeyTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            Window loginWindow = new LoginWindow();
            loginWindow.Show();
            Window.GetWindow(this)?.Close();
        }

        private void ContinueBTN_Click(object sender, RoutedEventArgs e)
        {
            string inputKey = KeyTB.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputKey))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Schlüssel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var usedKeys = LoadUsedKeys();

            // If the key was used before and is marked as 1 (fully used), deny access
            if (usedKeys.ContainsKey(inputKey) && usedKeys[inputKey] == 1)
            {
                MessageBox.Show("Dieser Schlüssel wurde bereits verwendet.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string keyQuery = "SELECT Form, UsesRemaining FROM Feedbackkeys WHERE `Key` = @Key";
            var keyParameter = new MySqlParameter("@Key", inputKey);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(keyQuery, connection))
                    {
                        cmd.Parameters.Add(keyParameter);
                        var reader = cmd.ExecuteReader();

                        if (!reader.Read())
                        {
                            MessageBox.Show("Ungültiger Schlüssel, bitte versuchen Sie es erneut.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        int form = Convert.ToInt32(reader["Form"]);
                        int usesRemaining = Convert.ToInt32(reader["UsesRemaining"]);

                        // If key is NOT in JSON and has no uses left in DB, block it
                        if (!usedKeys.ContainsKey(inputKey) && usesRemaining <= 0)
                        {
                            MessageBox.Show("Dieser Schlüssel hat keine verbleibenden Verwendungen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                // Open the correct form
                OpenForm(inputKey);

                // If key is NOT in JSON, update UsesRemaining and add to JSON with 0
                if (!usedKeys.ContainsKey(inputKey))
                {
                    UpdateKeyUses(inputKey);
                    usedKeys[inputKey] = 0;
                    SaveUsedKeys(usedKeys);
                }

                Window.GetWindow(this)?.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Datenbankfehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenForm(string inputKey)
        {
            string keyQuery = "SELECT Form FROM Feedbackkeys WHERE `Key` = @Key";
            var keyParameter = new MySqlParameter("@Key", inputKey);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(keyQuery, connection))
                    {
                        cmd.Parameters.Add(keyParameter);
                        var formValue = cmd.ExecuteScalar();

                        if (formValue == null)
                        {
                            MessageBox.Show("Ungültiges Formular.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        int form = Convert.ToInt32(formValue);

                        if (form == 1)
                        {
                            var studentFormWindow = new StudentFormWindow(inputKey, "UnterrichtsBeurteilung");
                            studentFormWindow.Show();
                        }
                        else if (form == 2)
                        {
                            var studentFormWindow = new StudentFormWindow(inputKey, "Zielscheibe");
                            studentFormWindow.Show();
                        }
                        else
                        {
                            MessageBox.Show("Ungültiges Formular.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen des Formulars: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<string, int> LoadUsedKeys()
        {
            try
            {
                if (File.Exists(usedKeysFile))
                {
                    string json = File.ReadAllText(usedKeysFile);
                    return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der verwendeten Schlüssel: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Dictionary<string, int>();
        }

        private void SaveUsedKeys(Dictionary<string, int> usedKeys)
        {
            try
            {
                string json = JsonSerializer.Serialize(usedKeys, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(usedKeysFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der verwendeten Schlüssel: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateKeyUses(string inputKey)
        {
            string updateQuery = "UPDATE Feedbackkeys SET UsesRemaining = UsesRemaining - 1 WHERE `Key` = @Key";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Key", inputKey)
            };

            bool success = _dbHandler.ExecuteNonQuery(updateQuery, parameters);

            if (!success)
            {
                MessageBox.Show("Die Verwendungsanzahl konnte nicht aktualisiert werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
