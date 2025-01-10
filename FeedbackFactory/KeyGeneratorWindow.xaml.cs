using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for KeyGeneratorWindow.xaml
    /// </summary>
    public partial class KeyGeneratorWindow : Window
    {
        private readonly string _username;
        private readonly DBConnectionHandler _dbHandler;

        public string SelectedClass { get; set; }

        public KeyGeneratorWindow(string username)
        {
            InitializeComponent();

            _username = username;
        

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            this.DataContext = this;
            LoadClassData();
        }

        private void LoadClassData()
        {
            string query = "SELECT ClassName, Subject FROM Classes WHERE Teacher = @username";

            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", _username);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var classSubjectList = new ObservableCollection<string>();
                        while (reader.Read())
                        {
                            string className = reader["ClassName"].ToString();
                            string subject = reader["Subject"].ToString();
                            classSubjectList.Add($"{className} ({subject})");
                        }

                        if (classSubjectList.Count == 0)
                        {
                            classSubjectList.Add("Keine Klassen verfügbar");
                        }

                        ClassComboBox.ItemsSource = classSubjectList;
                    }
                }
            }
        }

        private void GenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClassComboBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie eine Klasse aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string option = string.Empty;

            switch (true)
            {
                case var _ when Option1.IsChecked == true:
                    option = "Option 1";
                    break;
                case var _ when Option2.IsChecked == true:
                    option = "Option 2";
                    break;
                case var _ when Option3.IsChecked == true:
                    option = "Option 3";
                    break;
                default:
                    MessageBox.Show("Bitte wählen Sie eine Option.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            string generatedKey = GenerateRandomKey(option);
            GeneratedKeyText.Text = generatedKey;
            GeneratedKeyText.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            GenerateKeyButton.IsEnabled = false;

            // Now insert the key and class size into Feedbackkeys table
            InsertFeedbackKey(generatedKey);
        }

        private string GenerateRandomKey(string option)
        {
            int keyLength = 16;
            char[] printableChars = Enumerable.Range(32, 95)
                .Select(i => (char)i)
                .Where(c => !"~|\\/^°".Contains(c))
                .ToArray();

            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new char[keyLength];
                byte[] randomBytes = new byte[keyLength];

                rng.GetBytes(randomBytes);

                for (int i = 0; i < keyLength; i++)
                {
                    key[i] = printableChars[randomBytes[i] % printableChars.Length];
                }

                return new string(key);
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedKeyText.Text))
            {
                Clipboard.SetText(GeneratedKeyText.Text);
                MessageBox.Show("Der Schlüssel wurde in die Zwischenablage kopiert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void InsertFeedbackKey(string generatedKey)
        {
            // Parse the selected class and subject from the ComboBox
            string selectedClass = ClassComboBox.SelectedItem.ToString();
            string[] classSubject = selectedClass.Split('(');
            string className = classSubject[0].Trim();
            string subject = classSubject[1].Trim(')', ' ');

            // Step 1: Fetch ClassSize from Classes table using ClassName and Subject
            int classSize = GetClassSize(className, subject);

            if (classSize == -1)
            {
                MessageBox.Show("Fehler: Die Klassengröße konnte nicht abgerufen werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Step 2: Check if the key already exists in Feedbackkeys table
            if (IsKeyExist(generatedKey))
            {
                MessageBox.Show("Der generierte Schlüssel existiert bereits. Ein neuer Schlüssel wird erstellt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                generatedKey = GenerateRandomKey("Option 1"); // Generate a new unique key
            }

            // Step 3: Insert the new key and class size into the Feedbackkeys table
            string insertQuery = "INSERT INTO Feedbackkeys (`Key`, UsesRemaining) VALUES (@key, @usesRemaining)";
            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@key", generatedKey);
                    cmd.Parameters.AddWithValue("@usesRemaining", classSize);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Der Schlüssel wurde erfolgreich in die Datenbank eingefügt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private int GetClassSize(string className, string subject)
        {
            int classSize = -1;
            string query = "SELECT ClassSize FROM Classes WHERE ClassName = @className AND Subject = @subject";
            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@className", className);
                    cmd.Parameters.AddWithValue("@subject", subject);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        classSize = Convert.ToInt32(result);
                    }
                }
            }
            return classSize;
        }

        private bool IsKeyExist(string generatedKey)
        {
            string query = "SELECT COUNT(*) FROM Feedbackkeys WHERE `Key` = @key";
            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@key", generatedKey);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0; // If count > 0, the key already exists
                }
            }
        }
    }
}
