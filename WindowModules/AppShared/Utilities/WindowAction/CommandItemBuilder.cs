using ApplicationSuite.AppGenerator.Configuration;
using ApplicationSuite.Runtime.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApplicationSuite.WindowModules.AppShared.Utilities.WindowAction.WindowActionViewModel;

namespace ApplicationSuite.WindowModules.AppShared.Utilities.WindowAction
{
    /// <summary>
    /// Window用のコマンド項目(CommandItem)を構築するヘルパークラス。
    /// Shell構成のID一覧から OPEN コマンド、実行中Window一覧から CLOSE コマンドを構築する。
    /// </summary>
    public static class CommandItemBuilder
    {
        public static List<CommandItem> Build()
        {
            var result = new List<CommandItem>();

            var shellElements = ConfigStore.GetShellElements();

            // 1. SHUTDOWN を最上位に固定追加
            result.Add(new CommandItem
            {
                DisplayText = " - SHUTDOWN AllWindows",
                FirstCommand = "SHUTDOWN",
                FirstTargetName = "ALL"
            });

            // 2. Shell構成一覧から RESTART コマンドを追加
            foreach (var kv in shellElements)
            {
                var id = kv.Key;
                result.Add(new CommandItem
                {
                    DisplayText = $" - RESTART {id}",
                    FirstCommand = "RESTART",
                    FirstTargetName = id
                });
            }

            // 3. 実行中ウインドウ一覧から CLOSE コマンドを動的に生成
            var currentIds = WindowRegistry.Instance.GetKeys();
            foreach (var id in currentIds)
            {
                result.Add(new CommandItem
                {
                    DisplayText = $" - CLOSE {id}",
                    FirstCommand = "CLOSE",
                    FirstTargetName = id
                });
            }

            // 4. Shell構成一覧から OPEN コマンドを追加
            foreach (var kv in shellElements)
            {
                var id = kv.Key;
                result.Add(new CommandItem
                {
                    DisplayText = $" - OPEN {id}",
                    FirstCommand = "OPEN",
                    FirstTargetName = id
                });
            }

            return result;
        }
    }
}