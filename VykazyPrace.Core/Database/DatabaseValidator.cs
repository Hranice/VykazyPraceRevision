using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database
{
    public static class DatabaseValidator
    {
        private static readonly string[] RequiredTables =
        {
            "Users",
            "Projects",
            "TimeEntries",
            "TimeEntrySubTypes",
            "TimeEntryTypes",
            "UserGroups"
        };

        public static void ValidateStructure(VykazyPraceContext context)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            var foundTables = new HashSet<string>();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                foundTables.Add(reader.GetString(0));
            }

            foreach (var table in RequiredTables)
            {
                if (!foundTables.Contains(table))
                {
                    throw new InvalidOperationException($"Chyb� tabulka '{table}' v datab�zi.");
                }
            }

            EnsureUserArchiveColumn(connection);

            if (!context.Users.Any())
            {
                throw new InvalidOperationException("Datab�ze neobsahuje ��dn�ho u�ivatele.");
            }
        }
        private static void EnsureUserArchiveColumn(System.Data.Common.DbConnection connection)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(Users);";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(1));
                }
            }

            if (columns.Contains("IsArchived"))
                return;

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "ALTER TABLE Users ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
