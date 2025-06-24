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

        public async Task AddTagsAsync(long imageId, IEnumerable<string> tags)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var tag in tags)
            {
                long tagId;

                // 1. Додати тег, якщо такого ще немає
                const string insertTagQuery = @"
            INSERT INTO Tags (Name)
            VALUES (@name)
            ON DUPLICATE KEY UPDATE Id = LAST_INSERT_ID(Id);";

                using (var insertTagCmd = new MySqlCommand(insertTagQuery, connection))
                {
                    insertTagCmd.Parameters.AddWithValue("@name", tag);
                    await insertTagCmd.ExecuteNonQueryAsync();
                    tagId = insertTagCmd.LastInsertedId;
                }

                // 2. Додати зв'язок між зображенням і тегом
                const string insertImageTagQuery = @"
            INSERT IGNORE INTO ImageTags (ImageId, TagId)
            VALUES (@imageId, @tagId);";

                using (var insertImageTagCmd = new MySqlCommand(insertImageTagQuery, connection))
                {
                    insertImageTagCmd.Parameters.AddWithValue("@imageId", imageId);
                    insertImageTagCmd.Parameters.AddWithValue("@tagId", tagId);
                    await insertImageTagCmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
