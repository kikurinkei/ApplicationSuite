using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.BoatRaceDataBank.Record.RecordUnits;
using System.Linq;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Kv1
{
    /// <summary>
    /// Kv1Session の実行結果を VM が扱う KRecordUnit に“まとめるだけ”の薄いビルダー。
    /// I/O なし。余計な判定・ログなし。
    /// </summary>
    public static class Kv1RecordUnitBuilder
    {
        public static KRecordUnit Build(string filePath, string status, Kv1Session session)
        {
            return new KRecordUnit
            {
                FilePath = filePath,
                Status = status,

                // 変更前
                // RDate = session.RDate,
                // TrackNo = session.TrackNo,
                //// Rows は v1 の最小列（RaceNo, RegNo1..6）をそのまま受け渡し
                // Rows = session.Rows.ToArray(),

                // 変更後（コンパイル回避のダミー）
                RDate = default,
                TrackNo = 0,
                Rows = Array.Empty<KRaceRowV1>(),


            };
        }
    }
}