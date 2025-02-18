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

        // Hier sammeln wir die geladenen Feedback-Daten
        public ObservableCollection<FeedbackData> FeedbackList { get; set; } = new ObservableCollection<FeedbackData>();

        public auswertungsView()
        {
            InitializeComponent();

            // DB-Connection
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            LoadSubjectsFromDatabase();
            LoadClassesFromDatabase();
        }

        /// <summary>
        /// Lädt alle verfügbaren Fächer aus der Tabelle "Subject" und befüllt ComboBoxSubject.
        /// Achtung: Passe die Spaltennamen an deine DB-Struktur an!
        /// </summary>
        private void LoadSubjectsFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Beispiel: Tabelle "Subject" mit Spalten "ID" und "SubjectName".
                    // Falls deine Spalten anders heißen, bitte anpassen.
                    string query = "SELECT ID, SubjectName FROM Subject ORDER BY SubjectName;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var subjectItems = new List<SubjectItem>
                            {
                                // Dummy-Eintrag für "(kein Filter)"
                                new SubjectItem { SubjectID = -1, SubjectName = "(kein Filter)" }
                            };

                            while (reader.Read())
                            {
                                subjectItems.Add(new SubjectItem
                                {
                                    SubjectID = Convert.ToInt32(reader["ID"]),
                                    SubjectName = reader["SubjectName"].ToString()
                                });
                            }

                            ComboBoxSubject.ItemsSource = subjectItems;
                            // Der ComboBox sagen, welche Eigenschaft den Value liefert
                            ComboBoxSubject.SelectedValuePath = nameof(SubjectItem.SubjectID);
                            // Der ComboBox sagen, welche Eigenschaft angezeigt wird
                            ComboBoxSubject.DisplayMemberPath = nameof(SubjectItem.SubjectName);

                            ComboBoxSubject.SelectedIndex = 0; // Standard: "(kein Filter)"
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Fächer: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Lädt alle verfügbaren Klassen aus der Tabelle "Classes" und befüllt ComboBoxClass.
        /// Achtung: Passe die Spaltennamen an deine DB-Struktur an!
        /// </summary>
        private void LoadClassesFromDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Beispiel: Tabelle "Classes" mit Spalten "ID" und "Name".
                    // Falls deine Spalten anders heißen, bitte anpassen.
                    string query = "SELECT ID, Name FROM Classes ORDER BY Name;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            var classItems = new List<ClassItem>
                            {
                                // Dummy-Eintrag für "(kein Filter)"
                                new ClassItem { ClassID = -1, ClassName = "(kein Filter)" }
                            };

                            while (reader.Read())
                            {
                                classItems.Add(new ClassItem
                                {
                                    ClassID = Convert.ToInt32(reader["ID"]),
                                    ClassName = reader["Name"].ToString()
                                });
                            }

                            ComboBoxClass.ItemsSource = classItems;
                            // Der ComboBox sagen, welche Eigenschaft den Value liefert
                            ComboBoxClass.SelectedValuePath = nameof(ClassItem.ClassID);
                            // Der ComboBox sagen, welche Eigenschaft angezeigt wird
                            ComboBoxClass.DisplayMemberPath = nameof(ClassItem.ClassName);

                            ComboBoxClass.SelectedIndex = 0; // Standard: "(kein Filter)"
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Klassen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Klick auf den "Filtern"-Button:
        /// Liest die ausgewählten Filter (Fach, Klasse, Datum) und ruft LoadFeedbackData auf.
        /// </summary>
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            // Ausgewählte IDs ermitteln
            int? selectedSubjectId = ComboBoxSubject.SelectedValue as int?;
            int? selectedClassId = ComboBoxClass.SelectedValue as int?;

            // Falls -1 => "(kein Filter)", dann null verwenden
            if (selectedSubjectId == -1) selectedSubjectId = null;
            if (selectedClassId == -1) selectedClassId = null;

            DateTime? startDate = DatePickerStart.SelectedDate;
            DateTime? endDate = DatePickerEnd.SelectedDate;

            LoadFeedbackData(selectedSubjectId, selectedClassId, startDate, endDate);
        }

        /// <summary>
        /// Lädt gefilterte Daten aus der Tabelle "Zielscheibe" (statt "feedbacks")
        /// und verbindet sie über LEFT JOIN mit "Subject" (Fach) und "Classes" (Klasse).
        /// </summary>
        private void LoadFeedbackData(int? subjectId, int? classId, DateTime? startDate, DateTime? endDate)
        {
            FeedbackList.Clear();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    // Beispielhafter Aufbau der Tabelle "Zielscheibe":
                    //   ID, Ausdauer, Umsetzung, Theoriewert, Praxiswert, Fach (FK), Klasse (FK),
                    //   Thema, Kommentar, Notiz, Erfassungsdatum, ...
                    //
                    // Wichtig: In den SELECTs alias verwenden, damit wir die Werte
                    //          später sauber aus dem Reader auslesen können.
                    //
                    // Die WHERE-Bedingungen nutzen Parameter, damit wir optional filtern können.

                    string query = @"
                        SELECT 
                            z.ID,
                            z.Ausdauer,
                            z.Umsetzung,
                            z.Theoriewert,
                            z.Praxiswert,
                            z.Fach,
                            z.Klasse,
                            z.Thema,
                            z.Kommentar,
                            z.Notiz,
                            z.Erfassungsdatum,

                            s.SubjectName AS SubjectName,
                            c.Name AS ClassName

                        FROM Zielscheibe z
                        LEFT JOIN Subject s ON z.Fach = s.ID
                        LEFT JOIN Classes c ON z.Klasse = c.ID

                        WHERE
                            (@subjectId IS NULL OR z.Fach = @subjectId)
                            AND (@classId IS NULL OR z.Klasse = @classId)
                            AND (@startDate IS NULL OR z.Erfassungsdatum >= @startDate)
                            AND (@endDate IS NULL OR z.Erfassungsdatum <= @endDate)
                    ";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        // Parameter zuweisen
                        cmd.Parameters.AddWithValue("@subjectId", (object?)subjectId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@classId", (object?)classId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@startDate", (object?)startDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@endDate", (object?)endDate ?? DBNull.Value);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Beim Auslesen der Spalten anpassen an die echten Spaltennamen!
                                var item = new FeedbackData
                                {
                                    Id = Convert.ToInt32(reader["ID"]),

                                    // Fremdschlüssel
                                    SubjectKey = reader["Fach"] != DBNull.Value
                                                 ? Convert.ToInt32(reader["Fach"])
                                                 : (int?)null,
                                    ClassKey = reader["Klasse"] != DBNull.Value
                                               ? Convert.ToInt32(reader["Klasse"])
                                               : (int?)null,

                                    // Ratings
                                    Ausdauer = Convert.ToInt32(reader["Ausdauer"]),
                                    Umsetzung = Convert.ToInt32(reader["Umsetzung"]),
                                    Theoriewert = Convert.ToInt32(reader["Theoriewert"]),
                                    Praxiswert = Convert.ToInt32(reader["Praxiswert"]),

                                    // Textliche Felder
                                    Kommentar = reader["Kommentar"] != DBNull.Value
                                                ? reader["Kommentar"].ToString()
                                                : "",
                                    Notiz = reader["Notiz"] != DBNull.Value
                                            ? reader["Notiz"].ToString()
                                            : "",
                                    Thema = reader["Thema"] != DBNull.Value
                                            ? reader["Thema"].ToString()
                                            : "",

                                    Erfassungsdatum = Convert.ToDateTime(reader["Erfassungsdatum"]),

                                    // Aus den Joins
                                    SubjectName = reader["SubjectName"] != DBNull.Value
                                                  ? reader["SubjectName"].ToString()
                                                  : "",
                                    ClassName = reader["ClassName"] != DBNull.Value
                                                ? reader["ClassName"].ToString()
                                                : ""
                                };

                                FeedbackList.Add(item);
                            }
                        }
                    }
                }

                // DataGrid befüllen
                DataGridFeedback.ItemsSource = FeedbackList;

                // Durchschnittswerte berechnen & Diagramm aktualisieren
                CalculateAndShowAverages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Daten: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Berechnet Durchschnittswerte (hier nur über 4 Spalten: Ausdauer, Umsetzung, Theoriewert, Praxiswert)
        /// und aktualisiert das Balkendiagramm.
        /// </summary>
        private void CalculateAndShowAverages()
        {
            if (FeedbackList.Count == 0)
            {
                TxtAverage.Text = "Keine Daten für diesen Filter.";
                ChartItemsControl.ItemsSource = null;
                return;
            }

            // 1) Durchschnitt aller 4 Fragen pro Datensatz bilden, dann Mittelwert über alle Datensätze
            double totalSumOfAverages = 0;
            foreach (var item in FeedbackList)
            {
                double itemAvg = (item.Ausdauer + item.Umsetzung + item.Theoriewert + item.Praxiswert) / 4.0;
                totalSumOfAverages += itemAvg;
            }
            double overallAverage = totalSumOfAverages / FeedbackList.Count;
            TxtAverage.Text = $"Durchschnitt aller Fragen: {overallAverage:F2}";

            // 2) Durchschnitt pro Frage (Spalte) berechnen
            double avgAusdauer = FeedbackList.Average(f => f.Ausdauer);
            double avgUmsetzung = FeedbackList.Average(f => f.Umsetzung);
            double avgTheorie = FeedbackList.Average(f => f.Theoriewert);
            double avgPraxis = FeedbackList.Average(f => f.Praxiswert);

            // 3) Liste von KeyValuePairs für das Diagramm
            var avgData = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("Ausdauer", avgAusdauer),
                new KeyValuePair<string, double>("Umsetzung", avgUmsetzung),
                new KeyValuePair<string, double>("Theorie", avgTheorie),
                new KeyValuePair<string, double>("Praxis", avgPraxis)
            };

            // Diagramm aktualisieren
            ChartItemsControl.ItemsSource = avgData;
        }
    }

    /// <summary>
    /// Datenmodell für die Feedback-Daten aus der Tabelle "Zielscheibe".
    /// Passe es an deine tatsächlichen Spalten an.
    /// </summary>
    public class FeedbackData
    {
        public int Id { get; set; }

        // Fremdschlüssel
        public int? SubjectKey { get; set; }
        public int? ClassKey { get; set; }

        // Numerische Bewertungs-Spalten
        public int Ausdauer { get; set; }
        public int Umsetzung { get; set; }
        public int Theoriewert { get; set; }
        public int Praxiswert { get; set; }

        // Text-Spalten
        public string Kommentar { get; set; }
        public string Notiz { get; set; }
        public string Thema { get; set; }

        public DateTime Erfassungsdatum { get; set; }

        // Aus den Joins (Subject, Classes)
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
    }

    /// <summary>
    /// Hilfsklasse für die Fächer-ComboBox.
    /// </summary>
    public class SubjectItem
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
    }

    /// <summary>
    /// Hilfsklasse für die Klassen-ComboBox.
    /// </summary>
    public class ClassItem
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
    }
}
