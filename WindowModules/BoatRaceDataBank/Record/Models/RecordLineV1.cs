//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models
//{
//    /// <summary>
//    /// A型（1R=1行TSV）の最小列（v1）。
//    /// </summary>
//    public readonly record struct RecordLineV1(
//        long RID,            // RidBase + RaceNo
//        DateOnly RDATE,      // レース日
//        int TrackNo,         // 場番号
//        int RaceNo           // レース番号
//    )
//    {
//        public string[] ToTsvRow() => new[]
//        {
//            RID.ToString(),
//            RDATE.ToString("yyyy-MM-dd"),
//            TrackNo.ToString("00"),
//            RaceNo.ToString("00"),
//        };

//        public static readonly string[] Header = { "RID", "RDATE", "TrackNo", "RaceNo" };
//    }
//}