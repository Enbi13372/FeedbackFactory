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
                                    Teacher = reader.IsDBNull(reader.GetOrdinal("Teacher"))
                                        ? ""
                                        : reader.GetString("Teacher"),
                                    ClassName = reader.IsDBNull(reader.GetOrdinal("ClassName"))
                                        ? ""
                                        : reader.GetString("ClassName"),
                                    SchoolYear = reader.IsDBNull(reader.GetOrdinal("SchoolYear"))
                                        ? ""
                                        : reader.GetString("SchoolYear"),
                                    Department = reader.IsDBNull(reader.GetOrdinal("Department"))
                                        ? ""
                                        : reader.GetString("Department"),
                                    Grade = reader.IsDBNull(reader.GetOrdinal("Grade"))
                                        ? 0
                                        : reader.GetInt32("Grade"),
                                    ClassSize = reader.IsDBNull(reader.GetOrdinal("ClassSize"))
                                        ? 0
                                        : reader.GetInt32("ClassSize")
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
                                string subjectValue = reader.IsDBNull(reader.GetOrdinal("Subject"))
                                    ? ""
                                    : reader.GetString("Subject");

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
                ClassNameTextBox.Text = _selectedClass.ClassName;
                SchoolYearTextBox.Text = _selectedClass.SchoolYear;
                DepartmentTextBox.Text = _selectedClass.Department;
                GradeTextBox.Text = _selectedClass.Grade.ToString();
                ClassSizeTextBox.Text = _selectedClass.ClassSize.ToString();
            }
        }
        #endregion

        #region Save Button (entscheidet, ob Klasse oder Fach gespeichert wird)
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (panelKlasse.Visibility == Visibility.Visible)
            {
                SaveClass();
            }
            else if (panelFach.Visibility == Visibility.Visible)
            {
                SaveSubject();
            }
        }
        #endregion

        #region Speichern von Klassen
        private void SaveClass()
        {
            // Felder prüfen
            if (!int.TryParse(GradeTextBox.Text, out int grade))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Wert für das Grade ein.");
                return;
            }
            if (!int.TryParse(ClassSizeTextBox.Text, out int classSize))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Wert für die Klassengröße ein.");
                return;
            }

            if (_selectedClass == null && _newClass == null)
            {
                _newClass = new Class();
            }

            if (_selectedClass != null)
            {
                // Bestehende Klasse aktualisieren
                _selectedClass.ClassName = ClassNameTextBox.Text;
                _selectedClass.SchoolYear = SchoolYearTextBox.Text;
                _selectedClass.Department = DepartmentTextBox.Text;
                _selectedClass.Grade = grade;
                _selectedClass.ClassSize = classSize;

                UpdateClassInDatabase(_selectedClass);
            }
            else if (_newClass != null)
            {
                // Neue Klasse anlegen
                _newClass.ClassName = ClassNameTextBox.Text;
                _newClass.SchoolYear = SchoolYearTextBox.Text;
                _newClass.Department = DepartmentTextBox.Text;
                _newClass.Grade = grade;
                _newClass.ClassSize = classSize;

                AddClassToDatabase(_newClass);
            }
        }

        private void UpdateClassInDatabase(Class existingClass)
        {
            string query = "UPDATE Classes SET Teacher=@Teacher, SchoolYear=@SchoolYear, Department=@Department, Grade=@Grade, ClassSize=@ClassSize WHERE ClassName=@ClassName;";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Teacher", existingClass.Teacher);
                        cmd.Parameters.AddWithValue("@ClassName", existingClass.ClassName);
                        cmd.Parameters.AddWithValue("@SchoolYear", existingClass.SchoolYear);
                        cmd.Parameters.AddWithValue("@Department", existingClass.Department);
                        cmd.Parameters.AddWithValue("@Grade", existingClass.Grade);
                        cmd.Parameters.AddWithValue("@ClassSize", existingClass.ClassSize);
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

        private void AddClassToDatabase(Class newClass)
        {
            string query = "INSERT INTO Classes (Teacher, ClassName, SchoolYear, Department, Grade, ClassSize) " +
                           "VALUES (@Teacher, @ClassName, @SchoolYear, @Department, @Grade, @ClassSize);";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Teacher", newClass.Teacher);
                        cmd.Parameters.AddWithValue("@ClassName", newClass.ClassName);
                        cmd.Parameters.AddWithValue("@SchoolYear", newClass.SchoolYear);
                        cmd.Parameters.AddWithValue("@Department", newClass.Department);
                        cmd.Parameters.AddWithValue("@Grade", newClass.Grade);
                        cmd.Parameters.AddWithValue("@ClassSize", newClass.ClassSize);
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

        #region Speichern von Fächern
        private void SaveSubject()
        {
            string subject = SubjectTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(subject))
            {
                MessageBox.Show("Bitte geben Sie ein Fach ein.");
                return;
            }

            AddSubjectToDatabase(subject);

            // Textfeld leeren
            SubjectTextBox.Text = "";
        }

        private void AddSubjectToDatabase(string subject)
        {
            string query = "INSERT INTO Subject (Subject) VALUES (@Subject)";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Subject", subject);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadSubjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Fachs: {ex.Message}");
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

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            // Logik für Abbrechen oder Schließen
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
