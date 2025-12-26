using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationSuite.AppGenerator.Configuration
{
    public static class ConfigStore
    {
        private static Dictionary<string, ElementDetail> _suite = new();
        private static Dictionary<string, ElementDetail> _shell = new();
        private static Dictionary<string, ElementDetail> _utility = new();

        public static void SetSuiteElements(Dictionary<string, ElementDetail> value)
        {
            _suite = new(value);
        }

        public static void SetShellElements(Dictionary<string, ElementDetail> value)
        {
            _shell = new(value);
        }

        public static void SetUtilityElements(Dictionary<string, ElementDetail> value)
        {
            _utility = new(value);
        }

        public static Dictionary<string, ElementDetail> GetSuiteElements()
        {
            return new(_suite); // 防御的コピー
        }

        public static Dictionary<string, ElementDetail> GetShellElements()
        {
            return new(_shell);
        }

        public static Dictionary<string, ElementDetail> GetUtilityElements()
        {
            return new(_utility);
        }

        public static ElementDetail? GetSuiteElement(string id) =>
            _suite.TryGetValue(id, out var detail) ? detail : null;

        public static ElementDetail? GetShellElement(string id) =>
            _shell.TryGetValue(id, out var detail) ? detail : null;

        public static ElementDetail? GetUtilityElement(string id) =>
            _utility.TryGetValue(id, out var detail) ? detail : null;
    }
}
