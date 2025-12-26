using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Data.SqlClient;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Store.Repositories
{
    /// <summary>
    /// RR（レース結果）テーブルの読み取り専用ミニマム実装：
    /// 最終 RaceDate を 1件だけ返す（空なら null）。
    /// </summary>
    public sealed class RaceResultRepository
    {
        private static string ResolveConnectionString()
        {
            var env = Environment.GetEnvironmentVariable("BRDB_CONN");
            if (!string.IsNullOrWhiteSpace(env)) return env;

            // Windows認証（開発既定）
            return "Server=localhost;Database=BRDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public DateOnly? GetLastRaceDate()
        {
            const string sql = @"SELECT MAX([RaceDate]) FROM [BRDB].[dbo].[RR];";

            using var conn = new SqlConnection(ResolveConnectionString());
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            var scalar = cmd.ExecuteScalar();

            if (scalar == DBNull.Value || scalar is null) return null;

            var dt = Convert.ToDateTime(scalar);
            return DateOnly.FromDateTime(dt);
        }
    }
}
