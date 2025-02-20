using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace FeedbackFactory
{
    public partial class Zielscheibe : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public string FormularKey { get; set; }
        public string FormularClassName { get; set; }
        public string FormularSubject { get; set; }
        public string FormularTeacher { get; set; }

        public Zielscheibe(string key, string className, string subject, string teacher)
        {
            InitializeComponent();

            FormularKey = key;
            FormularClassName = className;
            FormularSubject = subject;
            FormularTeacher = teacher;

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        public Zielscheibe() : this("TestFormularKey_001", "TestKlasse", "TestSubject", "TestTeacher")
        {
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            int frage1 = GetRating(rdoQ1_5, rdoQ1_4, rdoQ1_3, rdoQ1_2, rdoQ1_1);
            int frage2 = GetRating(rdoQ2_5, rdoQ2_4, rdoQ2_3, rdoQ2_2, rdoQ2_1);
            int frage3 = GetRating(rdoQ3_5, rdoQ3_4, rdoQ3_3, rdoQ3_2, rdoQ3_1);
            int frage4 = GetRating(rdoQ4_5, rdoQ4_4, rdoQ4_3, rdoQ4_2, rdoQ4_1);
            int frage5 = GetRating(rdoQ5_5, rdoQ5_4, rdoQ5_3, rdoQ5_2, rdoQ5_1);
            int frage6 = GetRating(rdoQ6_5, rdoQ6_4, rdoQ6_3, rdoQ6_2, rdoQ6_1);
            int frage7 = GetRating(rdoQ7_5, rdoQ7_4, rdoQ7_3, rdoQ7_2, rdoQ7_1);
            int frage8 = GetRating(rdoQ8_5, rdoQ8_4, rdoQ8_3, rdoQ8_2, rdoQ8_1);

            string textRichtig = txtTextRichtig.Text;
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

                    string query = @"INSERT INTO Zielscheibe 
                                     (FormularKey, Erfassungsdatum, Frage1, Frage2, Frage3, Frage4, Frage5, Frage6, Frage7, Frage8, TextRichtig, TextAnders, ClassName, Subject, Teacher)
                                     VALUES (@FormularKey, @Erfassungsdatum, @Frage1, @Frage2, @Frage3, @Frage4, @Frage5, @Frage6, @Frage7, @Frage8, @TextRichtig, @TextAnders, @ClassName, @Subject, @Teacher)";
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
                        cmd.Parameters.AddWithValue("@TextRichtig", textRichtig);
                        cmd.Parameters.AddWithValue("@TextAnders", textAnders);
                        cmd.Parameters.AddWithValue("@ClassName", FormularClassName);
                        cmd.Parameters.AddWithValue("@Subject", FormularSubject);
                        cmd.Parameters.AddWithValue("@Teacher", FormularTeacher);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Formular wurde erfolgreich übermittelt!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des Formulars: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int GetRating(params RadioButton[] radioButtons)
        {
            for (int i = 0; i < radioButtons.Length; i++)
            {
                if (radioButtons[i].IsChecked == true)
                {
                    return radioButtons.Length - i;
                }
            }
            return 0;
        }
    }
}
