using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Helpers
{
    public static class SQLLiteHelper
    {
        public static int ExecuteNonQuery(this SQLiteConnection connection, string text)
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            var cmd = connection.CreateCommand();
            cmd.CommandText = text;
            return cmd.ExecuteNonQuery();
        }
    }
}
