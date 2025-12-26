using ApplicationSuite.Runtime.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Registry
{
    // FILE: Runtime/Pairing/ManualDocumentRegistry.cs
    // ROLE: ManualViewViewModel から参照される「書庫」。
    //       内部で閉じた世界の最小実装。Primary/Relay/外部読み書きは関知しない。
    // NOTE: モックデータで初期化してある。必要になれば差し替え可能。


    public class ManualDocumentRegistry
    {
        // ---- Singleton ----
        private static ManualDocumentRegistry? _instance;
        public static ManualDocumentRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ManualDocumentRegistry();
                }
                return _instance;
            }
        }

        // ---- Storage ----
        private readonly Dictionary<string, string> _documents = new();

        // プライベートコンストラクタにより外部からの生成を防止
        private ManualDocumentRegistry()
        {
            //Seed(); // [MOCK] 初期データ投入
            //_documents = new();
        }
        // ---- API ----

        /// <summary>
        /// ドキュメントが存在するか確認する。
        /// </summary>
        public bool Has(string docId)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));
            return _documents.ContainsKey(docId);
        }

        /// <summary>
        /// 本文を取得する。存在しなければ例外。
        /// </summary>
        public string Get(string docId)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));
            if (!_documents.TryGetValue(docId, out var content))
                throw new KeyNotFoundException($"Document not found: {docId}");
            return content;
        }

        /// <summary>
        /// 本文を保存する。新規なら追加。
        /// </summary>
        public void Set(string docId, string content)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            _documents[docId] = content;
        }

        /// <summary>
        /// 登録されている全ドキュメントIDを列挙する。
        /// </summary>
        public IEnumerable<string> ListAll()
        {
            return _documents.Keys;
        }


        // ---- [MOCK] 初期データ ----
//        private void Seed()
//        {
//            // [MOCK] ダミードキュメント
//            _documents["UtilityTools.md"] =
//@"# Utility Tools

//## intro
//ここはイントロダクション。

//## usage
//ここは使い方。

//";

//            // [TODO] 他のドキュメントを追加する場合はここに書く
//        }
    }
}
