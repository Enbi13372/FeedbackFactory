using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FeedbackFactory
{
   
    public partial class DashboardView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;
        private readonly string _currentTeacherUsername;

        public DashboardView()
        {
            InitializeComponent();

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            _currentTeacherUsername = App.Current.Properties["LoggedInTeacher"] as string;

            if (!string.IsNullOrEmpty(_currentTeacherUsername))
            {
                LoadDashboardData(_currentTeacherUsername);
            }
            else
            {
                MessageBox.Show("Keine Daten gefunden");
            }
        }

       
        private void LoadDashboardData(string teacher)
        {
            try
            {
                using (var conn = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    conn.Open();

                    var cmd = new MySqlCommand(@"
                        SELECT COUNT(*) 
                        FROM Zielscheibe
                        WHERE Teacher = @teacher
                    ", conn);
                    cmd.Parameters.AddWithValue("@teacher", teacher);
                    int countZiel = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd.CommandText = @"
                        SELECT COUNT(*) 
                        FROM UnterrichtsBeurteilung
                        WHERE Teacher = @teacher
                    ";
                    int countUnter = Convert.ToInt32(cmd.ExecuteScalar());

                    int totalForms = countZiel + countUnter;
                    txtTotalForms.Text = totalForms.ToString();

                    
                    cmd.CommandText = @"
                        SELECT AVG(
                            (Frage1 + Frage2 + Frage3 + Frage4 + Frage5 + Frage6 + Frage7 + Frage8) / 8.0
                        )
                        FROM Zielscheibe
                        WHERE Teacher = @teacher
                    ";
                    double avgZiel = 0;
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        avgZiel = Convert.ToDouble(result);

                    cmd.CommandText = @"
                        SELECT AVG(
                            (Frage1 + Frage2 + Frage3 + Frage4 + Frage5 + Frage6 + Frage7 + Frage8 + Frage9 + Frage10) / 10.0
                        )
                        FROM UnterrichtsBeurteilung
                        WHERE Teacher = @teacher
                    ";
                    double avgUnter = 0;
                    result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        avgUnter = Convert.ToDouble(result);

                    double weightedAvg = 0;
                    if ((countZiel + countUnter) > 0)
                    {
                        weightedAvg = (avgZiel * countZiel + avgUnter * countUnter)
                                      / (countZiel + countUnter);
                    }

                    if (weightedAvg < 0) weightedAvg = 0;
                    if (weightedAvg > 5) weightedAvg = 5;

                    UpdateStarRating(stackAverageRatingStars, weightedAvg);

                    double target = 100.0;
                    double progress = (target > 0) ? (totalForms / target) * 100.0 : 0;
                    if (progress > 100) progress = 100; 
                    txtProgress.Text = progress.ToString("0") + "%";

                    txtForm1SubmittedCount.Text = countZiel.ToString();
                    UpdateStarRating(stackForm1Stars, avgZiel);

                    txtForm2SubmittedCount.Text = countUnter.ToString();
                    UpdateStarRating(stackForm2Stars, avgUnter);

                    txtForm3SubmittedCount.Text = "0";
                    UpdateStarRating(stackForm3Stars, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Dashboard-Daten: " + ex.Message,
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //
        private void UpdateStarRating(StackPanel starPanel, double rating)
        {
            starPanel.Children.Clear();

            int fullStars = (int)Math.Floor(rating);
            double fractional = rating - fullStars; 

            for (int i = 0; i < fullStars; i++)
                starPanel.Children.Add(CreateStarTextBlock("★", Brushes.Gold));

            if (fractional >= 0.5)
                starPanel.Children.Add(CreateStarTextBlock("★", Brushes.Gold));

            while (starPanel.Children.Count < 5)
                starPanel.Children.Add(CreateStarTextBlock("☆", Brushes.Gray));
        }

        private TextBlock CreateStarTextBlock(string text, Brush color)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = 28,
                Margin = new Thickness(2)
            };
        }
    }
}
