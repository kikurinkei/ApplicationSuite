using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ApplicationSuite.BoatRaceDataBank.Record.RecordUnits
{
    /// <summary>
    /// K系（番組テキスト）1入力TXTに対する、VMがファイル化に使う最小DTO。
    /// ※ I/Oは行わない。解析結果のみを保持。
    /// </summary>
    public sealed class KRecordUnit
    {
        /// <summary>処理した元TXTのフルパス。</summary>
        public required string FilePath { get; init; }

        /// <summary>"Ok" / "Ng"（XAML表示用）。</summary>
        public required string Status { get; init; }

        /// <summary>種別は常に "K"（番組）。</summary>
        public string Kind => "K";

        /// <summary>ファイル名由来のレース日。</summary>
        public DateOnly RDate { get; init; }

        /// <summary>場番号（VenueBegin 由来）。</summary>
        public int TrackNo { get; init; }

        /// <summary>Kのレコード仕様バージョン（将来拡張用）。</summary>
        public KRecordVersion Version { get; init; } = KRecordVersion.V1;

        /// <summary>v1：1レース=1行の最小ワイド行（レース番号＋登録番号×6）。</summary>
        public IReadOnlyList<KRaceRowV1> Rows { get; init; } = Array.Empty<KRaceRowV1>();
    }

    /// <summary>Kレコードの仕様バージョン。</summary>
    public enum KRecordVersion
    {
        V1 = 1,
        V2 = 2,
        V3 = 3
    }

    /// <summary>
    /// v1の1レース=1行（最小）：RaceNo と RegNo1..6。
    /// </summary>
    public readonly record struct KRaceRowV1(
        int RaceNo,
        int RegNo1,
        int RegNo2,
        int RegNo3,
        int RegNo4,
        int RegNo5,
        int RegNo6
    );
}
