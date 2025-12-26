using ApplicationSuite.AppShared.Commands;
using ApplicationSuite.Runtime.Pairing;
using ApplicationSuite.WindowModules.AppShared.Base;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Registry;
using ApplicationSuite.WindowModules.SecondaryWindow.ManualView.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ApplicationSuite.WindowModules.SecondaryWindow.ManualView
{
    /// <summary>
    /// アンカー同期とオートスクロールの“最小導線”のみを実装。
    /// - AnchorId 変更 → Resolve → ScrollTargetIndex を更新（Behavior がUIを動かす）
    /// - AutoScrollEnabled はトグルの受け口（上流通知はユーザー側で実装予定）
    /// </summary>
    /// 
    public class ManualViewViewModel : BaseViewModel
    {
        // ========== 基本プロパティ ==========
        private string _pairedWindowUniqueId = string.Empty;   // Primary の相手ID
        public string PairedWindowUniqueId
        {
            get => _pairedWindowUniqueId;
            set => SetProperty(ref _pairedWindowUniqueId, value);
        }

        // コマンド追加
        public ICommand SaveCommand { get; private set; }


        // _autoScroll Enabled は UI のトグル（初期ON）
        private bool _autoScrollEnabled = false;
        public bool AutoScrollEnabled
        {
            get => _autoScrollEnabled;
            set
            {
                // ********** トグル変更 ＝ セットフロー ① **********
                if (SetProperty(ref _autoScrollEnabled, value)) 
                {
                    // トグルに変更があったときは、上流に通知
                    string b = PairSyncRelay.GetElementId(PairedWindowUniqueId, WindowUniqueId, _autoScrollEnabled);

                    AnchorId = b;
                }
            }
        }
        // ========== アンカー（外部から流入） ==========
        private string _anchorId = string.Empty;
        public string AnchorId
        {
            get => _anchorId;
            set
            {
                if (SetProperty(ref _anchorId, value))
                {
                    if (string.IsNullOrEmpty(_anchorId)) return;


                    // ********** アンカー変更 ＝ セットフロー ② **********
                    // AnchorId が更新されたら“それ自体がトリガ”
                    TryResolveAndScroll(_anchorId);
                }
            }
        }
        // ========== Anchor → offset 変換 → Behavior へ ==========
        private void TryResolveAndScroll(string anchorId)
        {
            if (!AutoScrollEnabled) return;
            if (string.IsNullOrEmpty(Content)) return;

            int offset = AnchorResolver.ResolveFirstAnchorOffset(Content, anchorId);
            // 見つからないときは -1（Behavior 側は何もしない）
            ScrollTargetIndex = offset;
        }

        // ========== Behavior に渡す“数値” ==========
        private int _scrollTargetIndex = -1;
        public int ScrollTargetIndex
        {
            get => _scrollTargetIndex;
            private set => SetProperty(ref _scrollTargetIndex, value);
        }

        // ========== ドキュメント名 ==========
        private string _currentDocId = ""; // MdFullPath = manualRegistry Key
        public string CurrentDocId
        {
            get => _currentDocId;
            set
            {
                if (SetProperty(ref _currentDocId, value))
                {

                    // ********** ロードフロー ② **********
                    LoadDocument(_currentDocId);
                }
            }
        }
        // ========== ドキュメント内容 ==========
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        // ========== 手動 初期化 の受け口 ==========
        // コンストラクタにロジックを置かないプロジェクト方針に合わせたメソッド。
        public void InitializeFromSetting(string windowUniqueId)
        {
            WindowUniqueId = windowUniqueId;

            // ********** ManualView 初期化 **********
            // Primary 側の WindowUniqueId で、ターゲットＭＤファイルのフルパスを取得する。
            string CIdMD = Regex.Replace(PairedWindowUniqueId, @"\d+$", "") +  ".md";
            string MdFullPath = ManualViewSupport.GetFullPath(CIdMD);
            // ファイル読み込みと Registry 登録
            bool b = ManualViewSupport.manualReadAndSetRegistry(MdFullPath, out string content);
            // ********** ManualView 初期化 **********

            // ********** ロードフロー① **********
            // 初回ロード
            CurrentDocId = MdFullPath;

            // SaveCommand 初期化
            SaveCommand = new RelayCommand(_ => SaveDocument());
        }

        // 保存処理
        private void SaveDocument()
        {
            ManualViewSupport.Save(CurrentDocId, Content);
        }

        // ========== Registry から読み込み ==========
        public void LoadDocument(string MdFullPath)
        {
            var content = ManualDocumentRegistry.Instance.Get(MdFullPath);

            // ********** ロードフロー ③ **********
            // 最小：レジストリから本文だけ取得（例外処理は段階追加でOK）
            Content = content;

            // ドキュメント切替時はスクロール指示をリセット
            ScrollTargetIndex = -1;
        }
    }
}
