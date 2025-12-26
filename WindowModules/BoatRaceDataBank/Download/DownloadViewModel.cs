using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ApplicationSuite.AppShared.Commands;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Download
{
    /// <summary>
    /// トグル（RP/RR）で切替。切替時は内部で初期化→Active切替。
    /// 表示は共有（TextBlock 1組）／値はSideStateで分離。Processor/Sessionは既存を利用。
    /// </summary>
    public class DownloadViewModel : BaseViewModel
    {
        // ========== 画面テキスト ==========
        private string _firstText = string.Empty;  // 上：対象（URL一覧など）
        public string FirstText { get => _firstText; set => SetProperty(ref _firstText, value); }

        private string _secondText = string.Empty; // 下：結果ログ
        public string SecondText { get => _secondText; set => SetProperty(ref _secondText, value); }

        // ========== 操作 ==========
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

        // ========== Active ==========
        public enum ActiveKind { RP, RR }

        private ActiveKind _active = ActiveKind.RP; // 既定：RP
        public ActiveKind Active
        {
            get => _active;
            private set
            {
                if (SetProperty(ref _active, value))
                {
                    RebindShared(); // 共有表示を現Activeの値で更新
                }
            }
        }

        // トグル用（コードビハインドなしで切替＆初期化を行う）
        public bool IsRpActive
        {
            get => Active == ActiveKind.RP;
            set
            {
                if (value && Active != ActiveKind.RP)
                {
                    ResetOnToggle();    // 表示をクリア
                    Active = ActiveKind.RP;
                }
            }
        }

        public bool IsRrActive
        {
            get => Active == ActiveKind.RR;
            set
            {
                if (value && Active != ActiveKind.RR)
                {
                    ResetOnToggle();    // 表示をクリア
                    Active = ActiveKind.RR;
                }
            }
        }

        // ========== 内部状態：SideState（RP/RRで分離保持） ==========
        private sealed class SideState
        {
            public DateOnly? LastDate { get; set; }
            public IReadOnlyList<DateOnly> TargetDates { get; set; } = Array.Empty<DateOnly>();
            public IReadOnlyList<UriComposer.UriItem> Uris { get; set; } = Array.Empty<UriComposer.UriItem>();
            public string SaveRoot { get; set; } = ""; // 必要に応じて上書き
        }

        private readonly SideState _rp = new() { SaveRoot = @"C:\BoatRaceDataBank\LZH\B" };
        private readonly SideState _rr = new() { SaveRoot = @"C:\BoatRaceDataBank\LZH\K" };

        private SideState Current => Active == ActiveKind.RP ? _rp : _rr;

        // ========== 共有表示（TextBlock 1組） ==========
        public string SharedLastDateText =>
            (Current.LastDate.HasValue ? Current.LastDate.Value.ToString("yyyy-MM-dd") : "（なし）");

        private string _sharedRangePreview = string.Empty;
        public string SharedRangePreview
        {
            get => _sharedRangePreview;
            private set => SetProperty(ref _sharedRangePreview, value);
        }

        private void RebindShared()
        {
            OnPropertyChanged(nameof(SharedLastDateText));
            SharedRangePreview = BuildRangePreview(Current.TargetDates);
            // 上の表示（URL一覧）も、Active側のUrisを反映
            FirstText = Current.Uris.Count == 0 ? "（対象なし）"
                                                : string.Join("\n", Current.Uris.Select(u => u.UriString));
        }

        private static string BuildRangePreview(IReadOnlyList<DateOnly> dates)
        {
            if (dates == null || dates.Count == 0) return "最新です";
            var start = dates[0];
            var end = dates[^1];
            return $"{start:yyyy-MM-dd}..{end:yyyy-MM-dd} ({dates.Count}d)";
        }

        // ========== 保存先（必要なら外部から変更可） ==========
        public string SaveRootRp { get => _rp.SaveRoot; set { _rp.SaveRoot = value; if (Active == ActiveKind.RP) RebindShared(); } }
        public string SaveRootRr { get => _rr.SaveRoot; set { _rr.SaveRoot = value; if (Active == ActiveKind.RR) RebindShared(); } }

        // ========== 依存 ==========
        private readonly DownloadProcessor _processor = new();

        // ========== 初期化 ==========
        public void InitializeFromSetting(string windowUniqueId)
        {
            WindowUniqueId = windowUniqueId;

            OperationItems.Clear();
            OperationItems.Add(new OperationItem("初期化", "Initialize"));
            OperationItems.Add(new OperationItem("DB問い合わせ", "CheckDbHead"));       // 共通：Active側に作用
            OperationItems.Add(new OperationItem("ダウンロードURI作成", "BuildUri"));  // 共通
            OperationItems.Add(new OperationItem("ダウンロード", "FetchArchive"));      // 共通

            // 既定はRPで開始、共有表示リフレッシュ
            Active = ActiveKind.RP;
            ResetOnToggle();
        }

        public DownloadViewModel() { }

        // ========== Dispatch ==========
        private void Dispatch(string op)
        {
            switch (op)
            {
                case "Initialize":
                    ResetAll();  // 両側の状態を完全クリア
                    break;

                case "CheckDbHead":
                    DoCheckDbHead();
                    break;

                case "BuildUri":
                    DoBuildUri();
                    break;

                case "FetchArchive":
                    _ = DoFetchArchiveAsync();
                    break;

                default:
                    // フォールバック：上の内容を下に写す
                    SecondText = FirstText;
                    break;
            }
        }

        // ========== Reset ==========
        private void ResetOnToggle()
        {
            // 表示だけ軽く初期化（SideStateは保持）
            FirstText = string.Empty;
            SecondText = string.Empty;
            OnPropertyChanged(nameof(SharedLastDateText));
            SharedRangePreview = string.Empty;
        }

        private void ResetAll()
        {
            // 画面クリア
            FirstText = string.Empty;
            SecondText = string.Empty;

            // 両側の内部状態もクリア
            _rp.LastDate = null; _rp.TargetDates = Array.Empty<DateOnly>(); _rp.Uris = Array.Empty<UriComposer.UriItem>();
            _rr.LastDate = null; _rr.TargetDates = Array.Empty<DateOnly>(); _rr.Uris = Array.Empty<UriComposer.UriItem>();

            // 共有表示更新（現在のActive側を反映）
            RebindShared();

            AppendLog("[INIT] クリアしました。");
        }

        // ========== 共通A：DB問い合わせ（最終日→レンジ計画） ==========
        private void DoCheckDbHead()
        {
            if (Active == ActiveKind.RP)
            {
                var head = _processor.RpCheckDbHead();
                _rp.LastDate = head.Last;
                _rp.TargetDates = head.Plan.Dates;
            }
            else
            {
                var head = _processor.RrCheckDbHead();
                _rr.LastDate = head.Last;
                _rr.TargetDates = head.Plan.Dates;
            }

            RebindShared();
        }

        // ========== 共通B：URI組立（上にURL一覧を出す） ==========
        private void DoBuildUri()
        {
            if (Active == ActiveKind.RP)
            {
                _rp.Uris = _processor.RpBuildUri(_rp.TargetDates);
            }
            else
            {
                _rr.Uris = _processor.RrBuildUri(_rr.TargetDates);
            }

            RebindShared(); // Active側のUrisを上Textに反映
        }

        // ========== 共通C：ダウンロード（下にログ追記） ==========
        private async Task DoFetchArchiveAsync()
        {
            var side = Current;

            if (side.Uris.Count == 0)
            {
                AppendLog("[INFO] ダウンロード対象がありません（まず『ダウンロードURI作成』を実行）");
                return;
            }

            var tag = Active == ActiveKind.RP ? "[RP]" : "[RR]";
            AppendLog($"[START]{tag} {DateTime.Now:yyyy-MM-dd HH:mm:ss} total={side.Uris.Count} throttle=10–20s saveRoot={side.SaveRoot}");

            var summary = (Active == ActiveKind.RP)
                ? await _processor.RpDownloadAsync(side.Uris, side.SaveRoot, line => AppendLog($"{tag} {line}"))
                : await _processor.RrDownloadAsync(side.Uris, side.SaveRoot, line => AppendLog($"{tag} {line}"));

            AppendLog($"[END]{tag} ok={summary.Succeeded}, skip={summary.Skipped}, err={summary.Failed}, elapsed={summary.Elapsed:hh\\:mm\\:ss}");
        }

        // ========== ログ追記 ==========
        private void AppendLog(string line)
        {
            SecondText += (SecondText.Length > 0 ? "\n" : "") + line;
        }
    }

    // 既存：OperationItem（表示名と内部名の薄いDTO）
    public sealed class OperationItem
    {
        public string DisplayText { get; }
        public string OperationName { get; }
        public OperationItem(string displayText, string operationName)
        {
            DisplayText = displayText;
            OperationName = operationName;
        }
    }
}



//using System;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using ApplicationSuite.AppShared.Commands;
//using ApplicationSuite.WindowModules.AppShared.Base;
//namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Download
//{
//    public class DownloadViewModel : BaseViewModel
//    {
//        // === 表示テキスト ===
//        private string _firstText = string.Empty;
//        public string FirstText { get => _firstText; set => SetProperty(ref _firstText, value); }

//        private string _secondText = string.Empty;
//        public string SecondText { get => _secondText; set => SetProperty(ref _secondText, value); }

//        // === 操作 ===
//        public ObservableCollection<OperationItem> OperationItems { get; } = new();

//        private OperationItem? _selectedOperation;
//        public OperationItem? SelectedOperation
//        {
//            get => _selectedOperation;
//            set
//            {
//                if (SetProperty(ref _selectedOperation, value))
//                {
//                    if (_selectedOperation != null)
//                    {
//                        Dispatch(_selectedOperation.OperationName);
//                        _selectedOperation = null;
//                        OnPropertyChanged(nameof(SelectedOperation));
//                    }
//                }
//            }
//        }

//        // === Active 列の共有化フラグ ===
//        public enum ActiveKind { None, RP, RR }
//        private ActiveKind _active = ActiveKind.None;
//        public ActiveKind Active
//        {
//            get => _active;
//            private set
//            {
//                if (SetProperty(ref _active, value))
//                {
//                    // 共有表示の更新
//                    OnPropertyChanged(nameof(SharedLastDateText));
//                    OnPropertyChanged(nameof(SharedRangePreview));
//                }
//            }
//        }

//        // === RP 側の保持（値は別々に持つ）===
//        private DateOnly? _lastDateRP = null;
//        public DateOnly? LastDateRP
//        {
//            get => _lastDateRP;
//            private set
//            {
//                if (SetProperty(ref _lastDateRP, value))
//                {
//                    OnPropertyChanged(nameof(LastDateRPText));
//                    if (Active == ActiveKind.RP)
//                        OnPropertyChanged(nameof(SharedLastDateText));
//                }
//            }
//        }
//        public string LastDateRPText => LastDateRP?.ToString("yyyy-MM-dd") ?? "（なし）";

//        private IReadOnlyList<DateOnly> _targetDatesRP = Array.Empty<DateOnly>();
//        public IReadOnlyList<DateOnly> TargetDatesRP
//        {
//            get => _targetDatesRP;
//            private set
//            {
//                if (SetProperty(ref _targetDatesRP, value))
//                {
//                    OnPropertyChanged(nameof(TargetDatesRPPreview));
//                    if (Active == ActiveKind.RP)
//                        OnPropertyChanged(nameof(SharedRangePreview));
//                }
//            }
//        }
//        private string _targetDatesRPPreview = string.Empty;
//        public string TargetDatesRPPreview
//        {
//            get => _targetDatesRPPreview;
//            private set
//            {
//                if (SetProperty(ref _targetDatesRPPreview, value) && Active == ActiveKind.RP)
//                    OnPropertyChanged(nameof(SharedRangePreview));
//            }
//        }

//        private IReadOnlyList<UriComposer.UriItem> _rpUris = Array.Empty<UriComposer.UriItem>();
//        public IReadOnlyList<UriComposer.UriItem> RpUris { get => _rpUris; private set => SetProperty(ref _rpUris, value); }

//        // === RR 側の保持（値は別々に持つ）===
//        private DateOnly? _lastDateRR = null;
//        public DateOnly? LastDateRR
//        {
//            get => _lastDateRR;
//            private set
//            {
//                if (SetProperty(ref _lastDateRR, value))
//                {
//                    OnPropertyChanged(nameof(LastDateRRText));
//                    if (Active == ActiveKind.RR)
//                        OnPropertyChanged(nameof(SharedLastDateText));
//                }
//            }
//        }
//        public string LastDateRRText => LastDateRR?.ToString("yyyy-MM-dd") ?? "（なし）";

//        private IReadOnlyList<DateOnly> _targetDatesRR = Array.Empty<DateOnly>();
//        public IReadOnlyList<DateOnly> TargetDatesRR
//        {
//            get => _targetDatesRR;
//            private set
//            {
//                if (SetProperty(ref _targetDatesRR, value))
//                {
//                    OnPropertyChanged(nameof(TargetDatesRRPreview));
//                    if (Active == ActiveKind.RR)
//                        OnPropertyChanged(nameof(SharedRangePreview));
//                }
//            }
//        }
//        private string _targetDatesRRPreview = string.Empty;
//        public string TargetDatesRRPreview
//        {
//            get => _targetDatesRRPreview;
//            private set
//            {
//                if (SetProperty(ref _targetDatesRRPreview, value) && Active == ActiveKind.RR)
//                    OnPropertyChanged(nameof(SharedRangePreview));
//            }
//        }

//        private IReadOnlyList<UriComposer.UriItem> _rrUris = Array.Empty<UriComposer.UriItem>();
//        public IReadOnlyList<UriComposer.UriItem> RrUris { get => _rrUris; private set => SetProperty(ref _rrUris, value); }

//        // === 共有表示（TextBlock 1つで表示）===
//        public string SharedLastDateText =>
//            Active == ActiveKind.RP ? LastDateRPText :
//            Active == ActiveKind.RR ? LastDateRRText :
//            "（なし）";

//        public string SharedRangePreview =>
//            Active == ActiveKind.RP ? TargetDatesRPPreview :
//            Active == ActiveKind.RR ? TargetDatesRRPreview :
//            string.Empty;

//        // 保存先（暫定）
//        public string SaveRootRp { get; set; } = @"C:\BoatRaceDataBank\LZH\B";
//        public string SaveRootRr { get; set; } = @"C:\BoatRaceDataBank\LZH\K";

//        // Processor
//        private readonly DownloadProcessor _processor = new();

//        public void InitializeFromSetting(string windowUniqueId)
//        {
//            WindowUniqueId = windowUniqueId;

//            OperationItems.Clear();
//            OperationItems.Add(new OperationItem("初期化", "Initialize"));

//            OperationItems.Add(new OperationItem("DB問い合わせ（RP）", "RP_CheckDbHead"));
//            OperationItems.Add(new OperationItem("ダウンロードURI作成（RP）", "RP_BuildUri"));
//            OperationItems.Add(new OperationItem("ダウンロード（RP）", "RP_FetchArchive"));

//            OperationItems.Add(new OperationItem("DB問い合わせ（RR）", "RR_CheckDbHead"));
//            OperationItems.Add(new OperationItem("ダウンロードURI作成（RR）", "RR_BuildUri"));
//            OperationItems.Add(new OperationItem("ダウンロード（RR）", "RR_FetchArchive"));
//        }

//        public DownloadViewModel() { }

//        private void Dispatch(string op)
//        {
//            switch (op)
//            {
//                case "Initialize":
//                    ResetAll();
//                    break;

//                // RP
//                case "RP_CheckDbHead":
//                    Active = ActiveKind.RP;
//                    DoRpCheckDbHead();
//                    break;
//                case "RP_BuildUri":
//                    Active = ActiveKind.RP;
//                    DoRpBuildUri();
//                    break;
//                case "RP_FetchArchive":
//                    Active = ActiveKind.RP;
//                    _ = DoRpFetchArchiveAsync();
//                    break;

//                // RR
//                case "RR_CheckDbHead":
//                    Active = ActiveKind.RR;
//                    DoRrCheckDbHead();
//                    break;
//                case "RR_BuildUri":
//                    Active = ActiveKind.RR;
//                    DoRrBuildUri();
//                    break;
//                case "RR_FetchArchive":
//                    Active = ActiveKind.RR;
//                    _ = DoRrFetchArchiveAsync();
//                    break;

//                default:
//                    SecondText = FirstText;
//                    break;
//            }
//        }

//        // === Initialize ===
//        private void ResetAll()
//        {
//            FirstText = string.Empty;
//            SecondText = string.Empty;

//            LastDateRP = null;
//            TargetDatesRP = Array.Empty<DateOnly>();
//            TargetDatesRPPreview = string.Empty;
//            RpUris = Array.Empty<UriComposer.UriItem>();

//            LastDateRR = null;
//            TargetDatesRR = Array.Empty<DateOnly>();
//            TargetDatesRRPreview = string.Empty;
//            RrUris = Array.Empty<UriComposer.UriItem>();

//            Active = ActiveKind.None; // 共有表示は自動で（なし／空）になる
//            AppendLog("[INIT] クリアしました。");
//        }

//        // === RP: A ===
//        private void DoRpCheckDbHead()
//        {
//            var result = _processor.RpCheckDbHead();
//            LastDateRP = result.Last;
//            TargetDatesRP = result.Plan.Dates;
//            TargetDatesRPPreview = result.Plan.IsEmpty
//                ? "最新です"
//                : $"{result.Plan.Start:yyyy-MM-dd}..{result.Plan.End:yyyy-MM-dd} ({result.Plan.Dates.Count}d)";
//        }

//        // === RP: B ===
//        private void DoRpBuildUri()
//        {
//            var uris = _processor.RpBuildUri(TargetDatesRP);
//            RpUris = uris;
//            FirstText = uris.Count == 0 ? "（対象なし）" : string.Join("\n", uris.Select(u => u.UriString));
//        }

//        // === RP: C ===
//        private async Task DoRpFetchArchiveAsync()
//        {
//            if (RpUris.Count == 0)
//            {
//                AppendLog("[INFO] ダウンロード対象がありません（まず『ダウンロードURI作成（RP）』を実行）");
//                return;
//            }

//            AppendLog($"[START][RP] {DateTime.Now:yyyy-MM-dd HH:mm:ss} total={RpUris.Count} throttle=10–20s saveRoot={SaveRootRp}");
//            var summary = await _processor.RpDownloadAsync(RpUris, SaveRootRp, line => AppendLog(line));
//            AppendLog($"[END][RP] ok={summary.Succeeded}, skip={summary.Skipped}, err={summary.Failed}, elapsed={summary.Elapsed:hh\\:mm\\:ss}");
//        }

//        // === RR: A ===
//        private void DoRrCheckDbHead()
//        {
//            var result = _processor.RrCheckDbHead();
//            LastDateRR = result.Last;
//            TargetDatesRR = result.Plan.Dates;
//            TargetDatesRRPreview = result.Plan.IsEmpty
//                ? "最新です"
//                : $"{result.Plan.Start:yyyy-MM-dd}..{result.Plan.End:yyyy-MM-dd} ({result.Plan.Dates.Count}d)";
//        }

//        // === RR: B ===
//        private void DoRrBuildUri()
//        {
//            var uris = _processor.RrBuildUri(TargetDatesRR);
//            RrUris = uris;
//            FirstText = uris.Count == 0 ? "（対象なし）" : string.Join("\n", uris.Select(u => u.UriString));
//        }

//        // === RR: C ===
//        private async Task DoRrFetchArchiveAsync()
//        {
//            if (RrUris.Count == 0)
//            {
//                AppendLog("[INFO] ダウンロード対象がありません（まず『ダウンロードURI作成（RR）』を実行）");
//                return;
//            }

//            AppendLog($"[START][RR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} total={RrUris.Count} throttle=10–20s saveRoot={SaveRootRr}");
//            var summary = await _processor.RrDownloadAsync(RrUris, SaveRootRr, line => AppendLog("[RR] " + line));
//            AppendLog($"[END][RR] ok={summary.Succeeded}, skip={summary.Skipped}, err={summary.Failed}, elapsed={summary.Elapsed:hh\\:mm\\:ss}");
//        }

//        // 共通：ログ追記
//        private void AppendLog(string line)
//        {
//            SecondText += (SecondText.Length > 0 ? "\n" : "") + line;
//        }
//    }

//    public sealed class OperationItem
//    {
//        public string DisplayText { get; }
//        public string OperationName { get; }
//        public OperationItem(string displayText, string operationName)
//        {
//            DisplayText = displayText;
//            OperationName = operationName;
//        }
//    }
//}
