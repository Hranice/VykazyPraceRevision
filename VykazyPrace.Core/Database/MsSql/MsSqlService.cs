using Microsoft.Data.SqlClient;
using System.Data;

namespace VykazyPrace.Core.Database.MsSql
{
    public class MsSqlService
    {
        private readonly string _connectionString;

        public MsSqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            using var adapter = new SqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void ExecuteNonQuery(string sql)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void ExecuteNonQueryMultiple(params string[] sqlCommands)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            foreach (var sql in sqlCommands)
            {
                using var command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
