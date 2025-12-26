using ApplicationSuite.Runtime.Pairing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationSuite.Runtime.Registries;
using ApplicationSuite.WindowModules.AppShared.Base;
namespace ApplicationSuite.AppGenerator.Activation
{
    /// <summary>
    /// PairingHub の "1回だけ初期化" 用ビルダー。
    /// - PrimaryShellBuildPipeline から毎回呼ばれても、2回目以降は何もしない。
    /// - 同時オープン前提は薄いので lock は入れない（必要になったら追加）。
    /// </summary>
    public static class PairingHubBuilder
    {
        public static void EnsureBuilt()
        {
            // 既に作られていれば終了
            if (PairingRegistry.Instance.Contains(PairingRegistry.GlobalKey)) return;

            // Hub（入口）と Relay（配達係）を作る
            var hub = new PairingHub
            {
                ManualJumpRelay = new ManualJumpRelay()
            };

            // SSOT 登録（Key=GLOBAL）
            PairingRegistry.Instance.Register(PairingRegistry.GlobalKey, hub);
        }
    }
}

