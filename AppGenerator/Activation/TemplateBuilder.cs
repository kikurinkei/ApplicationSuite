using ApplicationSuite.AppGenerator.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationSuite.AppGenerator.Activation
{

    /// <summary> WindowBuildCconductor
    /// ViewModelType / ViewType の文字列から Type を解決し、
    /// 動的な DataTemplate を構築するユーティリティクラス。
    /// </summary>
    public static class TemplateBuilder
    {
        public static List<string> UIList = new List<string>();

        /// <summary>
        /// 指定された型名から DataTemplate を生成する。
        /// vmTypeName: ViewModel の完全修飾名（例："Namespace.ViewModels.MyViewModel"）
        /// viewTypeName: View（UserControl）の完全修飾名（例："Namespace.Views.MyView"）
        /// 戻り値: 生成された DataTemplate。失敗した場合は null。
        /// </summary>
        //----- 利用 -----
        public static void BuildAndRegister(Dictionary<string, UtilityMetaInfo> childIds)
        {
            // DataTemplate を登録
            // １.ConfigStore.GetUtilityElements() から要素を取得
            //Dictionary<string, ElementDetail> _value = ConfigStore.GetUtilityElements();

            // ２.BitTargetFilter を使用して、特定の条件に合致する要素をフィルタリング
            //Dictionary<string, ElementDetail> _valu =
            //    BitTargetFilter.BitTargetFilter_DicForeach(_value, 4);

            // ３.ElementDetailConfirm を使用して、テンプレート登録の妥当性を確認と登録



            //if (_valu != null && _valu.Count > 0)// データが存在 → 判定メソッドへ
            //{
            //    ElementDetailConfirm.IsValidTemplateTarget(_valu);
            //}
            //else
            //{
            //    Console.WriteLine("isnull or 0");
            //}





            foreach (var kv in childIds)
            {
                UtilityMetaInfo detail = kv.Value; // KeyValuePair から Value を取得

                if (UIList.Contains(detail.UIElementId))
                {
                    Console.WriteLine($"[TemplateRegistrationHelper] 重複検出: {detail.UIElementId}");
                    continue;

                }




                if (string.IsNullOrEmpty(detail.UIViewModelPath))
                {
                    Console.WriteLine($"[TemplateRegistrationHelper] ViewModelType が未設定: {detail.UIElementId}");
                    continue;
                }
                if (string.IsNullOrEmpty(detail.UIControlPath))
                {
                    Console.WriteLine($"[TemplateRegistrationHelper] ViewType が未設定: {detail.UIElementId}");
                    continue;
                }
                RegisterTemplate(detail); // ②を即実行
            }
        }

        /// <summary>
        /// 指定された ElementDetail に基づいて DataTemplate を構築し、
        /// Application.Current.Resources に追加登録する。
        /// </summary>
        public static void RegisterTemplate(UtilityMetaInfo detail)
        {
            // Type を文字列から解決
            Type? vmType = Type.GetType(detail.UIViewModelPath);
            Type? viewType = Type.GetType(detail.UIControlPath);

            if (vmType == null)
            {
                Console.WriteLine($"[TemplateRegistrationHelper] ViewModel Type 解決失敗: {detail.UIViewModelPath}");
                return;
            }
            if (viewType == null)
            {
                Console.WriteLine($"[TemplateRegistrationHelper] - View Type -  解決失敗: {detail.UIControlPath}");
                return;
            }
            // テンプレート生成
            DataTemplate template = new()
            {
                DataType = vmType,
                VisualTree = new FrameworkElementFactory(viewType)
            };
            // Resources に登録
            Application.Current.Resources.Add(new DataTemplateKey(vmType), template);
            // UIList に登録
            UIList.Add(detail.UIElementId);

            Console.WriteLine($"[TemplateRegistrationHelper] +++ テンプレート登録 +++: {detail.UIViewModelPath}");
            Console.WriteLine($"[TemplateRegistrationHelper] +++ テンプレート登録 +++: {detail.UIControlPath}");
            Console.WriteLine($"[TemplateRegistrationHelper] テンプレート登録: {vmType.Name} → {viewType.Name}");
        }







    }
}
