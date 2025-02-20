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

        public ObservableCollection<ZielscheibeData> ZielscheibeList { get; set; } = new ObservableCollection<ZielscheibeData>();

        public auswertungsView()
        {
            InitializeComponent();

            _currentTeacherUsername = App.Current.Properties["LoggedInTeacher"] as string;

            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            if (!string.IsNullOrEmpty(_currentTeacherUsername))
            {
                ComboBoxTeacher.ItemsSource = new List<string> { _currentTeacherUsername };
                ComboBoxTeacher.SelectedIndex = 0;
                ComboBoxTeacher.IsEnabled = false;
            }

            ComboBoxFormular.ItemsSource = new List<string> { "(kein Filter)", "Zielscheibe", "UnterrichtsBeurteilung" };
            ComboBoxFormular.SelectedIndex = 0;

            UpdateFilterCombos();
        }

        #region ComboBox SelectionChanged-Events

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

        private void ComboBoxFormular_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingFilters) return;
            UpdateFilterCombos();
        }

        #endregion

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterCombos();
        }

        private void UpdateFilterCombos()
        {
            _isUpdatingFilters = true;
            try
            {
                string selectedFormType = GetSelectedValue(ComboBoxFormular);

                if (string.IsNullOrEmpty(selectedFormType))
                {
                    ZielscheibeList.Clear();
                    DataGridFeedback.ItemsSource = null;
                    TxtAverage.Text = "Bitte wählen Sie ein Formular aus.";
                    ChartItemsControl.ItemsSource = null;
                    return;
                }

                string selectedSubject = GetSelectedValue(ComboBoxSubject);
                string selectedClass = GetSelectedValue(ComboBoxClass);
                string selectedTeacher = !ComboBoxTeacher.IsEnabled ? _currentTeacherUsername : GetSelectedValue(ComboBoxTeacher);

                var newSubjects = GetDistinctValuesFromDB("Subject", "ClassName", selectedClass, "Teacher", selectedTeacher);
                var newClasses = GetDistinctValuesFromDB("ClassName", "Subject", selectedSubject, "Teacher", selectedTeacher);
                if (ComboBoxTeacher.IsEnabled)
                {
                    var newTeachers = GetDistinctValuesFromDB("Teacher", "Subject", selectedSubject, "ClassName", selectedClass);
                    UpdateComboBox(ComboBoxTeacher, newTeachers, ref selectedTeacher);
                }
                UpdateComboBox(ComboBoxSubject, newSubjects, ref selectedSubject);
                UpdateComboBox(ComboBoxClass, newClasses, ref selectedClass);

                LoadFeedbackData(selectedSubject, selectedClass, selectedTeacher, selectedFormType);
            }
            finally
            {
                _isUpdatingFilters = false;
            }
        }

        private List<string> GetDistinctValuesFromDB(string columnName, string otherColumn1, string otherValue1, string otherColumn2, string otherValue2)
        {
            var result = new List<string>();
            try
            {
                using (var conn = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    conn.Open();

                    string sql = $@"
                        SELECT DISTINCT `{columnName}`
                        FROM `Zielscheibe`
                        WHERE (@val1 IS NULL OR `{otherColumn1}` = @val1)
                          AND (@val2 IS NULL OR `{otherColumn2}` = @val2)
                        ORDER BY `{columnName}`;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@val1", (object?)otherValue1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@val2", (object?)otherValue2 ?? DBNull.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader[columnName] != DBNull.Value)
                                    result.Add(reader[columnName].ToString());
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
            var finalList = new List<string> { "(kein Filter)" };
            finalList.AddRange(newValues);
            combo.ItemsSource = finalList;

            if (selectedValue != null && !finalList.Contains(selectedValue))
                selectedValue = null;

            if (selectedValue == null)
                combo.SelectedIndex = 0;
            else
                combo.SelectedItem = selectedValue;
        }

        private string GetSelectedValue(ComboBox combo)
        {
            var val = combo.SelectedItem as string;
            if (string.IsNullOrEmpty(val) || val == "(kein Filter)")
                return null;
            return val;
        }

        private void LoadFeedbackData(string subject, string className, string teacher, string formType)
        {
            ZielscheibeList.Clear();

            DateTime? startDate = DatePickerStart.SelectedDate;
            DateTime? endDate = DatePickerEnd.SelectedDate;

            try
            {
                using (var conn = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    conn.Open();

                    string sql = string.Empty;
                    if (formType == "Zielscheibe")
                    {
                        sql = @"
                            SELECT 
                                `Frage1`, `Frage2`, `Frage3`, `Frage4`, `Frage5`, `Frage6`, `Frage7`, `Frage8`,
                                `TextRichtig`, `TextAnders`, `Subject`, `ClassName`, `Teacher`, `Erfassungsdatum`
                            FROM `Zielscheibe`
                            WHERE
                                (@subject IS NULL OR `Subject` = @subject)
                              AND (@className IS NULL OR `ClassName` = @className)
                              AND (@teacher IS NULL OR `Teacher` = @teacher)
                              AND (@startDate IS NULL OR `Erfassungsdatum` >= @startDate)
                              AND (@endDate IS NULL OR `Erfassungsdatum` <= @endDate);";
                    }
                    else
                    {
                        sql = @"
                            SELECT 
                                `Frage1`, `Frage2`, `Frage3`, `Frage4`, `Frage5`, `Frage6`, `Frage7`, `Frage8`, `Frage9`, `Frage10`,
                                `TextGut`, `TextSchlecht`, `TextAnders`, `Subject`, `ClassName`, `Teacher`, `Erfassungsdatum`
                            FROM `UnterrichtsBeurteilung`
                            WHERE
                                (@subject IS NULL OR `Subject` = @subject)
                              AND (@className IS NULL OR `ClassName` = @className)
                              AND (@teacher IS NULL OR `Teacher` = @teacher);";
                    }

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@subject", (object?)subject ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@className", (object?)className ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@teacher", (object?)teacher ?? DBNull.Value);

                        if (formType == "Zielscheibe")
                        {
                            cmd.Parameters.AddWithValue("@startDate", (object?)startDate ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@endDate", (object?)endDate ?? DBNull.Value);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ZielscheibeData();
                                if (formType == "Zielscheibe")
                                {
                                    item.Frage1 = reader["Frage1"] != DBNull.Value ? Convert.ToInt32(reader["Frage1"]) : 0;
                                    item.Frage2 = reader["Frage2"] != DBNull.Value ? Convert.ToInt32(reader["Frage2"]) : 0;
                                    item.Frage3 = reader["Frage3"] != DBNull.Value ? Convert.ToInt32(reader["Frage3"]) : 0;
                                    item.Frage4 = reader["Frage4"] != DBNull.Value ? Convert.ToInt32(reader["Frage4"]) : 0;
                                    item.Frage5 = reader["Frage5"] != DBNull.Value ? Convert.ToInt32(reader["Frage5"]) : 0;
                                    item.Frage6 = reader["Frage6"] != DBNull.Value ? Convert.ToInt32(reader["Frage6"]) : 0;
                                    item.Frage7 = reader["Frage7"] != DBNull.Value ? Convert.ToInt32(reader["Frage7"]) : 0;
                                    item.Frage8 = reader["Frage8"] != DBNull.Value ? Convert.ToInt32(reader["Frage8"]) : 0;
                                    item.TextRichtig = reader["TextRichtig"]?.ToString() ?? "";
                                    item.TextAnders = reader["TextAnders"]?.ToString() ?? "";
                                    item.Subject = reader["Subject"]?.ToString() ?? "";
                                    item.ClassName = reader["ClassName"]?.ToString() ?? "";
                                    item.Teacher = reader["Teacher"]?.ToString() ?? "";
                                    item.Erfassungsdatum = reader["Erfassungsdatum"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["Erfassungsdatum"]) : DateTime.MinValue;
                                    item.Formulartyp = "Zielscheibe";
                                }
                                else
                                {
                                    item.Frage1 = reader["Frage1"] != DBNull.Value ? Convert.ToInt32(reader["Frage1"]) : 0;
                                    item.Frage2 = reader["Frage2"] != DBNull.Value ? Convert.ToInt32(reader["Frage2"]) : 0;
                                    item.Frage3 = reader["Frage3"] != DBNull.Value ? Convert.ToInt32(reader["Frage3"]) : 0;
                                    item.Frage4 = reader["Frage4"] != DBNull.Value ? Convert.ToInt32(reader["Frage4"]) : 0;
                                    item.Frage5 = reader["Frage5"] != DBNull.Value ? Convert.ToInt32(reader["Frage5"]) : 0;
                                    item.Frage6 = reader["Frage6"] != DBNull.Value ? Convert.ToInt32(reader["Frage6"]) : 0;
                                    item.Frage7 = reader["Frage7"] != DBNull.Value ? Convert.ToInt32(reader["Frage7"]) : 0;
                                    item.Frage8 = reader["Frage8"] != DBNull.Value ? Convert.ToInt32(reader["Frage8"]) : 0;
                                    item.Frage9 = reader["Frage9"] != DBNull.Value ? Convert.ToInt32(reader["Frage9"]) : 0;
                                    item.Frage10 = reader["Frage10"] != DBNull.Value ? Convert.ToInt32(reader["Frage10"]) : 0;
                                    item.TextGut = reader["TextGut"]?.ToString() ?? "";
                                    item.TextSchlecht = reader["TextSchlecht"]?.ToString() ?? "";
                                    item.TextAnders = reader["TextAnders"]?.ToString() ?? "";
                                    item.Subject = reader["Subject"]?.ToString() ?? "";
                                    item.ClassName = reader["ClassName"]?.ToString() ?? "";
                                    item.Teacher = reader["Teacher"]?.ToString() ?? "";
                                    item.Erfassungsdatum = reader["Erfassungsdatum"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["Erfassungsdatum"]) : DateTime.MinValue;
                                    item.Formulartyp = "UnterrichtsBeurteilung";
                                }

                                ZielscheibeList.Add(item);
                            }
                        }
                    }
                }

                DataGridFeedback.ItemsSource = ZielscheibeList;
                CalculateAndShowAverages(formType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Daten: {ex.Message}");
            }
        }

        private void CalculateAndShowAverages(string selectedFormType)
        {
            if (ZielscheibeList.Count == 0)
            {
                TxtAverage.Text = "Keine Daten für diesen Filter.";
                ChartItemsControl.ItemsSource = null;
                return;
            }

            double overallAvg = 0;
            if (selectedFormType == "Zielscheibe")
            {
                overallAvg = ZielscheibeList.Average(x =>
                    (x.Frage1 + x.Frage2 + x.Frage3 + x.Frage4 +
                     x.Frage5 + x.Frage6 + x.Frage7 + x.Frage8) / 8.0);
                var chartData = new List<KeyValuePair<string, double>>
                {
                    new KeyValuePair<string, double>("F1", ZielscheibeList.Average(x => x.Frage1)),
                    new KeyValuePair<string, double>("F2", ZielscheibeList.Average(x => x.Frage2)),
                    new KeyValuePair<string, double>("F3", ZielscheibeList.Average(x => x.Frage3)),
                    new KeyValuePair<string, double>("F4", ZielscheibeList.Average(x => x.Frage4)),
                    new KeyValuePair<string, double>("F5", ZielscheibeList.Average(x => x.Frage5)),
                    new KeyValuePair<string, double>("F6", ZielscheibeList.Average(x => x.Frage6)),
                    new KeyValuePair<string, double>("F7", ZielscheibeList.Average(x => x.Frage7)),
                    new KeyValuePair<string, double>("F8", ZielscheibeList.Average(x => x.Frage8))
                };
                ChartItemsControl.ItemsSource = chartData;
            }
            else
            {
                overallAvg = ZielscheibeList.Average(x =>
                    (x.Frage1 + x.Frage2 + x.Frage3 + x.Frage4 +
                     x.Frage5 + x.Frage6 + x.Frage7 + x.Frage8 +
                     x.Frage9 + x.Frage10) / 10.0);
                var chartData = new List<KeyValuePair<string, double>>
                {
                    new KeyValuePair<string, double>("F1", ZielscheibeList.Average(x => x.Frage1)),
                    new KeyValuePair<string, double>("F2", ZielscheibeList.Average(x => x.Frage2)),
                    new KeyValuePair<string, double>("F3", ZielscheibeList.Average(x => x.Frage3)),
                    new KeyValuePair<string, double>("F4", ZielscheibeList.Average(x => x.Frage4)),
                    new KeyValuePair<string, double>("F5", ZielscheibeList.Average(x => x.Frage5)),
                    new KeyValuePair<string, double>("F6", ZielscheibeList.Average(x => x.Frage6)),
                    new KeyValuePair<string, double>("F7", ZielscheibeList.Average(x => x.Frage7)),
                    new KeyValuePair<string, double>("F8", ZielscheibeList.Average(x => x.Frage8)),
                    new KeyValuePair<string, double>("F9", ZielscheibeList.Average(x => x.Frage9)),
                    new KeyValuePair<string, double>("F10", ZielscheibeList.Average(x => x.Frage10))
                };
                ChartItemsControl.ItemsSource = chartData;
            }

            TxtAverage.Text = $"Durchschnitt: {overallAvg:F2}";
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
        public DateTime Erfassungsdatum { get; set; }

        public int Frage9 { get; set; }
        public int Frage10 { get; set; }
        public string TextGut { get; set; }
        public string TextSchlecht { get; set; }

        public string TextRichtig { get; set; }    
        public string TextAnders { get; set; }
        public string Subject { get; set; }
        public string ClassName { get; set; }
        public string Teacher { get; set; }

        public string Formulartyp { get; set; }
    }
}
