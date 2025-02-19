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
    public partial class KeyGeneratorWindow : Window
    {
        private readonly string _username;
        private readonly DBConnectionHandler _dbHandler;

        public string SelectedClass { get; set; }
        public string SelectedSubject { get; set; }

        public KeyGeneratorWindow(string username)
        {
            InitializeComponent();

            _username = username;

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            this.DataContext = this;
            LoadClassData();
            LoadSubjectData();
        }

        private void LoadClassData()
        {
            string query = "SELECT ClassName FROM Classes";

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
                            classSubjectList.Add(className);
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

        private void LoadSubjectData()
        {
            string query = "SELECT Subject FROM Subject";

            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var subjectList = new ObservableCollection<string>();
                        while (reader.Read())
                        {
                            string subject = reader["Subject"].ToString();
                            subjectList.Add(subject);
                        }

                        if (subjectList.Count == 0)
                        {
                            subjectList.Add("Keine Fächer verfügbar");
                        }

                        SubjectComboBox.ItemsSource = subjectList;
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

            if (SubjectComboBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie ein Fach aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int option = 0;

            if (Option1.IsChecked == true)
                option = 1;
            else if (Option2.IsChecked == true)
                option = 2;
            else if (Option3.IsChecked == true)
                option = 3;
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Option.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string generatedKey = GenerateRandomKey(option.ToString());
            GeneratedKeyText.Text = generatedKey;
            GeneratedKeyText.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            GenerateKeyButton.IsEnabled = false;

            InsertFeedbackKey(generatedKey, option);
        }

        private string GenerateRandomKey(string option)
        {
            int keyLength = 16;
            char[] printableChars = Enumerable.Range(32, 95)
                .Select(i => (char)i)
                .Where(c => "~|\\/^°".Contains(c) == false)
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

        private void InsertFeedbackKey(string generatedKey, int option)
        {
            string className = ClassComboBox.SelectedItem.ToString().Trim();
            int classSize = GetClassSize(className);

            if (classSize == -1)
            {
                MessageBox.Show("Fehler: Die Klassengröße konnte nicht abgerufen werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsKeyExist(generatedKey))
            {
                MessageBox.Show("Der generierte Schlüssel existiert bereits. Ein neuer Schlüssel wird erstellt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                generatedKey = GenerateRandomKey("Option 1");
                option = 1; // Ensure the option is set when regenerating
            }

            string insertQuery = "INSERT INTO Feedbackkeys (`Key`, UsesRemaining, Form) VALUES (@key, @usesRemaining, @form)";
            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@key", generatedKey);
                    cmd.Parameters.AddWithValue("@usesRemaining", classSize);
                    cmd.Parameters.AddWithValue("@form", option);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Der Schlüssel wurde erfolgreich in die Datenbank eingefügt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private int GetClassSize(string className)
        {
            int classSize = -1;
            string query = "SELECT ClassSize FROM Classes WHERE ClassName = @className";
            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@className", className);

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
                    return count > 0;
                }
            }
        }
    }
}
