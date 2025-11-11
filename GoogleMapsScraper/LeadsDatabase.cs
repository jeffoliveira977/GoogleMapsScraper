using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Dapper;

namespace GoogleMapsScraper
{
    class LeadsDatabase
    {
        private readonly string _connectionString;

        public LeadsDatabase(string dbName = "data\\leads.db")
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            var fullPath = System.IO.Path.Combine(curDir, dbName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            _connectionString = $"Data Source={fullPath}";
            CreateTableIfNeeded();
        }

        private SqliteConnection Connect()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private void CreateTableIfNeeded()
        {
            try
            {
                using var conn = Connect();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS leads (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    searchId TEXT,
                    name TEXT,
                    categories TEXT,
                    email TEXT,
                    phone TEXT,
                    url TEXT,
                    domain TEXT,
                    facebook TEXT,
                    instagram TEXT,
                    linkedin TEXT,
                    twitter TEXT,
                    youtube TEXT,
                    tiktok TEXT,
                    cnpj TEXT,
                    fulladdr TEXT,
                    local_name TEXT,
                    local_fulladdr TEXT,
                    created_at TEXT,
                    processed INTEGER,
                    key TEXT
                );
            ";

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro ao criar tabela: {ex.Message}");
            }
        }

        public async Task SaveRecordsAsync(List<BusinessRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                Console.WriteLine("Nenhum registro para salvar.");
                return;
            }

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync();

            var query = @"
                INSERT OR IGNORE INTO leads (
                    searchId, name, categories, email, phone, url, domain, facebook, instagram, linkedin, 
                    twitter, youtube, tiktok, cnpj, fulladdr, local_name, local_fulladdr, 
                    created_at, processed, key
                ) VALUES (
                    @searchId, @name, @categories, @email, @phone, @url, @domain, @facebook, @instagram, @linkedin, 
                    @twitter, @youtube, @tiktok, @cnpj, @fulladdr, @local_name, @local_fulladdr, 
                    @created_at, @processed, @key
                );
            ";

            await using var cmd = new SqliteCommand(query, conn, (SqliteTransaction?)tx);
            foreach (var r in records)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@searchId", r.SearchId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@name", r.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@categories", r.Categories ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@email", r.Email ?? "");
                cmd.Parameters.AddWithValue("@phone", r.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@url", r.Url ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@domain", r.Domain ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@facebook", r.Facebook ?? "");
                cmd.Parameters.AddWithValue("@instagram", r.Instagram ?? "");
                cmd.Parameters.AddWithValue("@linkedin", r.Linkedin ?? "");
                cmd.Parameters.AddWithValue("@twitter", r.Twitter ?? "");
                cmd.Parameters.AddWithValue("@youtube", r.Youtube ?? "");
                cmd.Parameters.AddWithValue("@tiktok", r.Tiktok ?? "");
                cmd.Parameters.AddWithValue("@cnpj", r.Cnpj ?? "");
                cmd.Parameters.AddWithValue("@fulladdr", r.FullAddr ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@local_name", r.LocalName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@local_fulladdr", r.LocalFullAddr ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@created_at", r.CreatedAt ?? DateTime.UtcNow.ToString("s"));
                cmd.Parameters.AddWithValue("@processed", r.Processed ? 1 : 0);
                cmd.Parameters.AddWithValue("@key", r.Key?.ToString() ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            Console.WriteLine($"{records.Count} registros salvos com sucesso no banco de dados.");
        }

        public List<BusinessRecord> GetLeadsBySearchId(string searchId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            const string sql = "SELECT * FROM leads WHERE searchId = @searchId";

            var leads = conn.Query<BusinessRecord>(sql, new { searchId }).ToList();

            return leads;
        }
    }
}
