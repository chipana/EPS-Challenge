using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;

namespace EPS.Challenge.Repositories
{
    public class DiscountRepository
    {
        private const string DbPath = "Data Source=/app/Data/discount.db";
        private static readonly ConcurrentDictionary<string, bool> ActiveCodes = new();

        public DiscountRepository()
        {
            InitializeDatabase();
            LoadCodesIntoMemory();
        }
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS DiscountCodes (
                Code TEXT PRIMARY KEY,
                IsUsed INTEGER DEFAULT 0
            );";
            command.ExecuteNonQuery();
        }

        private void LoadCodesIntoMemory()
        {
            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Code, IsUsed FROM DiscountCodes;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ActiveCodes[reader.GetString(0)] = reader.GetInt32(1) == 1;
            }
        }

        public bool SaveCodeToDatabase(string code)
        {
            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO DiscountCodes (Code) VALUES (@Code);";
            command.Parameters.AddWithValue("@Code", code);
            return command.ExecuteNonQuery() > 0;
        }
        public bool MarkCodeAsUsedInDatabase(string code)
        {
            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE DiscountCodes SET IsUsed = 1 WHERE Code = @Code;";
            command.Parameters.AddWithValue("@Code", code);
            return command.ExecuteNonQuery() > 0;
        }

        public bool GetActiveCode(string code, out bool isUsed) => ActiveCodes.TryGetValue(code, out isUsed);
        public bool AddActiveCode(string code) => ActiveCodes.TryAdd(code, false);
        public void SetCodeAsUsed(string code) => ActiveCodes[code] = true;
    }
}
