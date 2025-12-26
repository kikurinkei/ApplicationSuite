using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Store.Repositories;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Store.Contract;


namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Download
{
    /// <summary>
    /// Download の実処理（VMを薄く保つための委譲先）。
    /// RP: A/B/C、RR: A/B/C の対称実装。
    /// </summary>
    public sealed class DownloadProcessor
    {
        // A) RP
        private readonly RaceProgramRepository _rpRepo = new();
        private readonly DateRangePlanner _planner = new();

        public sealed record HeadResult(DateOnly? Last, DateRangePlanner.Plan Plan); // 共通DTO

        public HeadResult RpCheckDbHead()
        {
            var last = _rpRepo.GetLastRaceDate();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var seed = today.AddDays(-30);
            var plan = _planner.Build(last, today, seed, capDays: 30);
            return new HeadResult(last, plan);
        }

        public IReadOnlyList<UriComposer.UriItem> RpBuildUri(IReadOnlyList<DateOnly> targetDates)
            => UriComposer.BuildRpUris(targetDates ?? Array.Empty<DateOnly>());

        // C) RP: ダウンロード
        public sealed record DownloadSummary(int Succeeded, int Skipped, int Failed, TimeSpan Elapsed);

        private readonly DownloadSession _session = new();
        private readonly Random _rnd = new();

        public async Task<DownloadSummary> RpDownloadAsync(
            IReadOnlyList<UriComposer.UriItem> uris,
            string saveRoot,
            Action<string> log,
            CancellationToken ct = default)
        {
            return await DownloadCoreAsync(uris, saveRoot, log, ct);
        }

        // ===== RR 側 =====

        private readonly RaceResultRepository _rrRepo = new();

        public HeadResult RrCheckDbHead()
        {
            var last = _rrRepo.GetLastRaceDate();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var seed = today.AddDays(-30);
            var plan = _planner.Build(last, today, seed, capDays: 30);
            return new HeadResult(last, plan);
        }

        public IReadOnlyList<UriComposer.UriItem> RrBuildUri(IReadOnlyList<DateOnly> targetDates)
            => UriComposer.BuildRrUris(targetDates ?? Array.Empty<DateOnly>());

        public async Task<DownloadSummary> RrDownloadAsync(
            IReadOnlyList<UriComposer.UriItem> uris,
            string saveRoot,
            Action<string> log,
            CancellationToken ct = default)
        {
            return await DownloadCoreAsync(uris, saveRoot, log, ct);
        }

        // 共通DL本体（逐次＋スロットリング＋リトライ）
        private async Task<DownloadSummary> DownloadCoreAsync(
            IReadOnlyList<UriComposer.UriItem> uris,
            string saveRoot,
            Action<string> log,
            CancellationToken ct)
        {
            var startAt = DateTime.UtcNow;
            int ok = 0, skip = 0, err = 0;

            for (int i = 0; i < uris.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var item = uris[i];

                int attempt = 0;
                while (true)
                {
                    attempt++;
                    var result = await _session.DownloadOneAsync(item, saveRoot, ct);

                    if (result.Status == DownloadSession.Status.Ok)
                    {
                        ok++;
                        log($"[OK]    {item.UriString} -> {result.LocalPath} ({result.SizeBytes:N0} bytes)");
                        break;
                    }
                    if (result.Status == DownloadSession.Status.Skipped)
                    {
                        skip++;
                        log($"[SKIP]  {item.UriString} (exists)");
                        break;
                    }

                    if (attempt >= 3 || !result.Retryable)
                    {
                        err++;
                        log($"[ERR]   {item.UriString} ({result.ErrorMessage})");
                        break;
                    }

                    var backoff = attempt == 1 ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(15);
                    log($"[RETRY] {item.UriString} after {backoff.TotalSeconds}s ({result.ErrorMessage})");
                    await Task.Delay(backoff, ct);
                }

                if (i < uris.Count - 1)
                {
                    int delaySec = _rnd.Next(10, 21); // [10,20]
                    await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
                }
            }

            var elapsed = DateTime.UtcNow - startAt;
            return new DownloadSummary(ok, skip, err, elapsed);
        }
    }
}
