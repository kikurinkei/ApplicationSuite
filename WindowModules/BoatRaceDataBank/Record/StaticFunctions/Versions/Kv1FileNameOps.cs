//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;
//using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models;

//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions
//{
//    /// <summary>
//    /// ファイル名 KYYMMDD.TXT から RDATE/TrackNo/RidBase を導出（TrackNoは本文側と整合させる前提）。
//    /// </summary>
//    public static class Kv1FileNameOps
//    {
//        static readonly Regex Rx = new(@"^[Kk](\d{6})\.txt$", RegexOptions.Compiled);

//        /// <summary>
//        /// ファイル名から最小IDを生成。TrackNoは暫定0（VenueBeginで確定させる場合は上書きする）。
//        /// 不一致時は null を返す（行番号=0でNG扱いにできる）。
//        /// </summary>
//        public static FileIdentity? TryParse(string filePath)
//        {
//            var name = Path.GetFileName(filePath);
//            var m = Rx.Match(name);
//            if (!m.Success) return null;

//            var yyMMdd = m.Groups[1].Value;
//            var yy = int.Parse(yyMMdd[..2]);
//            var mm = int.Parse(yyMMdd.Substring(2, 2));
//            var dd = int.Parse(yyMMdd.Substring(4, 2));
//            var yyyy = 2000 + yy;
//            var rdate = new DateOnly(yyyy, mm, dd);

//            // TrackNo は本文側 KBGN の nn と一致させたいが、ここでは暫定 0。
//            var trackNo = 0;

//            var yyyymmdd = yyyy * 10000 + mm * 100 + dd;
//            long ridBase = (long)yyyymmdd * 100000 + (trackNo * 100);

//            return new FileIdentity(filePath, name, rdate, trackNo, ridBase);
//        }

//        /// <summary>
//        /// VenueBegin の nn（例: "24KBGN"）が来たら TrackNo を確定して RidBase を再計算。
//        /// </summary>
//        public static FileIdentity WithTrackNo(FileIdentity id, int trackNo)
//        {
//            var yyyymmdd = id.RDate.Year * 10000 + id.RDate.Month * 100 + id.RDate.Day;
//            long ridBase = (long)yyyymmdd * 100000 + (trackNo * 100);
//            return id with { TrackNo = trackNo, RidBase = ridBase };
//        }
//    }
//}
