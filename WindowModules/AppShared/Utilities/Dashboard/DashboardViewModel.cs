using ApplicationSuite.AppGenerator.Activation.Models;
using ApplicationSuite.WindowModules.AppShared.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.Dashboard
{

    /// <summary>
    /// Dashboard の ViewModel。
    /// 目的：
    ///   - DashboardEntry 一覧を提供（Registry から取得→Items へ再詰め）。
    ///   - SelectedItem 変更時に Composite.CurrentContentViewModel を切り替える。
    /// ポイント：
    ///   - コンストラクタは使わない（初期化は InitializeFromSetting / OnSelected）。
    ///   - LINQ/ラムダ/デリゲートは不使用（foreach/for で明示的に実装）。
    /// </summary>
    public sealed class DashboardViewModel : BaseViewModel, ISelectedAware
    {
        public DashboardViewModel() { }

        // ------------------------------

        private ICompositeViewModel? _composite; // Composite 参照は明示注入（SetCompositeViewModel）

        /// <summary>
        /// Dashboard の表示用エントリ一覧。
        /// ObservableCollection にして、Items の差し替えではなく「中身のクリア+追加」で更新する。
        /// </summary>
        public ObservableCollection<DashboardEntry> Items { get; } = new();

        private DashboardEntry? _selectedItem;
        /// <summary>
        /// ユーザーが選択したエントリ。
        /// Setter で副作用（Composite.CurrentContentViewModel の切替）を行う。
        /// </summary>
        public DashboardEntry? SelectedItem
        {
            get => _selectedItem;
            set
            {
                // SetProperty：値の差異を判定→変更通知を自動送出（BaseViewModel）
                if (SetProperty(ref _selectedItem, value))
                {
                    // 副作用：選択時にコンテンツを切り替える
                    if (value != null && _composite != null)
                    {
                        // 1) 右側のコンテンツを切り替え
                        // LINQ 不使用：AVM から安全に取り出す（存在チェックあり）
                        BaseViewModel? next;
                        if (_composite.AVM.TryGetValue(value.ElementId, out next))
                        {
                            _composite.CurrentContentViewModel = next;
                        }
                        else
                        {
                            // 必要に応じてログを入れる（本稿では最小のため未実装）
                        }

                        // 2) 左ナビ（NavigationList）の選択も同期する
                        //    - ここでは片方向同期（Dashboard -> NavigationList）
                        //    - 同じVMを再設定しても、Composite 側は「同一参照なら無処理」なのでループしない
                        BaseViewModel? navBase;
                        if (_composite.AVM.TryGetValue("NavigationList", out navBase))
                        {
                            var nav = navBase as ApplicationSuite.WindowModules.AppShared.Utilities.NavigationList.NavigationListViewModel;
                            if (nav != null)
                            {
                                // NavigationList の Items は Registry から取得できる
                                var items = ApplicationSuite.Runtime.Registries.NavigationListRegistry.Instance.Get(this.WindowUniqueId);
                                if (items != null)
                                {
                                    // LINQ 不使用で ElementId 一致を探索
                                    //WApplicationSuite.Core.Builders.NavigationListItem? match = null;
                                    ApplicationSuite.AppGenerator.Activation.Models.NavigationListItem? match = null;

                                    for (int i = 0; i < items.Count; i++)
                                    {
                                        var it = items[i];
                                        if (it != null && it.ElementId == value.ElementId)
                                        {
                                            match = it;
                                            break;
                                        }
                                    }

                                    // 一致が見つかった場合だけ SelectedItem を反映
                                    if (match != null)
                                    {
                                        nav.SelectedItem = match; // 片方向同期（見た目が合う）
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将来：Dashboard 自身を一覧から除外したい場合のスイッチ（初期は false）。
        /// true にすると、ElementId == "Dashboard" を除外して Items を構築。
        /// </summary>
        public bool ExcludeSelf { get; set; } = false;

        // ------------------------------
        // 初期化フック（コンストラクタ不使用方針）
        // ------------------------------

        /// <summary>
        /// WindowPropertyInjector から注入される初期化メソッド。
        /// ここでは WindowUniqueId を受け取り、必要なら軽い初期化のみ行う。
        /// 重い処理や一覧の再構築は OnSelected に譲る。
        /// </summary>
        public void InitializeFromSetting(string windowUniqueId)
        {
            this.WindowUniqueId = windowUniqueId; // 参照用途（ログ等）
                                                  // ※ ここでは一覧の構築は行わない（OnSelected で行う）。
        }

        /// <summary>
        /// Composite 側で当画面が "表示対象" になったときに呼ばれる。
        /// 役割：DashboardEntriesRegistry から一覧を取得→Items を再構築。
        /// </summary>
        public void OnSelected(string windowUniqueId, string elementId)
        {
            this.WindowUniqueId = windowUniqueId;

            // 一覧を再構築（LINQ 不使用）
            Items.Clear();

            // Registry から List<DashboardEntry> を取得
            //var list = WindowApplicationSuite.Core.Registries.DashboardEntriesRegistry.Get(windowUniqueId);
            var list = ApplicationSuite.Runtime.Registries.DashboardEntriesRegistry.Get(windowUniqueId);

            // ※ 初期は Self（"Dashboard"）を含める。
            //    将来、ExcludeSelf == true なら除外する IF を適用。
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];

                if (ExcludeSelf)
                {
                    // 将来用：Dashboard 自身を除外する場合の条件
                    if (entry != null && entry.ElementId == "Dashboard")
                    {
                        continue; // 除外
                    }
                }

                Items.Add(entry);
            }

            // 重要：ここで自動選択は行わない。
            // 既に Composite.CurrentContentViewModel は Dashboard を指している前提のため、
            // 二重に切り替わる（チラつく）可能性を避ける。
        }

        /// <summary>
        /// Composite 参照の注入。
        /// NavigationList と同様に、外部から現在の Composite を与えるための口。
        /// </summary>
        public void SetCompositeViewModel(ICompositeViewModel composite)
        {
            _composite = composite;
        }
    }
}