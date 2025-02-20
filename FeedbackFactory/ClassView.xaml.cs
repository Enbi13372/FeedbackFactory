using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    public partial class ClassView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private ObservableCollection<Class> _classes;
        private ObservableCollection<string> _subjects;

        private Class _selectedClass;
        private Class _newClass;

        public ClassView()
        {
            InitializeComponent();
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            LoadClasses();
            LoadSubjects();
        }

        #region Load Classes / Subjects
        private void LoadClasses()
        {
            _classes = new ObservableCollection<Class>();
            string query = "SELECT Teacher, ClassName, SchoolYear, Department, Grade, ClassSize FROM Classes;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _classes.Add(new Class
                                {
                                    Teacher = reader.IsDBNull(reader.GetOrdinal("Teacher")) ? "" : reader.GetString("Teacher"),
                                    ClassName = reader.IsDBNull(reader.GetOrdinal("ClassName")) ? "" : reader.GetString("ClassName"),
                                    SchoolYear = reader.IsDBNull(reader.GetOrdinal("SchoolYear")) ? "" : reader.GetString("SchoolYear"),
                                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? "" : reader.GetString("Department"),
                                    Grade = reader.IsDBNull(reader.GetOrdinal("Grade")) ? 0 : reader.GetInt32("Grade"),
                                    ClassSize = reader.IsDBNull(reader.GetOrdinal("ClassSize")) ? 0 : reader.GetInt32("ClassSize")
                                });
                            }
                        }
                    }
                }
                ClassesListView.ItemsSource = _classes;
                ClassesListView.SelectionChanged += ClassesListView_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes: {ex.Message}");
            }
        }

        private void LoadSubjects()
        {
            _subjects = new ObservableCollection<string>();
            string query = "SELECT Subject FROM Subject;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string subjectValue = reader.IsDBNull(reader.GetOrdinal("Subject")) ? "" : reader.GetString("Subject");
                                _subjects.Add(subjectValue);
                            }
                        }
                    }
                }
                SubjectsListView.ItemsSource = _subjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}");
            }
        }
        #endregion

        #region SelectionChanged (Klassenliste)
        private void ClassesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClass = (Class)ClassesListView.SelectedItem;
            if (_selectedClass != null)
            {
                AbteilungComboBox.Text = _selectedClass.Department;
                YearComboBox.Text = _selectedClass.Grade.ToString();
                BuchstabeCombobox.Text = _selectedClass.ClassName.Substring(_selectedClass.ClassName.Length - 1);
                ClassSizeTextBox.Text = _selectedClass.ClassSize.ToString();
            }
        }
        #endregion

        #region Save Class
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveClass();
        }

        private void SaveClass()
        {
            if (!int.TryParse(ClassSizeTextBox.Text, out int classSize))
            {
                MessageBox.Show("Bitte geben Sie eine gültige Klassengröße ein.");
                return;
            }

            string department = AbteilungComboBox.Text;
            string schoolYear = GradeComboBox.Text;             // Schuljahr als String (z.B. "2024/25")
            string buchstabe = BuchstabeCombobox.Text;

            if (string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(schoolYear) || string.IsNullOrWhiteSpace(buchstabe))
            {
                MessageBox.Show("Bitte alle Felder ausfüllen.");
                return;
            }

            if (!int.TryParse(YearComboBox.Text, out int grade))
            {
                MessageBox.Show("Bitte geben Sie eine gültige Klassenstufe (Zahl) ein.");
                return;
            }

            string className = $"{department}{grade}{buchstabe}";

            if (_selectedClass != null)
            {
                _selectedClass.ClassName = className;
                _selectedClass.SchoolYear = schoolYear;
                _selectedClass.Department = department;
                _selectedClass.Grade = grade;
                _selectedClass.ClassSize = classSize;
                UpdateClassInDatabase();
            }
            else
            {
                AddClassToDatabase(className, schoolYear, department, grade, classSize);
            }
        }

        private void UpdateClassInDatabase()
        {
            string query = "UPDATE Classes SET SchoolYear=@SchoolYear, Department=@Department, Grade=@Grade, ClassSize=@ClassSize WHERE ClassName=@ClassName;";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ClassName", _selectedClass.ClassName);
                        cmd.Parameters.AddWithValue("@SchoolYear", _selectedClass.SchoolYear);
                        cmd.Parameters.AddWithValue("@Department", _selectedClass.Department);
                        cmd.Parameters.AddWithValue("@Grade", _selectedClass.Grade);
                        cmd.Parameters.AddWithValue("@ClassSize", _selectedClass.ClassSize);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating class: {ex.Message}");
            }
        }

        private void AddClassToDatabase(string className, string schoolYear, string department, int grade, int classSize)
        {
            string query = "INSERT INTO Classes (ClassName, SchoolYear, Department, Grade, ClassSize) VALUES (@ClassName, @SchoolYear, @Department, @Grade, @ClassSize);";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ClassName", className);
                        cmd.Parameters.AddWithValue("@SchoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@Department", department);
                        cmd.Parameters.AddWithValue("@Grade", grade);
                        cmd.Parameters.AddWithValue("@ClassSize", classSize);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding class: {ex.Message}");
            }
        }
        #endregion

        #region Buttons
        private void BtnKlasse_Click(object sender, RoutedEventArgs e)
        {
            panelKlasse.Visibility = Visibility.Visible;
            panelFach.Visibility = Visibility.Collapsed;
        }

        private void BtnFach_Click(object sender, RoutedEventArgs e)
        {
            panelKlasse.Visibility = Visibility.Collapsed;
            panelFach.Visibility = Visibility.Visible;
        }
        #endregion
    }

    public class Class
    {
        public string Teacher { get; set; }
        public string ClassName { get; set; }
        public string SchoolYear { get; set; }
        public string Department { get; set; }
        public int Grade { get; set; }
        public int ClassSize { get; set; }
    }
}
