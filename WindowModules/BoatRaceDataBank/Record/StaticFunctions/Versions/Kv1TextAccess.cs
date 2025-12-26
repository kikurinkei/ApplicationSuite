//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions
//{
//    /// <summary>
//    /// TXT読み込み（SJIS優先→UTF8フォールバック）＋簡易アクセサ。
//    /// ※SJIS: 932
//    /// </summary>
//    public static class Kv1TextAccess
//    {
//        public static string[] ReadAllLinesSjisOrUtf8(string filePath)
//        {
//            try
//            {
//                var sjis = Encoding.GetEncoding(932);
//                return File.ReadAllLines(filePath, sjis);
//            }
//            catch
//            {
//                return File.ReadAllLines(filePath, new UTF8Encoding(false));
//            }
//        }

//        /// <summary>
//        /// VenueBegin 行例: "24KBGN" を検出して nn を返す（最初の1件）。
//        /// </summary>
//        public static bool TryFindTrackNo(string[] lines, out int trackNo, out int lineIndex)
//        {
//            trackNo = 0; lineIndex = -1;
//            for (int i = 0; i < lines.Length; i++)
//            {
//                var s = lines[i];
//                // 雑だが軽い：先頭2桁+KBGN を見る
//                if (s?.Length >= 6 && char.IsDigit(s[0]) && char.IsDigit(s[1]) && s.Contains("KBGN"))
//                {
//                    if (int.TryParse(s[..2], out var nn) && nn is >= 1 and <= 99)
//                    {
//                        trackNo = nn; lineIndex = i; return true;
//                    }
//                }
//            }
//            return false;
//        }
//    }
//}


//namespace ApplicationSuite.BoatRaceDataBank.Record.StaticFunctions.Kv1
//{
//    /// <summary>
//    /// TXT読み込み（SJIS優先→UTF8フォールバック）＋簡易アクセサ。
//    /// ※SJIS: 932
//    /// </summary>
//    public static class Kv1TextAccess
//    {
//        public static string[] ReadAllLinesSjisOrUtf8(string filePath)
//        {
//            try
//            {
//                var sjis = Encoding.GetEncoding(932);
//                return File.ReadAllLines(filePath, sjis);
//            }
//            catch
//            {
//                return File.ReadAllLines(filePath, new UTF8Encoding(false));
//            }
//        }

//        /// <summary>
//        /// VenueBegin 行例: "24KBGN" を検出して nn を返す（最初の1件）。
//        /// </summary>
//        public static bool TryFindTrackNo(string[] lines, out int trackNo, out int lineIndex)
//        {
//            trackNo = 0; lineIndex = -1;
//            for (int i = 0; i < lines.Length; i++)
//            {
//                var s = lines[i];
//                // 雑だが軽い：先頭2桁+KBGN を見る
//                if (s?.Length >= 6 && char.IsDigit(s[0]) && char.IsDigit(s[1]) && s.Contains("KBGN"))
//                {
//                    if (int.TryParse(s[..2], out var nn) && nn is >= 1 and <= 99)
//                    {
//                        trackNo = nn; lineIndex = i; return true;
//                    }
//                }
//            }
//            return false;
//        }
//    }
//}
