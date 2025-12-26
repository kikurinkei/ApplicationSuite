using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Shared.SqlServerConnect
{
    // Program.cs
    // 目的：SQL Server (Developer/既定インスタンス) に接続して
    //       BRDB.dbo.RP から TOP 1000 を読み、Console に出力するテスト。
    // 事前準備：dotnet add package Microsoft.Data.SqlClient



    public static class Program
    {
        // エントリポイント（async）
        public static async Task<int> Main()
        {
            // ★接続文字列
            // 既定インスタンスなら "localhost" or "." が使えます。
            // DB名はクエリに合わせて BRDB にしています（以前は BR になっていたので要注意）。
            // パスワードは実値に置き換えてください。
            var connectionString =
                "Server=localhost;" +
                "Database=BRDB;" +
                "User ID=sa;" +
                "Password=sa1XXXXXXX4;" +
                "TrustServerCertificate=True;"; // 開発環境向け。運用では証明書要検討。

            // ★テスト用SQL
            // 並びを固定すると検証しやすいので軽く ORDER BY を添えています（不要なら外してください）。
            const string sql = @"
SELECT TOP (1000)
    [RaceDate],
    [VenueCode],
    [RaceNumber],
    [Distance]
FROM [BRDB].[dbo].[RP]
ORDER BY [RaceDate], [VenueCode], [RaceNumber];";

            try
            {
                // 接続
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                Console.WriteLine("== Connected ==");
                Console.WriteLine($"DataSource: {conn.DataSource}");
                Console.WriteLine($"Database  : {conn.Database}");
                Console.WriteLine();

                // コマンド実行
                using var cmd = new SqlCommand(sql, conn);

                // リーダーで読み取り（前方オンリー/読み取り専用）
                using var reader = await cmd.ExecuteReaderAsync();

                int row = 0;

                // ★カラムの型が不明でも落ちにくいよう、ToString() ベースで安全に取得
                while (await reader.ReadAsync())
                {
                    string raceDate = reader["RaceDate"]?.ToString() ?? "";
                    string venueCode = reader["VenueCode"]?.ToString() ?? "";
                    string raceNumber = reader["RaceNumber"]?.ToString() ?? "";
                    string distance = reader["Distance"]?.ToString() ?? "";

                    // 見やすさのために行番号を付与
                    Console.WriteLine($"{++row:0000}: {raceDate}\t{venueCode}\t{raceNumber}\t{distance}");
                }

                Console.WriteLine();
                Console.WriteLine($"== Rows read: {row}");

                return 0; // 正常終了
            }
            catch (Exception ex)
            {
                // 例外は標準エラーに
                Console.Error.WriteLine("== DB test failed ==");
                Console.Error.WriteLine(ex.Message);
                return 1; // 異常終了
            }
        }
    }
}
