using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class StudentView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public StudentView()
        {
            InitializeComponent();

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        private void StudentView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the KeyTB (TextBox for the key) when the view is loaded
            KeyTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the LoginWindow
            Window loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close current view's window if it’s standalone
            Window.GetWindow(this)?.Close();
        }

        private void ContinueBTN_Click(object sender, RoutedEventArgs e)
        {
            string inputKey = KeyTB.Text;

            if (string.IsNullOrWhiteSpace(inputKey))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Schlüssel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Define query to check key in the Feedbackkeys table
            string keyQuery = "SELECT Form, UsesRemaining FROM Feedbackkeys WHERE `Key` = @Key";
            var keyParameter = new MySqlParameter("@Key", inputKey);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Check if the key exists in the Feedbackkeys table
                    using (MySqlCommand cmd = new MySqlCommand(keyQuery, connection))
                    {
                        cmd.Parameters.Add(keyParameter);
                        var reader = cmd.ExecuteReader();

                        if (!reader.Read()) // No matching key found
                        {
                            MessageBox.Show("Ungültiger Schlüssel, bitte versuchen Sie es erneut.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Now safely extract 'Form' and 'UsesRemaining'
                        int form = Convert.ToInt32(reader["Form"]);
                        int usesRemaining = Convert.ToInt32(reader["UsesRemaining"]);

                        // Check if the key has remaining uses
                        if (usesRemaining <= 0)
                        {
                            MessageBox.Show("Dieser Schlüssel hat keine verbleibenden Verwendungen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Check if the key has already been used
                        if (IsKeyUsed(inputKey))
                        {
                            MessageBox.Show("Dieser Schlüssel wurde bereits verwendet.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Now handle the form based on the value of the "Form" column
                        if (form == 1)
                        {
                            // Open UnterrichtsBeurteilung.xaml UserControl in StudentFormWindow
                            var studentFormWindow = new StudentFormWindow(inputKey, "UnterrichtsBeurteilung");
                            studentFormWindow.Show();
                        }
                        else if (form == 2)
                        {
                            // Open Zielscheibe.xaml UserControl in StudentFormWindow
                            var studentFormWindow = new StudentFormWindow(inputKey, "Zielscheibe");
                            studentFormWindow.Show();
                        }
                        else
                        {
                            // This else is to handle the case when the Form column has an unexpected value
                            MessageBox.Show("Ungültiges Formular.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Update the database to decrement UsesRemaining
                        UpdateKeyUses(inputKey);

                        // Close the current window (if any)
                        Window.GetWindow(this)?.Close();
                    }
                }
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



        private bool IsKeyUsed(string inputKey)
        {
            // Check if the key is listed in the usedKeys.json file
            string usedKeysFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "usedKeys.json");

            if (File.Exists(usedKeysFile))
            {
                string[] usedKeys = File.ReadAllLines(usedKeysFile);
                foreach (var usedKey in usedKeys)
                {
                    if (usedKey == inputKey)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateKeyUses(string inputKey)
        {
            // Update the UsesRemaining field by decrementing by 1
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

            // Write the key to usedKeys.json to prevent re-using it
            string usedKeysFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "usedKeys.json");
            File.AppendAllText(usedKeysFile, inputKey + Environment.NewLine);
        }
    }
}
