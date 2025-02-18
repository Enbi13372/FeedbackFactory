using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    public partial class auswertungsView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        public ObservableCollection<FeedbackData> FeedbackList { get; set; } = new ObservableCollection<FeedbackData>();

        public auswertungsView()
        {
            InitializeComponent();
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            LoadSubjectsFromDatabase();
            LoadClassesFromDatabase();
        }

        
        private void LoadFeedbackData(string subject, string className, DateTime? startDate, DateTime? endDate)
        {
            FeedbackList.Clear();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT z.*, c.Subject, c.ClassName 
                        FROM Zielscheibe z
                        LEFT JOIN Classes c ON z.FormularKey = c.ClassName
                        WHERE (@subject IS NULL OR @subject = '' OR c.Subject = @subject)
                          AND (@className IS NULL OR @className = '' OR c.ClassName = @className)
                          AND (@startDate IS NULL OR z.Erfassungsdatum >= @startDate)
                          AND (@endDate IS NULL OR z.Erfassungsdatum <= @endDate);";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@subject", string.IsNullOrWhiteSpace(subject) ? (object)DBNull.Value : subject);
                        cmd.Parameters.AddWithValue("@className", string.IsNullOrWhiteSpace(className) ? (object)DBNull.Value : className);
                        cmd.Parameters.AddWithValue("@startDate", startDate.HasValue ? (object)startDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@endDate", endDate.HasValue ? (object)endDate.Value : DBNull.Value);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FeedbackList.Add(new FeedbackData
                                {
                                    FormularKey = reader["FormularKey"].ToString(),
                                    Erfassungsdatum = Convert.ToDateTime(reader["Erfassungsdatum"]),
                                    Frage1 = Convert.ToInt32(reader["Frage1"]),
                                    Frage2 = Convert.ToInt32(reader["Frage2"]),
                                    Frage3 = Convert.ToInt32(reader["Frage3"]),
                                    Frage4 = Convert.ToInt32(reader["Frage4"]),
                                    Frage5 = Convert.ToInt32(reader["Frage5"]),
                                    Frage6 = Convert.ToInt32(reader["Frage6"]),
                                    Frage7 = Convert.ToInt32(reader["Frage7"]),
                                    Frage8 = Convert.ToInt32(reader["Frage8"]),
                                    TextRichtig = reader["TextRichtig"].ToString(),
                                    TextAnders = reader["TextAnders"].ToString(),
                                    Subject = reader["Subject"] != DBNull.Value ? reader["Subject"].ToString() : "",
                                    ClassName = reader["ClassName"] != DBNull.Value ? reader["ClassName"].ToString() : ""
                                });
                            }
                        }
                    }
                }
                DataGridFeedback.ItemsSource = FeedbackList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            string selectedSubject = ComboBoxSubject.SelectedItem as string;
            string selectedClass = ComboBoxClass.SelectedItem as string;
            DateTime? startDate = DatePickerStart.SelectedDate;
            DateTime? endDate = DatePickerEnd.SelectedDate;

            LoadFeedbackData(selectedSubject, selectedClass, startDate, endDate);
        }

       
        private void LoadSubjectsFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT Subject FROM Classes ORDER BY Subject;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ComboBoxSubject.Items.Clear();
                            ComboBoxSubject.Items.Add(""); 
                            while (reader.Read())
                            {
                                string subject = reader["Subject"].ToString();
                                ComboBoxSubject.Items.Add(subject);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Fächer: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClassesFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT ClassName FROM Classes ORDER BY ClassName;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            ComboBoxClass.Items.Clear();
                            ComboBoxClass.Items.Add(""); // Leere Auswahl für "kein Filter"
                            while (reader.Read())
                            {
                                string className = reader["ClassName"].ToString();
                                ComboBoxClass.Items.Add(className);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Klassen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

   
    public class FeedbackData
    {
        public string FormularKey { get; set; }
        public DateTime Erfassungsdatum { get; set; }
        public int Frage1 { get; set; }
        public int Frage2 { get; set; }
        public int Frage3 { get; set; }
        public int Frage4 { get; set; }
        public int Frage5 { get; set; }
        public int Frage6 { get; set; }
        public int Frage7 { get; set; }
        public int Frage8 { get; set; }
        public string TextRichtig { get; set; }
        public string TextAnders { get; set; }
        public string Subject { get; set; }
        public string ClassName { get; set; }
    }
}
