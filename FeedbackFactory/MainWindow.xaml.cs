using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FeedbackFactory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _username;
        private readonly int _role; // Store the role

        public MainWindow(string username, int role)
        {
            InitializeComponent();

            _username = username;
            _role = role;

            MainContent.Content = new DashboardView();
            TxtWelcome.Text = $"Willkommen, {_username}";

            // Adjust UI based on the user's role
            ConfigureUIBasedOnRole();

            // �berwache die Content-Eigenschaft des ContentControls
            DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl))
                .AddValueChanged(MainContent, MainContent_ContentChanged);
        }

        // Adjust UI based on role
        private void ConfigureUIBasedOnRole()
        {
            if (_role == 1)
            {
                BtnAdmin.Visibility = Visibility.Visible;            }
            else
            {
                BtnAdmin.Visibility = Visibility.Collapsed;
            }
        }

        // Event-Handler for changes in the Content property
        private void MainContent_ContentChanged(object sender, EventArgs e)
        {
            AktualisiereButtonSichtbarkeit();
        }

        // Updates the visibility of the "Formular erstellen" button
        private void AktualisiereButtonSichtbarkeit()
        {
            if (MainContent.Content is FormularView)
            {
                BtnFormularErstellen.Visibility = Visibility.Visible;
            }
            else
            {
                BtnFormularErstellen.Visibility = Visibility.Collapsed;
            }
        }

        // Event-Handler for button clicks
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView();
        }

        private void BtnFormulare_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new FormularView();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView(_username);
        }

        private void BtnAdminView_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AdminView();
        }

        private void BtnFormularErstellen_Click(object sender, RoutedEventArgs e)
        {
            // Pass the username to the KeyGeneratorWindow
            KeyGeneratorWindow keyGeneratorWindow = new KeyGeneratorWindow(_username);
            keyGeneratorWindow.ShowDialog(); // ShowDialog opens it as a modal window
        }

        private void BtnClasses_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClassView();
        }
    }
}
