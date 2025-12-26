using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions.KTokens
{

    // File: StaticFunctions/KTokens/KTokens.cs
    // 目的：1行テキストから「何の行か」を純粋関数で判定し、必要最小の付随情報を返す。
    // ポイント：副作用なし（Console/ログ出力なし）、厳密一致、固定桁ルール踏襲。


    /// <summary>判定された行の種類</summary>
    public enum KTokenKind
    {
        Unknown = 0,
        FileBegin,   // "STARTK"
        FileEnd,     // "FINALK"
        VenueBegin,  // "nnKBGN"（IndexOf("BGN",3)==3）
        VenueEnd,    // "nnKEND"（IndexOf("END",3)==3）
        RaceHeader   // "[2]が数字or空白, [3]が数字, [4]=='R'"
    }

    /// <summary>1行から得られる最小情報（必要なものだけ）</summary>
    public sealed class KToken
    {
        public KTokenKind Kind { get; }
        /// <summary>会場番号（先頭2桁をそのまま保持。ゼロ/空白含む場合あり）</summary>
        public string VenueNo { get; }
        /// <summary>レース番号（2桁相当をTrimした表示用："1" / "12"など）</summary>
        public string RaceNo { get; }

        public KToken(KTokenKind kind, string venueNo = null, string raceNo = null)
        {
            Kind = kind;
            VenueNo = venueNo;
            RaceNo = raceNo;
        }
    }

    /// <summary>K系トークン（行種別）の判定ユーティリティ。純粋関数のみ。</summary>
    public static class KTokens
    {
        /// <summary>行の種別だけが欲しいとき用。該当しなければ Unknown。</summary>
        public static KTokenKind Classify(string line)
        {
            return TryParse(line, out var token) ? token.Kind : KTokenKind.Unknown;
        }

        /// <summary>
        /// 行を判定し、必要最小の付随情報（会場番号/レース番号）を返す。
        /// 該当しない場合は false（token.Kind=Unknown）。
        /// </summary>
        public static bool TryParse(string line, out KToken token)
        {
            token = new KToken(KTokenKind.Unknown);

            if (string.IsNullOrEmpty(line))
                return false;

            // 1) ファイル境界
            if (IsFileBegin(line))
            {
                token = new KToken(KTokenKind.FileBegin);
                return true;
            }
            if (IsFileEnd(line))
            {
                token = new KToken(KTokenKind.FileEnd);
                return true;
            }

            // 2) 会場境界（現行準拠：IndexOfで厳密位置のみ確認）
            if (TryParseVenueBegin(line, out var venueBegin))
            {
                token = new KToken(KTokenKind.VenueBegin, venueNo: venueBegin);
                return true;
            }
            if (TryParseVenueEnd(line, out var venueEnd))
            {
                token = new KToken(KTokenKind.VenueEnd, venueNo: venueEnd);
                return true;
            }

            // 3) レース見出し（固定桁）
            if (TryParseRaceHeader(line, out var raceNo))
            {
                token = new KToken(KTokenKind.RaceHeader, raceNo: raceNo);
                return true;
            }

            return false;
        }

        // ---------- 内部ヘルパ（限定公開） ----------

        private static bool IsFileBegin(string s)
            => string.Equals(s, "STARTK", StringComparison.Ordinal);

        private static bool IsFileEnd(string s)
            => string.Equals(s, "FINALK", StringComparison.Ordinal);

        private static bool TryParseVenueBegin(string s, out string venueNo)
        {
            venueNo = null;

            // 最低限の長さチェック + "BGN" が index=3 に厳密一致（現行ロジック踏襲）
            if (s.Length >= 6 && s.IndexOf("BGN", 3, StringComparison.Ordinal) == 3)
            {
                // 先頭2桁をそのまま保持（ゼロ/空白含むことを許容）
                venueNo = s.Substring(0, Math.Min(2, s.Length));
                return true;
            }
            return false;
        }

        private static bool TryParseVenueEnd(string s, out string venueNo)
        {
            venueNo = null;

            // 最低限の長さチェック + "END" が index=3 に厳密一致（現行ロジック踏襲）
            if (s.Length >= 6 && s.IndexOf("END", 3, StringComparison.Ordinal) == 3)
            {
                venueNo = s.Substring(0, Math.Min(2, s.Length));
                return true;
            }
            return false;
        }

        private static bool TryParseRaceHeader(string s, out string raceNo)
        {
            raceNo = null;

            // 固定桁：長さ>=5, [2]は数字or空白, [3]は数字, [4]=='R'
            if (s.Length >= 5)
            {
                char c2 = s[2];
                char c3 = s[3];
                if ((char.IsDigit(c2) || c2 == ' ') && char.IsDigit(c3) && s[4] == 'R')
                {
                    // 2桁相当をTrimして表示用の番号に（" 1" -> "1", "12" -> "12"）
                    raceNo = new string(new[] { c2, c3 }).Trim();
                    return true;
                }
            }
            return false;
        }
    }
}

