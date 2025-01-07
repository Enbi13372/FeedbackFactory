using System;
using System.IO;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace FeedbackFactory
{
    public class DBConnectionHandler
    {
        private readonly string _connectionString;

        // Constructor that loads the connection string from a config file
        public DBConnectionHandler(string configFilePath)
        {
            _connectionString = LoadConnectionString(configFilePath);
        }

        // Public property to access the connection string
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        // Load connection string from JSON file
        private string LoadConnectionString(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<DatabaseConfig>(json);
                return config?.ConnectionString ?? throw new Exception("ConnectionString wurde nicht in der config Datei gefunden.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Lesen der Config Datei: {ex.Message}", ex);
            }
        }

        // Execute a non-query (INSERT, UPDATE, DELETE)
        public bool ExecuteNonQuery(string query, MySqlParameter[] parameters)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Datenbankvorgang fehlgeschlagen: {ex.Message}", ex);
            }
        }

        // Helper class to deserialize the JSON configuration
        private class DatabaseConfig
        {
            public string ConnectionString { get; set; }
        }
    }
}
