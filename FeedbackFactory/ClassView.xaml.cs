using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for ClassView.xaml
    /// </summary>
    public partial class ClassView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private ObservableCollection<Class> _classes;
        private Class _selectedClass;
        private Class _newClass;  // To hold the new class temporarily

        public ClassView()
        {
            InitializeComponent();
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
            LoadClasses();
        }

        private void LoadClasses()
        {
            _classes = new ObservableCollection<Class>();
            string query = "SELECT Teacher, ClassName, Subject, SchoolYear, Department, Grade, ClassSize FROM Classes;";

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
                                    Subject = reader.GetString("Subject"),
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

        private void ClassesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClass = (Class)ClassesListView.SelectedItem;

            if (_selectedClass != null)
            {
                TeacherTextBox.Text = _selectedClass.Teacher;
                ClassNameTextBox.Text = _selectedClass.ClassName;
                SubjectTextBox.Text = _selectedClass.Subject;
                SchoolYearTextBox.Text = _selectedClass.SchoolYear;
                DepartmentTextBox.Text = _selectedClass.Department;
                GradeTextBox.Text = _selectedClass.Grade.ToString();
                ClassSizeTextBox.Text = _selectedClass.ClassSize.ToString();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new class but don't add it to the ListView yet
            _newClass = new Class
            {
                Teacher = "New Teacher",
                ClassName = "New Class",
                Subject = "New Subject",
                SchoolYear = "2025",
                Department = "New Department",
                Grade = 1,
                ClassSize = 1
            };

            // Populate the details panel with the new class data for editing
            TeacherTextBox.Text = _newClass.Teacher;
            ClassNameTextBox.Text = _newClass.ClassName;
            SubjectTextBox.Text = _newClass.Subject;
            SchoolYearTextBox.Text = _newClass.SchoolYear;
            DepartmentTextBox.Text = _newClass.Department;
            GradeTextBox.Text = _newClass.Grade.ToString();
            ClassSizeTextBox.Text = _newClass.ClassSize.ToString();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClass != null)
            {
                // Remove from the database
                DeleteClassFromDatabase(_selectedClass);

                _classes.Remove(_selectedClass);
                _selectedClass = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClass != null)
            {
                // Save the edited class to the database
                _selectedClass.Teacher = TeacherTextBox.Text;
                _selectedClass.ClassName = ClassNameTextBox.Text;
                _selectedClass.Subject = SubjectTextBox.Text;
                _selectedClass.SchoolYear = SchoolYearTextBox.Text;
                _selectedClass.Department = DepartmentTextBox.Text;
                _selectedClass.Grade = int.Parse(GradeTextBox.Text);
                _selectedClass.ClassSize = int.Parse(ClassSizeTextBox.Text);

                // Update the database with the new values
                UpdateClassInDatabase(_selectedClass);
            }
            else if (_newClass != null) // Save the new class if there's one
            {
                _newClass.Teacher = TeacherTextBox.Text;
                _newClass.ClassName = ClassNameTextBox.Text;
                _newClass.Subject = SubjectTextBox.Text;
                _newClass.SchoolYear = SchoolYearTextBox.Text;
                _newClass.Department = DepartmentTextBox.Text;
                _newClass.Grade = int.Parse(GradeTextBox.Text);
                _newClass.ClassSize = int.Parse(ClassSizeTextBox.Text);

                // Insert the new class into the database
                InsertClassIntoDatabase(_newClass);

                // Add the new class to the list (ObservableCollection updates the ListView)
                _classes.Add(_newClass);

                // Refresh the ListView by selecting the newly added class
                ClassesListView.SelectedItem = _newClass;

                // Reset the new class holder for future adds
                _newClass = null;
            }

            // Refresh the ListView (although ObservableCollection handles this)
            ClassesListView.Items.Refresh();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClass != null)
            {
                ClassesListView_SelectionChanged(null, null);
            }
            else if (_newClass != null)
            {
                // If it's a new class that hasn't been saved, clear the details
                TeacherTextBox.Clear();
                ClassNameTextBox.Clear();
                SubjectTextBox.Clear();
                SchoolYearTextBox.Clear();
                DepartmentTextBox.Clear();
                GradeTextBox.Clear();
                ClassSizeTextBox.Clear();
                _newClass = null;
            }
        }

        private void InsertClassIntoDatabase(Class newClass)
        {
            string query = "INSERT INTO Classes (Teacher, ClassName, Subject, SchoolYear, Department, Grade, ClassSize) " +
                           "VALUES (@Teacher, @ClassName, @Subject, @SchoolYear, @Department, @Grade, @ClassSize);";

            ExecuteNonQuery(query, newClass);
        }

        private void UpdateClassInDatabase(Class updatedClass)
        {
            string query = "UPDATE Classes SET Teacher = @Teacher, Subject = @Subject, SchoolYear = @SchoolYear, " +
                           "Department = @Department, Grade = @Grade, ClassSize = @ClassSize " +
                           "WHERE ClassName = @ClassName;";

            ExecuteNonQuery(query, updatedClass);
        }

        private void DeleteClassFromDatabase(Class classToDelete)
        {
            string query = "DELETE FROM Classes WHERE ClassName = @ClassName;";

            using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ClassName", classToDelete.ClassName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteNonQuery(string query, Class classData)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Teacher", classData.Teacher);
                        cmd.Parameters.AddWithValue("@ClassName", classData.ClassName);
                        cmd.Parameters.AddWithValue("@Subject", classData.Subject);
                        cmd.Parameters.AddWithValue("@SchoolYear", classData.SchoolYear);
                        cmd.Parameters.AddWithValue("@Department", classData.Department);
                        cmd.Parameters.AddWithValue("@Grade", classData.Grade);
                        cmd.Parameters.AddWithValue("@ClassSize", classData.ClassSize);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database operation failed: {ex.Message}");
            }
        }
    }

    public class Class
    {
        public string Teacher { get; set; }
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public string SchoolYear { get; set; }
        public string Department { get; set; }
        public int Grade { get; set; }
        public int ClassSize { get; set; }
    }
}
