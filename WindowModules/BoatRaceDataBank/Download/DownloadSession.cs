using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Download
{
    /// <summary>
    /// HTTP 取得とローカル保存を担当（逐次）。CRC等の重い検証は「解凍」工程に委ねる。
    /// </summary>
    public sealed class DownloadSession
    {
        public enum Status { Ok, Skipped, Error }

        public readonly struct DownloadResult
        {
            public Status Status { get; }
            public bool Retryable { get; }
            public string? ErrorMessage { get; }
            public string? LocalPath { get; }
            public long SizeBytes { get; }

            private DownloadResult(Status s, bool retryable, string? errormessage, string? path, long size)
            { Status = s; Retryable = retryable; ErrorMessage = errormessage; LocalPath = path; SizeBytes = size; }

            public static DownloadResult Ok(string path, long size) => new(Status.Ok, false, null, path, size);
            public static DownloadResult Skipped(string path) => new(Status.Skipped, false, null, path, 0);
            public static DownloadResult Fail(string errorMessage, bool retryable) // ← Error → Fail
                => new(Status.Error, retryable, errorMessage, null, 0);
        }

        // HttpClient は使い回し（ソケット枯渇防止）
        private static readonly HttpClient _http = CreateClient();

        private static HttpClient CreateClient()
        {
            var h = new HttpClient();
            h.Timeout = TimeSpan.FromSeconds(60); // 1リクエスト上限
            return h;
        }

        /// <summary>
        /// 1ファイルをダウンロードして保存。存在済みなら Skipped。
        /// 保存先は {saveRoot}\{yyyy}\{fileName}。
        /// </summary>
        public async Task<DownloadResult> DownloadOneAsync(
            UriComposer.UriItem item,
            string saveRoot,
            CancellationToken ct)
        {
            string yearFolder = Path.Combine(saveRoot ?? "", item.Date.ToString("yyyy"));
            string fileName = item.FileName;
            string targetPath = Path.Combine(yearFolder, fileName);

            try
            {
                Directory.CreateDirectory(yearFolder);

                // 既存ならスキップ
                if (File.Exists(targetPath))
                    return DownloadResult.Skipped(targetPath);

                // 取得（ヘッダ先行でステータス判定）
                using var resp = await _http.GetAsync(item.UriString, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    var code = (int)resp.StatusCode;
                    bool retryable = resp.StatusCode == HttpStatusCode.TooManyRequests || (code >= 500 && code <= 599);
                    return DownloadResult.Fail($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}", retryable);
                }

                // 受信＆保存（.part → Move で原子的に）
                var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
                if (bytes.Length == 0)
                    return DownloadResult.Fail("Empty response", retryable: true);

                string tempPath = targetPath + ".part";
                try
                {
                    await File.WriteAllBytesAsync(tempPath, bytes, ct);
                    if (File.Exists(targetPath)) File.Delete(targetPath);
                    File.Move(tempPath, targetPath);
                }
                finally
                {
                    // 途中失敗の掃除
                    if (File.Exists(tempPath))
                    {
                        try { File.Delete(tempPath); } catch { /* ignore */ }
                    }
                }

                return DownloadResult.Ok(targetPath, bytes.LongLength);
            }
            catch (OperationCanceledException) { throw; }
            catch (HttpRequestException ex)
            {
                // ネットワーク系はリトライ可とする
                return DownloadResult.Fail($"HttpRequestException: {ex.Message}", retryable: true);
            }
            catch (IOException ex)
            {
                // IO は環境依存。原則リトライ不可
                return DownloadResult.Fail($"IOException: {ex.Message}", retryable: false);
            }
            catch (Exception ex)
            {
                // その他は一旦リトライ不可扱い
                return DownloadResult.Fail($"{ex.GetType().Name}: {ex.Message}", retryable: false);
            }
        }
    }
}
