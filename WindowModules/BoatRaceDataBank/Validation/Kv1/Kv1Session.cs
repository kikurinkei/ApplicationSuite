using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions.KTokens;

// File: Validation/Kv1/Kv1Session.cs
// 目的：1ファイルの v1 チェック（構造／固定行数）。KTokens で行種別だけ見て最小限に判定。
// 返り値： "Ok" / "Ng"。note は "Struct" / "Lines" / "Struct,Lines" の 3 通り（または null）。
// 方針：状態を持たない（Session は 1ファイルずつ使い捨て）。副作用なし（ログなし）。

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Kv1
{
    public sealed class Kv1Session
    {
        private enum State { Outside, InVenueHeader, InRace }

        public string Run(string filePath, out string? note)
        {
            note = null;

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
                        if (state != State.Outside || seenFileBegin ) structBad = true;
                        seenFileBegin = true;
                        break;

                    case KTokenKind.FileEnd:
                        // FILEEND は Outside で閉じるのが正解
                        if (state != State.Outside || !seenFileBegin) structBad = true;
                        seenFileEnd = true;
                        break;

                    case KTokenKind.VenueBegin:
                        // 会場開始は Outside でのみ許可。以降はヘッダカウント（KBGN を含めて 1 から）
                        if (state != State.Outside) structBad = true;
                        state = State.InVenueHeader;
                        currentVenueNo = tk.VenueNo;   // "nn"
                        headerCount = 1;               // KBGN 行を含むので 1 始まり
                        expectedRace = 1;              // 次の rR は 1
                        break;

                    case KTokenKind.RaceHeader:
                        // レース見出し（" 1R"〜"12R"）
                        if (!int.TryParse(tk.RaceNo, out int r)) { structBad = true; r = -1; }

                        if (state == State.InVenueHeader)
                        {
                            // 最初の rR は 1 であるべき。ヘッダ行数もここで確定判定。
                            if (r != 1) structBad = true;
                            if (headerCount != Kv1FormatSpec.HeaderLinesFixed) linesBad = true;

                            // レースブロック開始（rR 行を含めて 1 始まり）
                            state = State.InRace;
                            raceCount = 1;
                            expectedRace = 2;
                        }
                        else if (state == State.InRace)
                        {
                            // 直前レースの行数を確定させてから次レースへ
                            if (raceCount != Kv1FormatSpec.RaceLinesFixed) linesBad = true;

                            if (r != expectedRace) structBad = true;
                            raceCount = 1; // 新しい rR を含めて 1
                            expectedRace++;
                        }
                        else
                        {
                            // Outside で rR は構造エラー
                            structBad = true;
                            // 以降の判定継続のため、最低限の遷移だけ行う
                            state = State.InRace;
                            raceCount = 1;
                            expectedRace = r + 1;
                        }
                        break;

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
            if (state != State.Outside) structBad = true;
            if (!seenFileBegin || !seenFileEnd) structBad = true;

            // --- 返却 ---
            if (!structBad && !linesBad) return "Ok";

            if (structBad && linesBad) note = "Struct,Lines";
            else if (structBad) note = "Struct";
            else if (linesBad) note = "Lines";

            return "Ng";
        }
    }
}