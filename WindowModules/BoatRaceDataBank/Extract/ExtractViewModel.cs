using System.Collections.ObjectModel;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.WindowModules.AppShared.Base;
// -------------------------------
// FILE: ExtractViewModel.cs
// -------------------------------


namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Extract
{
    public sealed class ExtractViewModel : BaseViewModel
    {
        // ExtractViewModel に “テキスト版” プロパティを追加
        private string _targetsText = string.Empty;
        public string TargetsText
        {
            get => _targetsText;
            set => SetProperty(ref _targetsText, value ?? string.Empty);
        }

        private readonly System.Text.StringBuilder _logSb = new();
        private string _logText = string.Empty;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value ?? string.Empty);
        }



        // 上段：対象一覧

        // B/Kフィルタ
        private bool _filterB = true;
        public bool FilterB { get => _filterB; set => SetProperty(ref _filterB, value); }
        private bool _filterK = false;
        public bool FilterK { get => _filterK; set => SetProperty(ref _filterK, value); }

        // ルート（例：D:\BRDB\Files など）。必要に応じて設定から注入可
        private string _rootPath = @"C:\BoatRaceDataBank";
        public string RootPath { get => _rootPath; set => SetProperty(ref _rootPath, value ?? string.Empty); }

        // 実行中フラグ＆進行メッセージ（進捗バーなし）
        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set => SetProperty(ref _isRunning, value); }

        private string _currentMessage = string.Empty;
        public string CurrentMessage { get => _currentMessage; private set => SetProperty(ref _currentMessage, value ?? string.Empty); }

        // 下段：テキストログ
        public ObservableCollection<string> LogLines { get; } = new();

        // 操作（OperationItemsパターン）
        public ObservableCollection<OperationItem> OperationItems { get; } = new();

        private OperationItem? _selectedOperation;
        public OperationItem? SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetProperty(ref _selectedOperation, value))
                {
                    if (_selectedOperation != null)
                    {
                        Dispatch(_selectedOperation.OperationName);
                        _selectedOperation = null;
                        OnPropertyChanged(nameof(SelectedOperation));
                    }
                }
            }
        }

        // Processor / Session
        private readonly ExtractProcessor _processor = new();
        private ExtractSession? _session;

        public ExtractViewModel() { }

        // ========== 初期化 ==========
        public void InitializeFromSetting(string windowUniqueId)
        {

            Title = "Extract (LZH → TXT / ARCH)";
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
            OperationItems.Add(new OperationItem("走査", "Scan"));
            OperationItems.Add(new OperationItem("実行", "Run"));
            OperationItems.Add(new OperationItem("停止", "Stop"));
        }




        private void Dispatch(string op)
        {
            switch (op)
            {
                case "Initialize":
                    Initialize();
                    break;
                case "Scan":
                    Scan();
                    break;
                case "Run":
                    Run();
                    break;
                case "Stop":
                    Stop();
                    break;
                default:
                    break;
            }
        }

        private void Initialize()
        {
            if (IsRunning) return;
            TargetsText = string.Empty;
            LogText = string.Empty;
            _logSb.Clear();
            CurrentMessage = "初期化しました。";
            _session = null;
        }

        // ログ追記（UIスレッド限定：同期実行のため特別なマーシャリング不要）

        private void AppendLog(string line)
        {
            // 末尾追記・UIは軽量テキストのみ

            _logSb.AppendLine(line);
            LogText = _logSb.ToString();
            // 1000行などの上限管理は必要なら後で
        }

        private void Scan()
        {
            if (IsRunning) return;
            TargetsText = string.Empty;
            LogText = string.Empty;
            _logSb.Clear();
            CurrentMessage = "走査中…";

            try
            {
                var plans = _processor.Scan(RootPath, FilterB, FilterK);
                // 表示用の素朴な整形（列＝BK / サイズ / ファイル名）
                var sb = new System.Text.StringBuilder(plans.Count * 32);
                foreach (var p in plans)
                {
                    var bk = Extract.Statics.PathMap.BranchLabelFromPath(p.LzhPath);
                    var name = System.IO.Path.GetFileName(p.LzhPath);
                    sb.Append(bk).Append('\t')
                      .Append(p.SizeBytes.ToString("N0")).Append('\t')
                      .AppendLine(name);
                }
                TargetsText = sb.ToString();

                AppendLog($"SCAN OK: {plans.Count} 件");
                CurrentMessage = $"走査完了（{plans.Count}件）";
            }
            catch (Exception ex)
            {
                AppendLog($"SCAN ERROR: {ex.Message}");
                CurrentMessage = "走査エラー";
            }
        }


        private void Run()
        {
            if (IsRunning) return;

            var plans = _processor.Scan(RootPath, FilterB, FilterK); // シンプルに再スキャン
            if (plans.Count == 0)
            {
                AppendLog("RUN SKIP: 対象がありません。");
                return;
            }

            _session = new ExtractSession
            {
                Overwrite = ExtractEnums.OverwritePolicy.Skip,
                DegreeOfParallelism = 1,
            };

            IsRunning = true;
            CurrentMessage = "実行開始…";
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // ★同期実行：処理中はUI入力できません（最小・シンプル優先）
                _processor.Run(
                    plans, _session!,
                    onInfo: msg => AppendLog(msg),
                    onCurrent: msg => CurrentMessage = msg,
                    System.Threading.CancellationToken.None
                );

                sw.Stop();
                AppendLog($"RUN DONE: OK={_session.Ok} SKIP={_session.Skip} FAIL={_session.Fail} / {sw.Elapsed}");
                CurrentMessage = "実行完了";
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppendLog($"RUN ERROR: {ex.Message}");
                CurrentMessage = "実行エラー";
            }
            finally
            {
                IsRunning = false;
            }
        }

        private void Stop()
        {
            // 初稿は逐次・キャンセルなし。将来 CTS を導入。
            AppendLog("STOP: 初稿は未対応（逐次のみ）");
            CurrentMessage = "停止（未対応）";
        }
    }

    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName)
        { DisplayText = displayText; OperationName = operationName; }
    }
}