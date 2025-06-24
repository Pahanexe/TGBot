using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGBot.Source
{
    internal class MySQLdb
    {
        private readonly string _connectionString;

        public MySQLdb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<long> SaveImageAsync(string fileId, string filePath)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"
            INSERT INTO Images (FileId, FilePath) 
            VALUES (@fileId, @filePath);
        ";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@fileId", fileId);
            cmd.Parameters.AddWithValue("@filePath", filePath);

            await cmd.ExecuteNonQueryAsync();

            return cmd.LastInsertedId; // Повертаємо Id з таблиці Images
        }
    }
}
