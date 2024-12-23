using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;

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
                return config?.ConnectionString ?? throw new Exception("Connection string not found in configuration file.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read the configuration file: {ex.Message}", ex);
            }
        }

        // Execute a non-query (INSERT, UPDATE, DELETE)
        public bool ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
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
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        // Helper class to deserialize the JSON configuration
        private class DatabaseConfig
        {
            public string ConnectionString { get; set; }
        }
    }
}
