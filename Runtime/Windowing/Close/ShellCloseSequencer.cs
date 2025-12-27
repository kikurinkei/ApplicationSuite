using ApplicationSuite.Runtime.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.Runtime.Windowing.Close
{
    /// <summary>
    /// クローズ時の「実務（レジストリ掃除等）」を司る static クラス
    /// </summary>
    public static class ShellCloseSequencer
    {
        public static bool PrepareForShutdown(string windowUniqueId)
        {
            // IDが空なら、やることはないので完了として戻す
            if (string.IsNullOrEmpty(windowUniqueId)) return true;

            try
            {
                // ---------------------------------------------------------
                // 【将来の予約】セカンダリウインドウのクローズ連動処理
                // 次回の改修にて、ここにセカンダリの特定とクローズ命令を記述します。
                // ---------------------------------------------------------

                // 1. 各種レジストリ・シングルトンから「参照」を削除
                // 本体はGCに任せ、名簿から消すことに徹します。
                ViewModelRegistry.Instance.RemoveAll(windowUniqueId);
                UserControlRegistry.Instance.RemoveAll(windowUniqueId);
                WindowRegistry.Instance.Remove(windowUniqueId);

                // 2. コンポジット（紐付け関係）の解消
                // プライマリが消える際、紐付いていたセカンダリ側の参照もここで断つ
                CompositeViewModelRegistry.Instance.Remove(windowUniqueId);
                NavigationListRegistry.Instance.Remove(windowUniqueId);

                // 全ての手順が正常に完了
                return true;
            }
            catch (Exception ex)
            {
                // 失敗した場合は false を返し、Shell 側に異常を知らせる
                // 必要に応じてここに Logger.Error(ex) などを記述
                return false;
            }
        }
    }
}