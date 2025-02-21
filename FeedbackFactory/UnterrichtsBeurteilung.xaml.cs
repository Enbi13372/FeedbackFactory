using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace FeedbackFactory
{
    public partial class UnterrichtsBeurteilung : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public string FormularKey { get; set; }
        public string FormName { get; set; }
        public string ClassName { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }

        public UnterrichtsBeurteilung(string key, string formName, string className, string subject, string teacher)
        {
            InitializeComponent();

            FormularKey = key;
            FormName = formName;
            ClassName = className;
            Subject = subject;
            Teacher = teacher;

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        public UnterrichtsBeurteilung()
            : this("DefaultKey", "UnterrichtsBeurteilung", "DefaultClass", "DefaultSubject", "DefaultTeacher")
        {
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            int frage1 = GetRating(rdoFrage1_4, rdoFrage1_3, rdoFrage1_2, rdoFrage1_1);
            int frage2 = GetRating(rdoFrage2_4, rdoFrage2_3, rdoFrage2_2, rdoFrage2_1);
            int frage3 = GetRating(rdoFrage3_4, rdoFrage3_3, rdoFrage3_2, rdoFrage3_1);
            int frage4 = GetRating(rdoFrage4_4, rdoFrage4_3, rdoFrage4_2, rdoFrage4_1);
            int frage5 = GetRating(rdoFrage5_4, rdoFrage5_3, rdoFrage5_2, rdoFrage5_1);
            int frage6 = GetRating(rdoFrage6_4, rdoFrage6_3, rdoFrage6_2, rdoFrage6_1);
            int frage7 = GetRating(rdoFrage7_4, rdoFrage7_3, rdoFrage7_2, rdoFrage7_1);
            int frage8 = GetRating(rdoFrage8_4, rdoFrage8_3, rdoFrage8_2, rdoFrage8_1);
            int frage9 = GetRating(rdoFrage9_4, rdoFrage9_3, rdoFrage9_2, rdoFrage9_1);
            int frage10 = GetRating(rdoFrage10_4, rdoFrage10_3, rdoFrage10_2, rdoFrage10_1);

            string textGut = txtTextGut.Text;
            string textSchlecht = txtTextSchlecht.Text;
            string textAnders = txtTextAnders.Text;

            if (string.IsNullOrWhiteSpace(FormularKey))
            {
                MessageBox.Show("FormularKey ist nicht gesetzt. Bitte melden Sie sich erneut an oder überprüfen Sie Ihre Eingabe.",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                  
                    string query = @"
                        INSERT INTO UnterrichtsBeurteilung 
                        (
                            FormularKey, 
                            Erfassungsdatum,
                            Frage1, Frage2, Frage3, Frage4, Frage5, Frage6, Frage7, Frage8, Frage9, Frage10,
                            TextGut, TextSchlecht, TextAnders,
                            ClassName, Subject, Teacher
                        )
                        VALUES
                        (
                            @FormularKey, 
                            @Erfassungsdatum,
                            @Frage1, @Frage2, @Frage3, @Frage4, @Frage5, @Frage6, @Frage7, @Frage8, @Frage9, @Frage10,
                            @TextGut, @TextSchlecht, @TextAnders,
                            @ClassName, @Subject, @Teacher
                        )";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FormularKey", FormularKey);
                        cmd.Parameters.AddWithValue("@Erfassungsdatum", DateTime.Now);

                        cmd.Parameters.AddWithValue("@Frage1", frage1);
                        cmd.Parameters.AddWithValue("@Frage2", frage2);
                        cmd.Parameters.AddWithValue("@Frage3", frage3);
                        cmd.Parameters.AddWithValue("@Frage4", frage4);
                        cmd.Parameters.AddWithValue("@Frage5", frage5);
                        cmd.Parameters.AddWithValue("@Frage6", frage6);
                        cmd.Parameters.AddWithValue("@Frage7", frage7);
                        cmd.Parameters.AddWithValue("@Frage8", frage8);
                        cmd.Parameters.AddWithValue("@Frage9", frage9);
                        cmd.Parameters.AddWithValue("@Frage10", frage10);

                        cmd.Parameters.AddWithValue("@TextGut", textGut);
                        cmd.Parameters.AddWithValue("@TextSchlecht", textSchlecht);
                        cmd.Parameters.AddWithValue("@TextAnders", textAnders);

                        cmd.Parameters.AddWithValue("@ClassName", ClassName);
                        cmd.Parameters.AddWithValue("@Subject", Subject);
                        cmd.Parameters.AddWithValue("@Teacher", Teacher);
                        cmd.Parameters.AddWithValue("@FormName", FormName);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Formular wurde erfolgreich übermittelt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Formulars: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //
        private int GetRating(params RadioButton[] radioButtons)
        {
            
            int max = radioButtons.Length;
            for (int i = 0; i < max; i++)
            {
                if (radioButtons[i].IsChecked == true)
                {
                    
                    return max - i;
                }
            }
            return 0;
        }
    }
}
