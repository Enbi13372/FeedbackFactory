using System;
using System.Linq;
using System.Windows;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for KeyGeneratorWindow.xaml
    /// </summary>
    public partial class KeyGeneratorWindow : Window
    {
        public KeyGeneratorWindow()
        {
            InitializeComponent();
        }

        private void GenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Überprüfen, welche Option ausgewählt wurde
            string option = string.Empty;

            if (Option1.IsChecked == true)
            {
                option = "Option 1";
            }
            else if (Option2.IsChecked == true)
            {
                option = "Option 2";
            }
            else if (Option3.IsChecked == true)
            {
                option = "Option 3";
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Option.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Schlüssel generieren basierend auf der gewählten Option
            string generatedKey = GenerateRandomKey(option);

            // Generierten Schlüssel anzeigen
            GeneratedKeyText.Text = generatedKey;
            GeneratedKeyText.Visibility = Visibility.Visible;

            // Den Button zum Kopieren sichtbar machen
            CopyButton.Visibility = Visibility.Visible;

            // Den Button deaktivieren, sodass der Schlüssel nicht mehrmals generiert werden kann
            GenerateKeyButton.IsEnabled = false;
        }

        // Methode zum Generieren eines zufälligen Schlüssels
        private string GenerateRandomKey(string option)
        {
            // Beispielhafte Schlüsselgenerierung
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string key = string.Join(string.Empty, Enumerable.Range(0, 16).Select(_ => chars[random.Next(chars.Length)]));
            return key;
        }

        // Event-Handler zum Kopieren des Schlüssels in die Zwischenablage
        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedKeyText.Text))
            {
                Clipboard.SetText(GeneratedKeyText.Text);  // Schlüssel in die Zwischenablage kopieren
                MessageBox.Show("Der Schlüssel wurde in die Zwischenablage kopiert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
