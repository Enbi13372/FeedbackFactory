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
using System.Collections.ObjectModel;
using System.Globalization;


namespace FeedbackFactory
{
    public partial class AdminView : UserControl
    {
        private readonly DBConnectionHandler _dbHandler;

        public ICommand DeleteUserCommand { get; }
        public ICommand ChangeRoleCommand { get; }


        public ObservableCollection<Benutzer> BenutzerListe { get; set; } = new ObservableCollection<Benutzer>();

        public AdminView()
        {
            InitializeComponent();

            DeleteUserCommand = new RelayCommand<Benutzer>(DeleteUser);
            ChangeRoleCommand = new RelayCommand<Benutzer>(ChangeRole);

            this.DataContext = this;


            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
            _dbHandler = new DBConnectionHandler(configPath);

            
            this.DataContext = this;

           
            LadeBenutzerAusDatenbank();
        }

        private void DeleteUser(Benutzer benutzer)
        {
            if (benutzer == null) return;

            string query = "DELETE FROM Users WHERE Username = @Username";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", benutzer.Name);
                        cmd.ExecuteNonQuery();
                    }
                }

                BenutzerListe.Remove(benutzer);

                MessageBox.Show("Benutzer erfolgreich gelöscht.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen des Benutzers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeRole(Benutzer benutzer)
        {
            if (benutzer == null) return;

            benutzer.Rolle = benutzer.Rolle == 0 ? 1 : 0;

            string query = "UPDATE Users SET Role = @Role WHERE Username = @Username";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Role", benutzer.Rolle);
                        cmd.Parameters.AddWithValue("@Username", benutzer.Name);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Rolle erfolgreich geändert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ändern der Rolle: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LadeBenutzerAusDatenbank()
        {
            string query = "SELECT Username,  Role FROM Users;";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                BenutzerListe.Add(new Benutzer
                                {
                                    
                                    Name = reader.GetString("Username"),
                                    Rolle = reader.GetInt32("Role")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Benutzer: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;
            var selectedRole = RoleInput.SelectedItem as ComboBoxItem;

            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || selectedRole == null)
            {
                MessageBox.Show("Bitte geben Sie einen Benutzernamen ein Passwort und eine Rolle ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int role = int.Parse(selectedRole.Tag.ToString());
            
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            string query = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role);";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbHandler.ConnectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Role", role);

                        cmd.ExecuteNonQuery();
                    }
                }

                BenutzerListe.Add(new Benutzer
                {
                    Name = username,
                    Rolle = role
                });

                MessageBox.Show("Benutzer erfolgreich hinzugefügt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);


                UsernameInput.Text = string.Empty;
                PasswordInput.Password = string.Empty;
                RoleInput.SelectedIndex = -1;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Benutzers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


   
    public class Benutzer
    {
        
        public string Name { get; set; }
        public int Rolle { get; set; }    
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    


}
