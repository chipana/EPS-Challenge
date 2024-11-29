using System.Collections.Concurrent;
using EPS.Challenge.Model;
using EPS.Challenge.Repositories.Interfaces;
using Microsoft.Data.Sqlite;

namespace EPS.Challenge.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private const string DbPath = "Data Source=/app/Data/discount.db";
        private static readonly ConcurrentDictionary<string, DiscountCode> ActiveCodes = new();

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
                ActiveCodes[reader.GetString(0)] =  new DiscountCode { Code = reader.GetString(0), IsUsed = reader.GetInt32(1) == 1 };
            }
        }

        public bool SaveCodeToDatabase(string code)
        {
            ActiveCodes.TryAdd(code, new DiscountCode { Code = code, IsUsed = false});

            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO DiscountCodes (Code) VALUES (@Code);";
            command.Parameters.AddWithValue("@Code", code);
            return command.ExecuteNonQuery() > 0;
        }
        public bool MarkCodeAsUsed(string code)
        {
            ActiveCodes[code].IsUsed = true;

            using var connection = new SqliteConnection(DbPath);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE DiscountCodes SET IsUsed = 1 WHERE Code = @Code;";
            command.Parameters.AddWithValue("@Code", code);
            return command.ExecuteNonQuery() > 0;
        }

        public DiscountCode? GetActiveCode(string code) => ActiveCodes.GetValueOrDefault(code);
    }
}
