using ApplicationSuite.BoatRaceDataBank.Record.RecordUnits;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Kv1;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Models;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Record.StaticFunctions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Record.Kv1
{
    public class Kv1Processor
    {
        public async Task<IReadOnlyList<RecordResult>> RunCheckBatchAsync(
                IReadOnlyList<string> files,
                int dop = 6,
                CancellationToken ct = default)
        {
            if (files is null) throw new ArgumentNullException(nameof(files));

            // UIスレッドをふさがないため、重い処理はスレッドプール側で実行
            return await Task.Run(() =>
            {
                // 1) 入力順を保持するため、インデックス付きにして配列化
                var indexed = files
                    .Select((path, idx) => new IndexedPath(idx, path))
                    .ToArray();

                // 2) 並列処理で結果を溜める入れ物（スレッドセーフ）
                var bag = new ConcurrentBag<(int Index, RecordResult Result)>();

                // 3) 並列度（dop）。6なら“Sessionが最大6個”だけ作られ、以後再利用される
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, dop),
                    CancellationToken = ct
                };

                // 4) Parallel.ForEach の localInit/localFinally を使用
                //    → 各ワーカー（=最大dop本）で 1 回だけ Session を new し、以後その Session を再利用
                Parallel.ForEach<IndexedPath, Kv1Session>(
                    source: indexed,
                    parallelOptions: options,
                    localInit: () => new Kv1Session(),
                    body: (item, loopState, session) =>
                    {
                        // キャンセル要求が来ていたら中断（上位へ例外として伝播させる）
                        options.CancellationToken.ThrowIfCancellationRequested();

                        var sw = Stopwatch.StartNew();

                        string verdict = "Ng";   // "Ok" or "Ng"（既定はNgとしておき、成功でOkに上書き）
                        string? note = null;     // Session.Run からの補足
                        string? exSummary = null; // 例外が出たときの短い要約

                        try
                        {
                            // ★ Session は「1ファイルの手足」：読み込み→行判定→可視化（Console）まで担当
                            verdict = session.Run(item.Path, out note);
                            // 期待値としては "Ok"。Session内で例外が起きなければ Ok を返す現在仕様
                        }
                        catch (Exception ex)
                        {
                            // ★ 例外は“落とさない”。Ng(例外)として回収して処理続行
                            verdict = "Ng";
                            exSummary = $"{ex.GetType().Name}: {ex.Message}";
                        }
                        finally
                        {
                            sw.Stop();

                            // RecordResult は UI でそのまま整形・表示できる共通モデル
                            var vr = new RecordResult
                            {
                                FilePath = item.Path,
                                Result = verdict,                  // "Ok" or "Ng"
                                Note = BuildNote(note, exSummary), // 例外時は要約を追記
                                DestinationPath = null,            // V1チェックでは未使用
                                ElapsedMs = sw.ElapsedMilliseconds
                            };

                            bag.Add((item.Index, vr));
                        }

                        // ★ この return は「このワーカー専用の Session を次も使う」ために必要
                        return session;
                    },
                    localFinally: session =>
                    {
                        // 現状、SessionはIDisposableではないので後始末なし。
                        // 将来、ファイルハンドル等のリソースを持つ場合はここで解放する。
                    });

                // 5) 元の順序に並べ直して返却（UIで読みやすい）
                var ordered = bag
                    .OrderBy(x => x.Index)
                    .Select(x => x.Result)
                    .ToList();

                return (IReadOnlyList<RecordResult>)ordered;
            }, ct).ConfigureAwait(false);
        }

        /// <summary>Processor内の状態を持たないため、現状はダミー。拡張時に使用可。</summary>
        public void Clear()
        {
            // 状態を保持しない方針なので、現状は何もしません。
            // 例：将来、内部キャッシュや統計情報を持たせた場合にここでクリア。
        }

        // ---- ここから下は内部ヘルパ ------------------------------------------------

        private static string BuildNote(string? noteFromSession, string? exceptionSummary)
        {
            // 優先順位：
            //  1) 例外があれば例外要約を先頭に
            //  2) Session側の補足（nullなら空文字）
            if (!string.IsNullOrEmpty(exceptionSummary))
            {
                // 例外要約 + （必要なら）既存ノート
                if (!string.IsNullOrEmpty(noteFromSession))
                    return $"{exceptionSummary} | {noteFromSession}";
                return exceptionSummary;
            }
            return noteFromSession ?? string.Empty;
        }

        private readonly struct IndexedPath
        {
            public int Index { get; }
            public string Path { get; }
            public IndexedPath(int index, string path)
            {
                Index = index;
                Path = path ?? throw new ArgumentNullException(nameof(path));
            }
        }


        /// <summary>
        /// VM の「Kv1Record」用：TXT 群を Kv1Session にかけ、KRecordUnit のリストを返す。
        /// I/O は一切行わない。入力順を維持。並列度は dop。
        /// </summary>
        public async Task<List<KRecordUnit>> RunKv1RecordBatchAsync(
            IEnumerable<string> txtPaths,
            int dop = 6,
            CancellationToken ct = default)
        {
            if (txtPaths is null) throw new ArgumentNullException(nameof(txtPaths));

            var paths = txtPaths.ToList();
            var results = new KRecordUnit[paths.Count];

            using var gate = new SemaphoreSlim(dop);
            var tasks = new List<Task>(paths.Count);

            for (int index = 0; index < paths.Count; index++)
            {
                var i = index;
                var path = paths[i];

                tasks.Add(Task.Run(async () =>
                {
                    await gate.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        // Session 実行（I/Oなし）
                        var s = new Kv1Session();

                        // エラー回避/コンパイルエラー回避/変更前
                        // var status = s.RunForRecord(path); // 既存の "Ok"/"Ng" を返す想定


                        // エラー回避/変更後（既存の Run を呼ぶだけ）
                        var status = s.Run(path, out _);



                        // “箱”にまとめる（I/Oなし・判定なし）
                        var unit = Kv1RecordUnitBuilder.Build(path, status, s);
                        results[i] = unit;
                    }
                    finally
                    {
                        gate.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.ToList();
        }
    }

}
