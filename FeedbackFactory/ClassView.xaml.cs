using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for ClassView.xaml
    /// </summary>
    public partial class ClassView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        // Constructor to initialize DB connection
        public ClassView()
        {
            InitializeComponent();

            // Specify the path to the JSON configuration file
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");

            // Initialize the DB handler
            _dbHandler = new DBConnectionHandler(configPath);

            // Load all classes from the database
            LoadClasses();
        }

        // Method to load all classes
        private void LoadClasses()
        {
            ObservableCollection<Class> classes = new ObservableCollection<Class>();

            // SQL query to select all columns we're interested in
            string query = "SELECT Teacher, ClassName, Subject, SchoolYear FROM Classes;";

            try
            {
                // Execute the query using ExecuteReader for SELECT
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Read the data and add the class information to the ObservableCollection
                            while (reader.Read())
                            {
                                classes.Add(new Class
                                {
                                    Teacher = reader.GetString("Teacher"),
                                    ClassName = reader.GetString("ClassName"),
                                    Subject = reader.GetString("Subject"),
                                    SchoolYear = reader.GetString("SchoolYear")
                                });
                            }
                        }
                    }
                }

                // Bind the classes to the ListView
                ClassesListView.ItemsSource = classes;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., connection errors)
                MessageBox.Show($"Error loading classes: {ex.Message}");
            }
        }
    }

    // Define a class to represent the data we are interested in
    public class Class
    {
        public string Teacher { get; set; }
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public string SchoolYear { get; set; }
    }
}
