﻿using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FeedbackFactory
{
    public partial class RegisterView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public RegisterView()
        {
            InitializeComponent();

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);
        }

        private void RegisterView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterBTN_Click(RegisterBTN, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void RegisterView_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTB.Focus();
        }

        private void BackBTN_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new TeacherView();
            }
        }

        private void RegisterBTN_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordTB.Password;
            string confirmPassword = ConfirmPasswordTB.Password;
            string username = UsernameTB.Text;
            string registrationKey = RegistrationKeyTB.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwörter stimmen nicht überein. Bitte versuchen Sie es erneut.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(registrationKey))
            {
                MessageBox.Show("Alle Felder müssen ausgefüllt werden. Bitte versuchen Sie es erneut.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string keyQuery = "SELECT Expiration FROM Registrationkeys WHERE `Key` = @Key";
            var keyParameter = new MySqlParameter("@Key", registrationKey);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();

                    using (MySqlCommand cmd = new MySqlCommand(keyQuery, connection))
                    {
                        cmd.Parameters.Add(keyParameter);
                        var expirationDateObj = cmd.ExecuteScalar();

                        if (expirationDateObj == null)
                        {
                            MessageBox.Show("Schlüssel ungültig, bitte kontaktieren Sie den Systemadministrator.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        DateTime expirationDate = Convert.ToDateTime(expirationDateObj);
                        if ((DateTime.Now - expirationDate).TotalDays > 7)
                        {
                            MessageBox.Show("Schlüssel abgelaufen, bitte kontaktieren Sie den Systemadministrator.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username;";
                    var checkParameters = new MySqlParameter[]
                    {
                        new MySqlParameter("@Username", username)
                    };

                    using (MySqlCommand cmd = new MySqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddRange(checkParameters);
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            MessageBox.Show("Dieser Nutzer existiert bereits.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, 0);";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Username", username),
                    new MySqlParameter("@Password", hashedPassword)
                };

                bool success = _dbHandler.ExecuteNonQuery(query, parameters);

                if (success)
                {
                    MessageBox.Show("Benutzer erfolgreich registriert!");

                    var parentWindow = Window.GetWindow(this) as LoginWindow;
                    if (parentWindow != null)
                    {
                        parentWindow.MainContent.Content = new TeacherView();
                    }
                }
                else
                {
                    MessageBox.Show("Benutzer konnte nicht registriert werden.", "Registrierung Fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
        }

        private void RegisterLBL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as LoginWindow;
            if (parentWindow != null)
            {
                parentWindow.MainContent.Content = new RegisterView();
            }
        }
    }
}