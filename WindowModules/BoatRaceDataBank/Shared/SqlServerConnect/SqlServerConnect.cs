//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Shared.SqlServerConnect
//{
//    public class SqlServerConnect
//    {
//        public static void sqlServerConnect()
//        {
//            // ★ 接続文字列（※自分の環境に合わせて変更してください）
//            string connectionString =
//                "Data Source=Z390-PRO4\\SQLEXPRESS;" +
//                "Initial Catalog=BR;" +
//                "User ID=sa;" +
//                "Password=19860314;" +
//                "TrustServerCertificate=True;";

//            // ★ 実行する SELECT クエリ（サンプルとして SomeTable の最初の 10 行を取得）
//            string query = "SELECT TOP 10 * FROM [BR].[dbo].[RP];";

//            /*Z390_MAIN
//             string connectionString =
//            "Data Source=localhost;" +
//            "Initial Catalog=BR;" +
//            "User ID=sa;" +
//            "Password=.....;" +
//            "TrustServerCertificate=True;";
//             * 
//            SELECT TOP (1000) [RaceDate]
//             ,[VenueCode]
//            ,[RaceNumber]
//            ,[Distance]
//            FROM [BRDB].[dbo].[RP]
//            */

//            // ★ SqlConnection を using ブロックで生成・管理する（接続終了時に自動的にクローズされる）
//            SqlConnection connection = new SqlConnection(connectionString);

//            // ★ SqlCommand オブジェクトにクエリと接続情報を渡す
//            SqlCommand command = new SqlCommand(query, connection);

//            // ★ SQL Server への接続をオープン
//            connection.Open();

//            // ★ クエリを実行して結果を SqlDataReader オブジェクトで受け取る
//            SqlDataReader reader = command.ExecuteReader();

//            // ★ 読み取った結果を 1 行ずつループ
//            while (reader.Read())
//            {
//                // ★ ここでは例として、1列目の値を文字列にして出力
//                Console.WriteLine(reader[0].ToString());
//            }
//;
//            // ★ 例外が発生した場合はエラーメッセージを出力
//            // Console.WriteLine("接続またはクエリ実行エラー: " + ex.Message ;


//            // ★ プログラム終了前にキー入力を待つ（Visual Studio のデバッグ時に結果を確認するため）
//            Console.WriteLine("処理終了。Enterキーを押してください...");
//            Console.ReadLine();
//        }
//    }
//}