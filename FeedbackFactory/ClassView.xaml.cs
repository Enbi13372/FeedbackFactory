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
        private ObservableCollection<string> _subjects;  // Liste für die Fächer
        private Class _selectedClass;
        private Class _newClass;

        public ClassView()
        {
            InitializeComponent();
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
            LoadClasses();
            LoadSubjects();  // Lade die Fächer in das ListView
        }

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
                                    Teacher = reader.GetString("Teacher"),
                                    ClassName = reader.GetString("ClassName"),
                                    SchoolYear = reader.GetString("SchoolYear"),
                                    Department = reader.GetString("Department"),
                                    Grade = reader.GetInt32("Grade"),
                                    ClassSize = reader.GetInt32("ClassSize")
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
            _subjects = new ObservableCollection<string>();  // Liste für Fächer
            string query = "SELECT Subject FROM Subjects;";

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
                                _subjects.Add(reader.GetString("Subject"));
                            }
                        }
                    }
                }

                // Setze das ItemsSource des ListView für die Fächer
                SubjectsListView.ItemsSource = _subjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}");
            }
        }

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClass != null)
            {
                _selectedClass.ClassName = ClassNameTextBox.Text;
                _selectedClass.SchoolYear = SchoolYearTextBox.Text;
                _selectedClass.Department = DepartmentTextBox.Text;
                _selectedClass.Grade = int.Parse(GradeTextBox.Text);
                _selectedClass.ClassSize = int.Parse(ClassSizeTextBox.Text);
            }
            else if (_newClass != null)
            {
                _newClass.ClassName = ClassNameTextBox.Text;
                _newClass.SchoolYear = SchoolYearTextBox.Text;
                _newClass.Department = DepartmentTextBox.Text;
                _newClass.Grade = int.Parse(GradeTextBox.Text);
                _newClass.ClassSize = int.Parse(ClassSizeTextBox.Text);

                _classes.Add(_newClass);
                ClassesListView.SelectedItem = _newClass;
                _newClass = null;
            }
            ClassesListView.Items.Refresh();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _newClass = new Class
            {
                Teacher = "New Teacher",
                ClassName = "New Class",
                SchoolYear = "2025",
                Department = "New Department",
                Grade = 1,
                ClassSize = 1
            };

            ClassNameTextBox.Text = _newClass.ClassName;
            SchoolYearTextBox.Text = _newClass.SchoolYear;
            DepartmentTextBox.Text = _newClass.Department;
            GradeTextBox.Text = _newClass.Grade.ToString();
            ClassSizeTextBox.Text = _newClass.ClassSize.ToString();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset fields and close the form
            ClassNameTextBox.Clear();
            SchoolYearTextBox.Clear();
            DepartmentTextBox.Clear();
            GradeTextBox.Clear();
            ClassSizeTextBox.Clear();
        }

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

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Entfernen der ausgewählten Klasse aus der Liste
            if (_selectedClass != null)
            {
                var result = MessageBox.Show($"Möchten Sie die Klasse '{_selectedClass.ClassName}' wirklich entfernen?", "Bestätigung", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _classes.Remove(_selectedClass);
                    _selectedClass = null;

                    // Clear the TextBox fields after removal
                    ClassNameTextBox.Clear();
                    SchoolYearTextBox.Clear();
                    DepartmentTextBox.Clear();
                    GradeTextBox.Clear();
                    ClassSizeTextBox.Clear();
                }
            }
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
}
