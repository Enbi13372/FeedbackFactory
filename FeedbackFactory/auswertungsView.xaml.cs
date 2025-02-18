using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    public partial class auswertungsView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        // Hier sammeln wir die geladenen Daten aus "Zielscheibe"
        public ObservableCollection<ZielscheibeData> ZielscheibeList { get; set; }
            = new ObservableCollection<ZielscheibeData>();

        public auswertungsView()
        {
            InitializeComponent();

            // DB-Connection
            string configPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            // ComboBoxen füllen
            LoadSubjectsFromDatabase();
            LoadClassesFromDatabase();
            LoadTeachersFromDatabase();
        }

        /// <summary>
        /// Lädt alle distinct Subject-Einträge direkt aus Zielscheibe.
        /// </summary>
        private void LoadSubjectsFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // DISTINCT Subject aus "Zielscheibe" holen
                    string query = "SELECT DISTINCT Subject FROM Zielscheibe ORDER BY Subject;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var subjectList = new List<string>
                            {
                                "(kein Filter)"
                            };

                            while (reader.Read())
                            {
                                if (reader["Subject"] != DBNull.Value)
                                {
                                    subjectList.Add(reader["Subject"].ToString());
                                }
                            }

                            ComboBoxSubject.ItemsSource = subjectList;
                            ComboBoxSubject.SelectedIndex = 0; // Standard: "(kein Filter)"
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Fächer: " + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Lädt alle distinct Klassen-Einträge direkt aus Zielscheibe.
        /// </summary>
        private void LoadClassesFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // DISTINCT ClassName aus "Zielscheibe" holen
                    string query = "SELECT DISTINCT ClassName FROM Zielscheibe ORDER BY ClassName;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var classList = new List<string>
                            {
                                "(kein Filter)"
                            };

                            while (reader.Read())
                            {
                                if (reader["ClassName"] != DBNull.Value)
                                {
                                    classList.Add(reader["ClassName"].ToString());
                                }
                            }

                            ComboBoxClass.ItemsSource = classList;
                            ComboBoxClass.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Klassen: " + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Lädt alle distinct Lehrer-Einträge direkt aus Zielscheibe.
        /// </summary>
        private void LoadTeachersFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // DISTINCT Teacher aus "Zielscheibe" holen
                    string query = "SELECT DISTINCT Teacher FROM Zielscheibe ORDER BY Teacher;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var teacherList = new List<string>
                            {
                                "(kein Filter)"
                            };

                            while (reader.Read())
                            {
                                if (reader["Teacher"] != DBNull.Value)
                                {
                                    teacherList.Add(reader["Teacher"].ToString());
                                }
                            }

                            ComboBoxTeacher.ItemsSource = teacherList;
                            ComboBoxTeacher.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Lehrer: " + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Klick auf den "Filtern"-Button:
        /// Liest die ausgewählten Filter (Fach, Klasse, Lehrer, Datum) und ruft LoadZielscheibeData auf.
        /// </summary>
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            // Subject
            string selectedSubject = ComboBoxSubject.SelectedItem as string;
            if (selectedSubject == "(kein Filter)") selectedSubject = null;

            // ClassName
            string selectedClass = ComboBoxClass.SelectedItem as string;
            if (selectedClass == "(kein Filter)") selectedClass = null;

            // Teacher
            string selectedTeacher = ComboBoxTeacher.SelectedItem as string;
            if (selectedTeacher == "(kein Filter)") selectedTeacher = null;

            // Datum
            DateTime? startDate = DatePickerStart.SelectedDate;
            DateTime? endDate = DatePickerEnd.SelectedDate;

            LoadZielscheibeData(selectedSubject, selectedClass, selectedTeacher, startDate, endDate);
        }

        /// <summary>
        /// Lädt gefilterte Daten aus der Tabelle "Zielscheibe".
        /// Filter auf: Subject, ClassName, Teacher, Erfassungsdatum.
        /// Zeigt und wertet aber nur Frage1..Frage8, TextRichtig, TextAnders.
        /// </summary>
        private void LoadZielscheibeData(string subject, string className, string teacher,
                                         DateTime? startDate, DateTime? endDate)
        {
            ZielscheibeList.Clear();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Wir holen alle relevanten Spalten (Frage1..Frage8, TextRichtig, TextAnders,
                    // Erfassungsdatum, Subject, Teacher, ClassName) aus Zielscheibe
                    string query = @"
                        SELECT 
                            z.Frage1,
                            z.Frage2,
                            z.Frage3,
                            z.Frage4,
                            z.Frage5,
                            z.Frage6,
                            z.Frage7,
                            z.Frage8,
                            z.TextRichtig,
                            z.TextAnders,
                            z.Erfassungsdatum,
                            z.Subject,
                            z.Teacher,
                            z.ClassName

                        FROM Zielscheibe z
                        WHERE
                            (@subject IS NULL OR z.Subject = @subject)
                            AND (@className IS NULL OR z.ClassName = @className)
                            AND (@teacher IS NULL OR z.Teacher = @teacher)
                            AND (@startDate IS NULL OR z.Erfassungsdatum >= @startDate)
                            AND (@endDate IS NULL OR z.Erfassungsdatum <= @endDate)
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        // Parameter belegen
                        cmd.Parameters.AddWithValue("@subject", (object?)subject ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@className", (object?)className ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@teacher", (object?)teacher ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@startDate", (object?)startDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@endDate", (object?)endDate ?? DBNull.Value);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Achtung: Stelle sicher, dass in der DB wirklich INT-Werte für Frage1..Frage8 stehen
                                var item = new ZielscheibeData
                                {
                                    Frage1 = reader["Frage1"] != DBNull.Value ? Convert.ToInt32(reader["Frage1"]) : 0,
                                    Frage2 = reader["Frage2"] != DBNull.Value ? Convert.ToInt32(reader["Frage2"]) : 0,
                                    Frage3 = reader["Frage3"] != DBNull.Value ? Convert.ToInt32(reader["Frage3"]) : 0,
                                    Frage4 = reader["Frage4"] != DBNull.Value ? Convert.ToInt32(reader["Frage4"]) : 0,
                                    Frage5 = reader["Frage5"] != DBNull.Value ? Convert.ToInt32(reader["Frage5"]) : 0,
                                    Frage6 = reader["Frage6"] != DBNull.Value ? Convert.ToInt32(reader["Frage6"]) : 0,
                                    Frage7 = reader["Frage7"] != DBNull.Value ? Convert.ToInt32(reader["Frage7"]) : 0,
                                    Frage8 = reader["Frage8"] != DBNull.Value ? Convert.ToInt32(reader["Frage8"]) : 0,

                                    TextRichtig = reader["TextRichtig"]?.ToString() ?? "",
                                    TextAnders = reader["TextAnders"]?.ToString() ?? "",

                                    Erfassungsdatum = reader["Erfassungsdatum"] != DBNull.Value
                                                     ? Convert.ToDateTime(reader["Erfassungsdatum"])
                                                     : DateTime.MinValue,

                                    Subject = reader["Subject"]?.ToString() ?? "",
                                    Teacher = reader["Teacher"]?.ToString() ?? "",
                                    ClassName = reader["ClassName"]?.ToString() ?? ""
                                };

                                ZielscheibeList.Add(item);
                            }
                        }
                    }
                }

                // DataGrid befüllen
                DataGridFeedback.ItemsSource = ZielscheibeList;

                // Durchschnittswerte berechnen & Diagramm aktualisieren
                CalculateAndShowAverages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Berechnet Durchschnittswerte über die 8 Fragen und aktualisiert das Balkendiagramm.
        /// </summary>
        private void CalculateAndShowAverages()
        {
            if (ZielscheibeList.Count == 0)
            {
                TxtAverage.Text = "Keine Daten für diesen Filter.";
                ChartItemsControl.ItemsSource = null;
                return;
            }

            // 1) Durchschnitt aller 8 Fragen pro Datensatz bilden, dann den Mittelwert über alle Datensätze
            double totalSumOfAverages = 0;
            foreach (var item in ZielscheibeList)
            {
                double itemAvg = (item.Frage1 + item.Frage2 + item.Frage3 + item.Frage4
                                  + item.Frage5 + item.Frage6 + item.Frage7 + item.Frage8) / 8.0;
                totalSumOfAverages += itemAvg;
            }
            double overallAverage = totalSumOfAverages / ZielscheibeList.Count;
            TxtAverage.Text = $"Durchschnitt aller 8 Fragen: {overallAverage:F2}";

            // 2) Durchschnitt pro Frage (Spalte) berechnen
            double avgF1 = ZielscheibeList.Average(x => x.Frage1);
            double avgF2 = ZielscheibeList.Average(x => x.Frage2);
            double avgF3 = ZielscheibeList.Average(x => x.Frage3);
            double avgF4 = ZielscheibeList.Average(x => x.Frage4);
            double avgF5 = ZielscheibeList.Average(x => x.Frage5);
            double avgF6 = ZielscheibeList.Average(x => x.Frage6);
            double avgF7 = ZielscheibeList.Average(x => x.Frage7);
            double avgF8 = ZielscheibeList.Average(x => x.Frage8);

            // 3) KeyValuePairs fürs Diagramm
            var avgData = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("Frage1", avgF1),
                new KeyValuePair<string, double>("Frage2", avgF2),
                new KeyValuePair<string, double>("Frage3", avgF3),
                new KeyValuePair<string, double>("Frage4", avgF4),
                new KeyValuePair<string, double>("Frage5", avgF5),
                new KeyValuePair<string, double>("Frage6", avgF6),
                new KeyValuePair<string, double>("Frage7", avgF7),
                new KeyValuePair<string, double>("Frage8", avgF8),
            };

            // Diagramm aktualisieren
            ChartItemsControl.ItemsSource = avgData;
        }
    }

    /// <summary>
    /// Datenmodell für die Tabelle "Zielscheibe" mit 8 Fragen und Textfeldern.
    /// Erfassungsdatum/Subject/Teacher/ClassName nur für Filter, 
    /// werden aber nicht im DataGrid angezeigt.
    /// </summary>
    public class ZielscheibeData
    {
        // Numerische Bewertungs-Spalten
        public int Frage1 { get; set; }
        public int Frage2 { get; set; }
        public int Frage3 { get; set; }
        public int Frage4 { get; set; }
        public int Frage5 { get; set; }
        public int Frage6 { get; set; }
        public int Frage7 { get; set; }
        public int Frage8 { get; set; }

        // Text-Spalten
        public string TextRichtig { get; set; }
        public string TextAnders { get; set; }

        // Nur fürs Filtern (Datum, Fach, Lehrer, Klasse)
        public DateTime Erfassungsdatum { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string ClassName { get; set; }
    }
}
