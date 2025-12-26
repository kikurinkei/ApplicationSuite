using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ApplicationSuite.WindowModules.AppShared.Base; // BaseViewModel
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Models;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.Kv1;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.StaticFunctions;
using ApplicationSuite.WindowModules.BoatRaceDataBank.Validation.IO;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Validation
{
    public sealed class ValidationViewModel : BaseViewModel
    {
        // ==== 上段/下段の表示 ====
        private string _inputText = string.Empty;
        public string InputText { get => _inputText; set => SetProperty(ref _inputText, value ?? string.Empty); }

        private string _outputText = string.Empty;
        public string OutputText { get => _outputText; set => SetProperty(ref _outputText, value ?? string.Empty); }

        // ==== ルート（K系：K系テキスト） ====
        private string _kSourceRoot = @"C:\BoatRaceDataBank\TXT\K\2025";
        public string KSourceRoot { get => _kSourceRoot; set => SetProperty(ref _kSourceRoot, value ?? string.Empty); }

        //private string _kv1OkRoot = @"C:\BoatRaceDataBank\TXT\K\_v1_ok";
        private string _kv1OkRoot = @"C:\BoatRaceDataBank\VAL\KV1\K\2025";
        public string Kv1OkRoot { get => _kv1OkRoot; set => SetProperty(ref _kv1OkRoot, value ?? string.Empty); }

        //private string _kv2OkRoot = @"C:\BoatRaceDataBank\TXT\K\_v2_ok";
        private string _kv2OkRoot = @"C:\BoatRaceDataBank\VAL\KV2\K\2025";
        public string Kv2OkRoot { get => _kv2OkRoot; set => SetProperty(ref _kv2OkRoot, value ?? string.Empty); }

        private string _kv3OkRoot = @"C:\BoatRaceDataBank\VAL\KV3\K\2025";
        public string Kv3OkRoot { get => _kv3OkRoot; set => SetProperty(ref _kv3OkRoot, value ?? string.Empty); }

        // ==== ルート（B系：B系テキスト） ====
        private string _bSourceRoot = @"C:\BoatRaceDataBank\TXT\B\2025";
        public string BSourceRoot { get => _bSourceRoot; set => SetProperty(ref _bSourceRoot, value ?? string.Empty); }

        private string _bv1OkRoot = @"C:\BoatRaceDataBank\VAL\KV1\B\2025";
        public string Bv1OkRoot { get => _bv1OkRoot; set => SetProperty(ref _bv1OkRoot, value ?? string.Empty); }

        private string _bv2OkRoot = @"C:\BoatRaceDataBank\VAL\KV2\B\2025";
        public string Bv2OkRoot { get => _bv2OkRoot; set => SetProperty(ref _bv2OkRoot, value ?? string.Empty); }

        // ==== 行ごとの OperationItems（XAML: ListBox ItemsSource=...） ====
        public ObservableCollection<OperationItem> Kv1OperationItems { get; } = new();
        public ObservableCollection<OperationItem> Kv2OperationItems { get; } = new();
        public ObservableCollection<OperationItem> Kv3OperationItems { get; } = new();
        public ObservableCollection<OperationItem> Bv1OperationItems { get; } = new();
        public ObservableCollection<OperationItem> Bv2OperationItems { get; } = new();

        // クリックされた項目（全ListBox共通）
        private OperationItem? _selectedOperation;
        public OperationItem? SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (SetProperty(ref _selectedOperation, value) && value != null)
                {
                    // 選択=実行。薄いルーター：Op名で単機能に分岐
                    _ = DispatchAsync(value.OperationName);
                    // 連続押下できるよう、選択解除
                    _selectedOperation = null;
                    OnPropertyChanged(nameof(SelectedOperation));
                }
            }
        }

        // ==== 一時キャッシュ（“再チェックしない”ための最小限） ====
        private List<string> _kCurrentFiles = new();  // KのDiscoverで作る一覧（Kv1/Kv2/Kv3で共用）
        private List<string> _bCurrentFiles = new();  // BのDiscoverで作る一覧（Bv1/Bv2で共用）

        private List<string> _kv1OkFiles = new();
        private List<string> _kv2OkFiles = new();
        private List<string> _kv3OkFiles = new();
        private List<string> _bv1OkFiles = new();
        private List<string> _bv2OkFiles = new();

        // ==== Processor（V1のみ確定。V2/V3は後で差し替え） ====
        private readonly Kv1Processor _kV1Processor = new();

        public ValidationViewModel() { }

        public void InitializeFromSetting(string windowUniqueId)
        {
            // 各行4ボタンずつ（Discover → Validate → Move → Clear）
            Kv1OperationItems.Add(new OperationItem("Kv1Discover", "Kv1Discover"));
            Kv1OperationItems.Add(new OperationItem("Kv1Validate", "Kv1Validate"));
            Kv1OperationItems.Add(new OperationItem("Kv1Move", "Kv1Move"));
            Kv1OperationItems.Add(new OperationItem("Kv1Clear", "Kv1Clear"));

            Kv2OperationItems.Add(new OperationItem("Kv2Discover", "Kv2Discover"));
            Kv2OperationItems.Add(new OperationItem("Kv2Validate", "Kv2Validate"));
            Kv2OperationItems.Add(new OperationItem("Kv2Move", "Kv2Move"));
            Kv2OperationItems.Add(new OperationItem("Kv2Clear", "Kv2Clear"));

            Kv3OperationItems.Add(new OperationItem("Kv3Discover", "Kv3Discover"));
            Kv3OperationItems.Add(new OperationItem("Kv3Validate", "Kv3Validate"));
            Kv3OperationItems.Add(new OperationItem("Kv3Move", "Kv3Move"));
            Kv3OperationItems.Add(new OperationItem("Kv3Clear", "Kv3Clear"));

            Bv1OperationItems.Add(new OperationItem("Bv1Discover", "Bv1Discover"));
            Bv1OperationItems.Add(new OperationItem("Bv1Validate", "Bv1Validate"));
            Bv1OperationItems.Add(new OperationItem("Bv1Move", "Bv1Move"));
            Bv1OperationItems.Add(new OperationItem("Bv1Clear", "Bv1Clear"));

            Bv2OperationItems.Add(new OperationItem("Bv2Discover", "Bv2Discover"));
            Bv2OperationItems.Add(new OperationItem("Bv2Validate", "Bv2Validate"));
            Bv2OperationItems.Add(new OperationItem("Bv2Move", "Bv2Move"));
            Bv2OperationItems.Add(new OperationItem("Bv2Clear", "Bv2Clear"));
        }

        // ==== 薄いディスパッチ（単機能で直線的に） ====
        private async Task DispatchAsync(string op)
        {
            try
            {
                switch (op)
                {
                    // --- K: Kv1 ---
                    case "Kv1Discover":
                        _kCurrentFiles = EnumerateTxt(KSourceRoot).ToList();
                        InputText = ResultText.FromFileList(KSourceRoot, _kCurrentFiles);
                        break;

                    case "Kv1Validate":
                        {
                            var results = await _kV1Processor.RunCheckBatchAsync(_kCurrentFiles, dop: 6);
                            _kv1OkFiles = results.Where(r => string.Equals(r.Result, "Ok", StringComparison.OrdinalIgnoreCase))
                                                 .Select(r => r.FilePath)
                                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                                 .ToList();
                            OutputText = ResultText.FromValidate("[Kv1]", results, KSourceRoot);
                            break;
                        }

                    case "Kv1Move":
                        {
                            var moves1 = MoveOps.MoveOkFiles(_kv1OkFiles, KSourceRoot, Kv1OkRoot);
                            OutputText = ResultText.FromMove("[Kv1]", moves1, Kv1OkRoot);
                            break;
                        }

                    case "Kv1Clear":
                        _kCurrentFiles.Clear();
                        InputText = string.Empty;
                        OutputText = string.Empty;
                        break;

                    // --- K: Kv2（Validate は暫定で v1 Processor を使用。後で差し替え） ---
                    case "Kv2Discover":
                        _kCurrentFiles = EnumerateTxt(KSourceRoot).ToList();
                        InputText = ResultText.FromFileList(KSourceRoot, _kCurrentFiles);
                        break;

                    case "Kv2Validate":
                        {
                            // TODO: V2専用Processorに差し替える
                            var results = await _kV1Processor.RunCheckBatchAsync(_kCurrentFiles , dop: 6);
                            _kv2OkFiles = results.Where(r => string.Equals(r.Result, "Ok", StringComparison.OrdinalIgnoreCase))
                                                 .Select(r => r.FilePath)
                                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                                 .ToList();
                            //OutputText = FormatResultsWithTag("[Kv2]", results, KSourceRoot);
                            OutputText = ResultText.FromValidate("[Kv2]", results, KSourceRoot);
                            break;
                        }

                    case "Kv2Move":
                        {
                            var moves2 = MoveOps.MoveOkFiles(_kv2OkFiles, KSourceRoot, Kv2OkRoot);
                            OutputText = ResultText.FromMove("[Kv2]", moves2, Kv2OkRoot);
                            break;
                        }

                    case "Kv2Clear":
                        _kv2OkFiles.Clear();
                        break;

                    // --- K: Kv3（Validate は暫定で v1 Processor を使用。後で差し替え） ---
                    case "Kv3Discover":
                        _kCurrentFiles = EnumerateTxt(KSourceRoot).ToList();
                        InputText = ResultText.FromFileList(KSourceRoot, _kCurrentFiles);
                        break;

                    case "Kv3Validate":
                        {
                            // TODO: V3専用Processorに差し替える
                            var results = await _kV1Processor.RunCheckBatchAsync(_kCurrentFiles , dop: 6);
                            _kv3OkFiles = results.Where(r => string.Equals(r.Result, "Ok", StringComparison.OrdinalIgnoreCase))
                                                 .Select(r => r.FilePath)
                                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                                 .ToList();
                            OutputText = ResultText.FromValidate("[Kv3]", results, KSourceRoot);
                            break;
                        }

                    case "Kv3Move":
                        {
                            var moves3 = MoveOps.MoveOkFiles(_kv3OkFiles, KSourceRoot, Kv3OkRoot);
                            OutputText = ResultText.FromMove("[Kv3]", moves3, Kv3OkRoot);
                            break;
                        }

                    case "Kv3Clear":
                        _kv3OkFiles.Clear();
                        break;

                    // --- B: Bv1（番組は現段階ではValidate未実装：分かりやすく明示） ---
                    case "Bv1Discover":
                        _bCurrentFiles = EnumerateTxt(BSourceRoot).ToList();
                        InputText = ResultText.FromFileList(KSourceRoot, _bCurrentFiles);
                        break;

                    case "Bv1Validate":
                        break;

                    case "Bv1Move":
                        {
                            var movesB1 = MoveOps.MoveOkFiles(_bv1OkFiles, BSourceRoot, Bv1OkRoot);
                            OutputText = ResultText.FromMove("[Bv1]", movesB1, Bv1OkRoot);
                            break;
                        }

                    case "Bv1Clear":
                        _bv1OkFiles.Clear();
                        break;

                    // --- B: Bv2 ---
                    case "Bv2Discover":
                        _bCurrentFiles = EnumerateTxt(BSourceRoot).ToList();
                        InputText = ResultText.FromFileList(KSourceRoot, _bCurrentFiles);
                        break;

                    case "Bv2Validate":
                        break;

                    case "Bv2Move":
                        {
                            var movesB2 = MoveOps.MoveOkFiles(_bv2OkFiles, BSourceRoot, Bv2OkRoot);
                            OutputText = ResultText.FromMove("[Bv2]", movesB2, Bv2OkRoot);
                            break;
                        }

                    case "Bv2Clear":
                        _bv2OkFiles.Clear();
                        break;

                    default:
                        // 何もしない
                        break;
                }
            }
            catch (Exception ex)
            {
                // 落とさない：下段に短い要約だけ表示
                OutputText = $"[ERR] {ex.GetType().Name}: {ex.Message}";
            }
        }

        // ==== 共通ツール（最小） ====
        private static IEnumerable<string> EnumerateTxt(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return Enumerable.Empty<string>();

            // 再帰なし。*.TXTのみ。必要になったら拡張する。
            return Directory.EnumerateFiles(root, "*.TXT", SearchOption.TopDirectoryOnly);
        }
    }

    // ListBoxのItemsSourceに並べる軽量アイテム
    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName)
        {
            DisplayText = displayText;
            OperationName = operationName;
        }
        public override string ToString() => DisplayText;
    }
}
