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
        private readonly string _currentTeacherUsername; 
        private bool _isUpdatingFilters;

        public ObservableCollection<ZielscheibeData> ZielscheibeList { get; set; }
            = new ObservableCollection<ZielscheibeData>();

        public auswertungsView()
        {
            InitializeComponent();

            // Beispiel: Den aktuell angemeldeten Lehrer aus einer globalen Property abrufen.
            // Diese Property muss nach erfolgreicher Anmeldung gesetzt werden.
            _currentTeacherUsername = App.Current.Properties["LoggedInTeacher"] as string;

            string configPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            // Falls ein Lehrer angemeldet ist, wird der Lehrerfilter festgelegt und deaktiviert.
            if (!string.IsNullOrEmpty(_currentTeacherUsername))
            {
                ComboBoxTeacher.ItemsSource = new List<string> { _currentTeacherUsername };
                ComboBoxTeacher.SelectedIndex = 0;
                ComboBoxTeacher.IsEnabled = false;
            }

            UpdateFilterCombos();
        }

        #region SelectionChanged-Events der ComboBoxen

        private void ComboBoxSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFilters) return;
            UpdateFilterCombos();
        }

        private void ComboBoxClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFilters) return;
            UpdateFilterCombos();
        }

        private void ComboBoxTeacher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFilters) return;
            UpdateFilterCombos();
        }

        #endregion

        #region Button "Filtern" (für Datum) 

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadZielscheibeData();
        }

        #endregion

        private void UpdateFilterCombos()
        {
            _isUpdatingFilters = true;
            try
            {
                // Aktuelle Auswahl
                string selectedSubject = GetSelectedValue(ComboBoxSubject);
                string selectedClass = GetSelectedValue(ComboBoxClass);
                // Bei deaktivierter Lehrer-ComboBox wird der aktuell angemeldete Lehrer genutzt
                string selectedTeacher = !ComboBoxTeacher.IsEnabled ? _currentTeacherUsername : GetSelectedValue(ComboBoxTeacher);

                // 1) Subject-Liste ermitteln (Klasse + Lehrer)
                var newSubjects = GetDistinctValuesFromDB(
                    columnName: "Subject",
                    otherColumn1: "ClassName", otherValue1: selectedClass,
                    otherColumn2: "Teacher", otherValue2: selectedTeacher
                );

                // 2) Class-Liste ermitteln (Subject + Lehrer)
                var newClasses = GetDistinctValuesFromDB(
                    columnName: "ClassName",
                    otherColumn1: "Subject", otherValue1: selectedSubject,
                    otherColumn2: "Teacher", otherValue2: selectedTeacher
                );

                // 3) Teacher-Liste ermitteln (Subject + Klasse) – nur wenn die ComboBox aktiviert ist
                if (ComboBoxTeacher.IsEnabled)
                {
                    var newTeachers = GetDistinctValuesFromDB(
                        columnName: "Teacher",
                        otherColumn1: "Subject", otherValue1: selectedSubject,
                        otherColumn2: "ClassName", otherValue2: selectedClass
                    );
                    UpdateComboBox(ComboBoxTeacher, newTeachers, ref selectedTeacher);
                }

                // ComboBoxen aktualisieren
                UpdateComboBox(ComboBoxSubject, newSubjects, ref selectedSubject);
                UpdateComboBox(ComboBoxClass, newClasses, ref selectedClass);

                // Anschließend gefilterte Datensätze laden
                LoadZielscheibeData();
            }
            finally
            {
                _isUpdatingFilters = false;
            }
        }

        private List<string> GetDistinctValuesFromDB(
            string columnName,
            string otherColumn1, string otherValue1,
            string otherColumn2, string otherValue2)
        {
            var result = new List<string>();
            try
            {
                using (var conn = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    conn.Open();

                    string sql = $@"
                        SELECT DISTINCT {columnName}
                        FROM Zielscheibe
                        WHERE
                            (@val1 IS NULL OR {otherColumn1} = @val1)
                            AND (@val2 IS NULL OR {otherColumn2} = @val2)
                        ORDER BY {columnName};
                    ";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@val1", (object?)otherValue1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@val2", (object?)otherValue2 ?? DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader[columnName] != DBNull.Value)
                                {
                                    result.Add(reader[columnName].ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei GetDistinctValuesFromDB({columnName}): {ex.Message}");
            }
            return result;
        }

        private void UpdateComboBox(ComboBox combo, List<string> newValues, ref string selectedValue)
        {
            // "(kein Filter)" als erste Option
            var finalList = new List<string> { "(kein Filter)" };
            finalList.AddRange(newValues);
            combo.ItemsSource = finalList;

            if (selectedValue != null && !finalList.Contains(selectedValue))
            {
                selectedValue = null;
            }

            if (selectedValue == null)
            {
                combo.SelectedIndex = 0;
            }
            else
            {
                combo.SelectedItem = selectedValue;
            }
        }

        private string GetSelectedValue(ComboBox combo)
        {
            var val = combo.SelectedItem as string;
            if (string.IsNullOrEmpty(val) || val == "(kein Filter)")
                return null;
            return val;
        }

        private void LoadZielscheibeData()
        {
            ZielscheibeList.Clear();

            // Datum auslesen
            DateTime? startDate = DatePickerStart.SelectedDate;
            DateTime? endDate = DatePickerEnd.SelectedDate;

            // Filter aus den ComboBoxen auslesen
            string subject = GetSelectedValue(ComboBoxSubject);
            string className = GetSelectedValue(ComboBoxClass);
            // Bei deaktivierter ComboBox wird der aktuell angemeldete Lehrer verwendet
            string teacher = !ComboBoxTeacher.IsEnabled ? _currentTeacherUsername : GetSelectedValue(ComboBoxTeacher);

            try
            {
                using (var conn = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT
                            Frage1, Frage2, Frage3, Frage4,
                            Frage5, Frage6, Frage7, Frage8,
                            TextRichtig, TextAnders,
                            Subject, ClassName, Teacher,
                            Erfassungsdatum
                        FROM Zielscheibe
                        WHERE
                            (@subject IS NULL OR Subject = @subject)
                            AND (@className IS NULL OR ClassName = @className)
                            AND (@teacher IS NULL OR Teacher = @teacher)
                            AND (@startDate IS NULL OR Erfassungsdatum >= @startDate)
                            AND (@endDate IS NULL OR Erfassungsdatum <= @endDate)
                    ";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@subject", (object?)subject ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@className", (object?)className ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@teacher", (object?)teacher ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@startDate", (object?)startDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@endDate", (object?)endDate ?? DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
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
                                    Subject = reader["Subject"]?.ToString() ?? "",
                                    ClassName = reader["ClassName"]?.ToString() ?? "",
                                    Teacher = reader["Teacher"]?.ToString() ?? "",
                                    Erfassungsdatum = reader["Erfassungsdatum"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["Erfassungsdatum"])
                                        : DateTime.MinValue
                                };

                                ZielscheibeList.Add(item);
                            }
                        }
                    }
                }

                DataGridFeedback.ItemsSource = ZielscheibeList;
                CalculateAndShowAverages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Daten: {ex.Message}");
            }
        }

        private void CalculateAndShowAverages()
        {
            if (ZielscheibeList.Count == 0)
            {
                TxtAverage.Text = "Keine Daten für diesen Filter.";
                ChartItemsControl.ItemsSource = null;
                return;
            }

            double totalSumOfAverages = 0;
            foreach (var item in ZielscheibeList)
            {
                double avg = (item.Frage1 + item.Frage2 + item.Frage3 + item.Frage4 +
                              item.Frage5 + item.Frage6 + item.Frage7 + item.Frage8) / 8.0;
                totalSumOfAverages += avg;
            }
            double overallAvg = totalSumOfAverages / ZielscheibeList.Count;
            TxtAverage.Text = $"Durchschnitt (8 Fragen): {overallAvg:F2}";

            // Durchschnittswerte pro Frage für das Diagramm
            var avgF1 = ZielscheibeList.Average(x => x.Frage1);
            var avgF2 = ZielscheibeList.Average(x => x.Frage2);
            var avgF3 = ZielscheibeList.Average(x => x.Frage3);
            var avgF4 = ZielscheibeList.Average(x => x.Frage4);
            var avgF5 = ZielscheibeList.Average(x => x.Frage5);
            var avgF6 = ZielscheibeList.Average(x => x.Frage6);
            var avgF7 = ZielscheibeList.Average(x => x.Frage7);
            var avgF8 = ZielscheibeList.Average(x => x.Frage8);

            var chartData = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>("F1", avgF1),
                new KeyValuePair<string, double>("F2", avgF2),
                new KeyValuePair<string, double>("F3", avgF3),
                new KeyValuePair<string, double>("F4", avgF4),
                new KeyValuePair<string, double>("F5", avgF5),
                new KeyValuePair<string, double>("F6", avgF6),
                new KeyValuePair<string, double>("F7", avgF7),
                new KeyValuePair<string, double>("F8", avgF8),
            };

            ChartItemsControl.ItemsSource = chartData;
        }
    }

    public class ZielscheibeData
    {
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
        public string Teacher { get; set; }
        public DateTime Erfassungsdatum { get; set; }
    }
}
