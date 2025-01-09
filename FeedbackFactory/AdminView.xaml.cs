using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Data.Common;


namespace FeedbackFactory
{
    /// <summary>
    /// Interaktionslogik für AdminView.xaml
    /// </summary>
    public partial class AdminView : UserControl
    {

        private readonly DBConnectionHandler _dbHandler;
        
        public AdminView()
        {
            InitializeComponent();

            // Specify the path to the JSON configuration file
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");

            // Initialize the DB handler
            _dbHandler = new DBConnectionHandler(configPath);


            string query = "SELECT Username FROM Users";


            try
            {
                // Benutzernamen direkt an die ListBox binden
                UserListBox.ItemsSource = (System.Collections.IEnumerable)_dbHandler.ExecuteQuery(query);
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung
                MessageBox.Show("Fehler beim Abrufen der Benutzernamen: " + ex.Message);
            }


        }
       

   

















    }
}
