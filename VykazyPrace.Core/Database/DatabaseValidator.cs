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
                    throw new InvalidOperationException($"Chybí tabulka '{table}' v databázi.");
                }
            }

            if (!context.Users.Any())
            {
                throw new InvalidOperationException("Databáze neobsahuje žádného uživatele.");
            }
        }
    }
}
