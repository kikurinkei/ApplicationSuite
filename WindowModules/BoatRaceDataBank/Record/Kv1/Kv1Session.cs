using ApplicationSuite.BoatRaceDataBank.Record.RecordUnits;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.RecordUnits; // KRaceRowV1 用
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.KTokens;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions.Versions;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File: Record/Kv1/Kv1Session.cs
// 目的：1ファイルの v1 チェック（構造／固定行数）。KTokens で行種別だけ見て最小限に判定。
// 返り値： "Ok" / "Ng"。note は "Struct" / "Lines" / "Struct,Lines" の 3 通り（または null）。
// 方針：状態を持たない（Session は 1ファイルずつ使い捨て）。副作用なし（ログなし）。

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Kv1
{
    public sealed class Kv1Session
    {
        // === レース単位の完成品 ===
        private sealed class RaceRecord
        {
            public DateTime Date { get; set; }     // 日付（ファイル単位で固定）
            public int VenueNo { get; set; }       // 場番号
            public int DayNo { get; set; }         // 開催日（大会の経過日）
            public int RaceNo { get; set; }        // レース番号
            public int RacerNo { get; set; }       // レーサー番号
            public decimal WinOdds { get; set; }   // 単勝配当
        }

        // === Session 内で保持する材料 ===
        private DateTime _date;             // ファイル単位で固定
        private int _venueNo;               // 場番号
        private int _dayNo;                 // 開催日
        private int _raceNo;                // レース番号
        private readonly List<int> _racerNos = new(); // レーサー番号（複数）
        private decimal _winOdds;           // 単勝配当

        // === 完成品を積む箱 ===
        private readonly List<RaceRecord> _records = new();


        private enum State { Outside, InVenueHeader, InRace }


        public string Run(string filePath, out string? note)
        {
            note = null;

            // ファイル開始時に一度だけ呼び出す
            this._date = DateTime.ParseExact(
                ParseFileDate.FromFileName(filePath),
                "yyyyMMdd",
                null
            );


            //// （読み込みブロックの直前に追加）
            ///* --- ファイル名→RDATE（先に確定：不一致は line=0 扱い） --- */
            //if (!FileNameDateOps.TryParseKFileDate(filePath, out var rdate))
            //    throw new InvalidDataException($"{Path.GetFileName(filePath)} (line=0)");



            // --- 読み込み（SJIS→UTF8 フォールバック） ---
            string[] lines;
            try { lines = File.ReadAllLines(filePath, Encoding.GetEncoding(932)); }
            catch { lines = File.ReadAllLines(filePath, new UTF8Encoding(false)); }

            // --- 判定用の最小状態 ---
            var state = State.Outside;
            bool seenFileBegin = false;
            bool seenFileEnd = false;

            string? currentVenueNo = null;
            int headerCount = 0;      // KBGN を含めて数える
            int raceCount = 0;        // rR を含めて数える
            int expectedRace = 0;     // 次に来るべき rR（1 始まり）

            bool structBad = false;   // 構造エラー（順序・境界の不一致）
            bool linesBad = false;   // 行数エラー（固定値と不一致）


            // --- 走査 ---
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!KTokens.TryParse(line, out var tk))
                {
                    // トークン不明：カウントの対象だけ行う
                    if (state == State.InVenueHeader) headerCount++;
                    else if (state == State.InRace) raceCount++;
                    continue;
                }

                switch (tk.Kind)
                {
                    case KTokenKind.FileBegin:
                        // STARTK はファイルの外で 1 回だけ
                        if (state != State.Outside || seenFileBegin) structBad = true;
                        seenFileBegin = true;
                        break;

                    case KTokenKind.FileEnd:
                        // FILEEND は Outside で閉じるのが正解
                        if (state != State.Outside || !seenFileBegin) structBad = true;
                        seenFileEnd = true;
                        break;

                    case KTokenKind.VenueBegin:

                        // headerStart は KBGN/BBGN の行インデックス（i）
                        var headerStart = i;

                        // 固定行取り：6行目=大会名（前後トリム）、8行目=開催日目「第 n日」の n（5〜6文字目→半角化→数値）
                        VenueHeaderPick.ReadTitleAndDay(lines, headerStart, out var title, out var dayNo);



                        // ヘッダ残りを消費：次ループの i++ で 1R に着地
                        i = headerStart + Kv1FormatSpec.HeaderLinesFixed - 1;

                        // -- Recordでの追加処理終了 --


                        // 会場開始は Outside でのみ許可。以降はヘッダカウント（KBGN を含めて 1 から）
                        //if (state != State.Outside) structBad = true;
                        //state = State.InVenueHeader;
                        //currentVenueNo = tk.VenueNo;   // "nn"
                        //headerCount = 1;               // KBGN 行を含むので 1 始まり
                        //expectedRace = 1;              // 次の rR は 1

                        break;
                    case KTokenKind.RaceHeader:

                        var raceStart = i;

                        // rR 行の「1〜4文字目」を Trim→int として RaceNo、
                        // rR=1行目から 4〜9行目が 1〜6人目、各行の 9〜12文字目(4桁) を RegNo として取得
                        RaceHeaderPick.ReadRaceNoAndRegs(
                            lines, raceStart,
                            out var raceNo,
                            out var r1, out var r2, out var r3, out var r4, out var r5, out var r6
                        );


                        // このブロックを消費：次ループの i++ で次アンカー（次R/END）に着地
                        i = raceStart + Kv1FormatSpec.RaceLinesFixed - 1; // 既定: 21


                        break;

                    // --- 払戻金（単勝） ---
                    case KTokenKind.Odds:
                        {
                            // i が現在の行インデックスなので、それを渡す
                            this._winOdds = Kv1OddsPick.ReadWinOdds(lines, i);

                            // 材料が揃ったのでレコード化
                            foreach (var racerNo in this._racerNos)
                            {
                                var record = new RaceRecord
                                {
                                    Date = this._date,
                                    VenueNo = this._venueNo,
                                    DayNo = this._dayNo,
                                    RaceNo = this._raceNo,
                                    RacerNo = racerNo,
                                    WinOdds = this._winOdds
                                };
                                this._records.Add(record);
                            }

                            // 次レースに備えてレーサー番号をクリア
                            this._racerNos.Clear();
                            break;
                        }

                    case KTokenKind.VenueEnd:
                        // 会場終了：直前レースの行数判定＋レース数 12 本終了を確認
                        if (state != State.InRace) structBad = true;

                        // 直前のレース行数チェック（rR 含む）
                        if (raceCount != Kv1FormatSpec.RaceLinesFixed) linesBad = true;

                        // r=1..12 を消化した？ → 期待値が 13 になっているはず
                        if (expectedRace != Kv1FormatSpec.RacesPerVenue + 1) structBad = true;

                        // venueNo が一致しているか（最低限の整合性）
                        if (currentVenueNo != tk.VenueNo) structBad = true;

                        // 会場を閉じて Outside に戻る
                        state = State.Outside;
                        currentVenueNo = null;
                        headerCount = 0;
                        raceCount = 0;
                        expectedRace = 0;
                        break;

                    default:
                        // Unknown はここに来ない（TryParse で弾かれている）
                        break;
                }

                // 進行中のカウント（Known token の行は、上の分岐で必要箇所だけ +1 済）
                if (tk.Kind == KTokenKind.RaceHeader || tk.Kind == KTokenKind.VenueBegin)
                {
                    // すでにカウント済み（rR/KBGN を “含む” 仕様のためここでは何もしない）
                }
                else if (state == State.InVenueHeader)
                {
                    headerCount++;
                }
                else if (state == State.InRace)
                {
                    raceCount++;
                }
            }

            // --- 終了時の最終チェック ---

            bool isOk = true; // ← 実際は Validation 結果で判定
            if (isOk)
            {
                //Repository.Instance.Add(filePath, _records);
            }

            return isOk ? "Ok" : "Ng";


        }
    }
}