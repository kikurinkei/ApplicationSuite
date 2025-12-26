using System;
using System.Collections.Generic;
using ApplicationSuite.Runtime.Registries._Base;
using ApplicationSuite.WindowModules.AppShared.Base;

namespace ApplicationSuite.Runtime.Windowing
{
    /// <summary>
    /// Window インスタンスのレジストリに「親（Primary）↔子（Secondary）」の薄いリンクを追加。
    /// </summary>
    public class WindowRegistry : Registries._Base.OneLevelRegistryBase<IShell>
    {
        private static WindowRegistry _instance;
        public static WindowRegistry Instance
        {
            get { return _instance ?? (_instance = new WindowRegistry()); }
        }

        private WindowRegistry() { }

        // childId → parentId / parentId → childId（Manualは1対1想定）
        private readonly Dictionary<string, string> _childToParent = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _parentToChild = new Dictionary<string, string>();

        public void LinkParentChild(string childWindowId, string parentWindowId)
        {
            if (string.IsNullOrEmpty(childWindowId) || string.IsNullOrEmpty(parentWindowId)) return;

            _childToParent[childWindowId] = parentWindowId;
            _parentToChild[parentWindowId] = childWindowId;

            Console.WriteLine("[WindowRegistry] Link Parent-Child: parent=" + parentWindowId + " → child=" + childWindowId);
        }

        public void UnlinkByChild(string childWindowId)
        {
            if (string.IsNullOrEmpty(childWindowId)) return;

            string parentId;
            if (_childToParent.TryGetValue(childWindowId, out parentId))
            {
                _childToParent.Remove(childWindowId);

                string currentChild;
                if (!string.IsNullOrEmpty(parentId) &&
                    _parentToChild.TryGetValue(parentId, out currentChild) &&
                    string.Equals(currentChild, childWindowId, StringComparison.Ordinal))
                {
                    _parentToChild.Remove(parentId);
                }
                Console.WriteLine("[WindowRegistry] Unlink: child=" + childWindowId + " (parent=" + parentId + ")");
            }
        }

        public bool TryGetChildOfParent(string parentWindowId, out string childWindowId)
        {
            childWindowId = null;
            if (string.IsNullOrEmpty(parentWindowId)) return false;

            string child;
            if (_parentToChild.TryGetValue(parentWindowId, out child))
            {
                childWindowId = child;
                return !string.IsNullOrEmpty(childWindowId);
            }
            return false;
        }

        public bool TryGetParentOfChild(string childWindowId, out string parentWindowId)
        {
            parentWindowId = null;
            if (string.IsNullOrEmpty(childWindowId)) return false;

            string parent;
            if (_childToParent.TryGetValue(childWindowId, out parent))
            {
                parentWindowId = parent;
                return !string.IsNullOrEmpty(parentWindowId);
            }
            return false;
        }
    }
}
