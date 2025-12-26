using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;



namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Store.Repositories
{
    /// <summary>
    /// RP（番組表）テーブルの読み取り専用ミニマム実装：
    /// 最終 RaceDate を 1件だけ返す（空なら null）。
    /// </summary>
    public sealed class RaceProgramRepository
    {
        // 接続方針：
        // - 既に別チャットで接続テスト済み（SqlClient）
        // - ここでは最小限として Windows認証を既定、必要に応じて環境変数で上書き
        //   BRDB_CONN があればそれを使用、無ければ Trusted_Connection=True を使う
        private static string ResolveConnectionString()
        {
            var env = Environment.GetEnvironmentVariable("BRDB_CONN");
            if (!string.IsNullOrWhiteSpace(env)) return env;

            // 既定：ローカル開発のWindows認証
            return "Server=localhost;Database=BRDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public DateOnly? GetLastRaceDate()
        {
            const string sql = @"SELECT MAX([RaceDate]) FROM [BRDB].[dbo].[RP];";

            using var conn = new SqlConnection(ResolveConnectionString());
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            var scalar = cmd.ExecuteScalar();

            if (scalar == DBNull.Value || scalar is null) return null;

            // SQL Server の date → .NET の DateTime で返るので DateOnly へ変換
            var dt = Convert.ToDateTime(scalar);
            return DateOnly.FromDateTime(dt);
        }
    }
}
