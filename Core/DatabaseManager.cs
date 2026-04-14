using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SecurIT_Memory.Models;

namespace SecurIT_Memory.Core
{
    public class DatabaseManager
    {
        private string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MemorySecurIT;Integrated Security=True";

        public void CreateTableIfNotExists()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Scores' AND xtype='U') " +
                               "CREATE TABLE Scores (Id INT PRIMARY KEY IDENTITY, PlayerName NVARCHAR(50), Time INT, Attempts INT)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveScore(Score newScore)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Scores (PlayerName, Time, Attempts) VALUES (@Name, @Time, @Attempts)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", newScore.PlayerName);
                    command.Parameters.AddWithValue("@Time", newScore.Time.TotalSeconds);
                    command.Parameters.AddWithValue("@Attempts", newScore.Attempts);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Score> GetTopScores()
        {
            List<Score> topScores = new List<Score>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT TOP 10 PlayerName, Time, Attempts FROM Scores ORDER BY Time ASC, Attempts ASC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            topScores.Add(new Score
                            {
                                PlayerName = reader["PlayerName"].ToString(),
                                Time = TimeSpan.FromSeconds(Convert.ToInt32(reader["Time"])),
                                Attempts = Convert.ToInt32(reader["Attempts"])
                            });
                        }
                    }
                }
            }
            return topScores;
        }
    }
}