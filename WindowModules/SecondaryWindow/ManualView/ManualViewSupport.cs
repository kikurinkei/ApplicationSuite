using ApplicationSuite.AppGenerator.Configuration;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ApplicationSuite.WindowModules.SecondaryWindow.ManualView
{
    public class ManualViewSupport
    {
        public static string GetMDFilePath()
        {
            string? rootPath = null;

            // SuiteId → ManualRootDir（プロジェクトの既存規約に従う）
            string suiteId = ConfigResolver.GetSuiteRootId();

            if (!string.IsNullOrEmpty(suiteId))
            {
                rootPath = ConfigResolver.GetManualRootDir(suiteId);
            }

            return rootPath;

        }

        public static string GetFullPath(string docId)
        {
            var root = GetMDFilePath();
            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("Manual Root Path is not configured.");
            }
            // フルパスを組み立てる
            var fullPath = System.IO.Path.Combine(root, docId);
            return fullPath;
        }

        public static bool manualReadAndSetRegistry(string filePath, out string content)
        {
            content = string.Empty;

            // ファイル読み込み
            try
            {
                if (!File.Exists(filePath)) return false;
                content = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch
            {
                return false;
            }

            // 登録
            ManualDocumentRegistry.Instance.Set(filePath, content);

            // 登録確認
            if (ManualDocumentRegistry.Instance.Has(filePath) == false) return false;

            return true;
        }



        public static void Save(string docId, string content)
        {
            // Registry に保存
            ManualDocumentRegistry.Instance.Set(docId, content);

            // MDファイルに保存
            File.WriteAllText(docId, content);
        }


    }
}
