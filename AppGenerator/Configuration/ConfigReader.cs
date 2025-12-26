using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Configuration
{
    /// <summary>
    /// JSONファイル群を読み込み、ConfigStore に構成情報として登録するクラス。
    /// 対象は Suite / Shell / Utility の3カテゴリ。
    /// 各カテゴリは .exe と同階層の /JSON/ 配下に保存されている前提。
    /// </summary>
    public static class ConfigReader
    {
        /// <summary>
        /// 指定されたパスから JSON を読み込み、T 型にデシリアライズする。
        /// ファイル存在チェックと例外処理を含む安全設計。
        /// </summary>
        private static T? LoadJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[ConfigReader] ファイルが存在しません: {path}");
                return default;
            }
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigReader] JSON読み込み失敗: {path} → {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 全カテゴリ（Suite / Shell / Utility）の構成を読み込み、
        /// ConfigStore に登録するルート処理。
        /// </summary>
        public static void LoadAll()
        {
            string basePath = AppContext.BaseDirectory;
            string resolvedSuiteRootPath = Path.Combine(basePath, "JSON", "WindowApplicationSuite");
            string resolvedUShellRootPath = Path.Combine(basePath, "JSON", "WindowApplicationShell");
            string resolvedUtilityRootPath = Path.Combine(basePath, "JSON", "WindowApplicationUtility");

            // Suite構成の読み込み → 登録
            var suiteElements = Load(resolvedSuiteRootPath);
            ConfigStore.SetSuiteElements(suiteElements);

            // Shell構成の読み込み → 登録
            var shellElements = Load(resolvedUShellRootPath);
            ConfigStore.SetShellElements(shellElements);

            // Utility構成の読み込み → 登録
            var utilityElements = LoadUtilityElements(resolvedUtilityRootPath);
            ConfigStore.SetUtilityElements(utilityElements);

        }
        /// <summary>
        /// 指定カテゴリフォルダから複数の JSON を読み込み、
        /// ElementDetail 辞書としてまとめて返す。
        /// </summary>
        private static Dictionary<string, ElementDetail> Load(string resolvedRootPath)
        {
            var result = new Dictionary<string, ElementDetail>();

            if (!Directory.Exists(resolvedRootPath))
            {
                Console.WriteLine($"[ConfigReader] ディレクトリが存在しません: {resolvedRootPath}");
                return result; // 空辞書で返す（安全）
            }

            // JSONファイルをすべて走査
            foreach (var file in Directory.GetFiles(resolvedRootPath, "*.json"))
            {
                Console.WriteLine($"[ConfigReader] 読み込み対象: {file}");

                // 個別ファイルをデシリアライズ
                var data = LoadJson<Dictionary<string, ElementDetail>>(file);
                if (data == null)
                {
                    Console.WriteLine($"[ConfigReader] デシリアライズ失敗: {file}");
                    continue;
                }

                // 読み込んだ内容を統合（重複IDは上書き）
                foreach (var kv in data)
                {
                    result[kv.Key] = kv.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Utility構成のJSONファイルを読み込み、ElementIdをキーとした辞書に変換する。
        /// ※ ルート直下のJSONは読み込まず、必ずサブフォルダ配下のみを対象とする。
        /// </summary>
        /// <param name="utilityRootPath">Utility構成のルートフォルダ（例: "WindowApplicationUtility/"）</param>
        /// <returns>ElementIdをキーとしたElementDetailの辞書</returns>
        public static Dictionary<string, ElementDetail> LoadUtilityElements(string resolvedUtilityRootPath)
        {
            var result = new Dictionary<string, ElementDetail>();

            // 既存Loadと同様：まずはルートディレクトリの存在確認＋同一フォーマットのログ
            if (!Directory.Exists(resolvedUtilityRootPath))
            {
                Console.WriteLine($"[ConfigReader] ディレクトリが存在しません: {resolvedUtilityRootPath}");
                return result; // 空で返す
            }

            // 「Utilityはルート直下は読まず、“サブフォルダ直下の *.json” のみ読む」仕様
            foreach (var subdir in Directory.GetDirectories(resolvedUtilityRootPath))
            {
                foreach (var file in Directory.GetFiles(subdir, "*.json", SearchOption.TopDirectoryOnly))
                {
                    // 既存Loadと同じログ
                    Console.WriteLine($"[ConfigReader] 読み込み対象: {file}");

                    // 既存Loadと同じく共通のLoadJson<T>でデシリアライズ
                    var data = LoadJson<Dictionary<string, ElementDetail>>(file);
                    if (data == null)
                    {
                        Console.WriteLine($"[ConfigReader] デシリアライズ失敗: {file}");
                        continue;
                    }

                    // 既存Loadと同じ「重複キーは後勝ち」で統合
                    foreach (var kv in data)
                    {
                        result[kv.Key] = kv.Value;
                    }
                }
            }

            return result;
        }
    }
}
