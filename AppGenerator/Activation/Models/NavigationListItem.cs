using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Activation.Models
{
    // ======================================
    // サイドバーに表示する項目のデータ構造
    // ======================================
    /// <summary>
    /// ナビゲーションリストに表示する1項目の情報を格納するモデル
    /// </summary>
    public class NavigationListItem
    {
        /// <summary>
        /// CompositeViewModel に登録されている ViewModel のキー
        /// </summary>
        public string ElementId { get; set; } = string.Empty;

        /// <summary>
        /// サイドバーに表示する名前（ElementDetail.ElementName）
        /// </summary>
        //public string DisplayName { get; set; } = string.Empty;
        public string ElementName { get; set; } = string.Empty;

        /// <summary>
        /// アイコン画像のパス（未使用）
        /// </summary>
        public string? IconPath { get; set; } = null;
    }
}