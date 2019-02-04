using System;
using System.Linq;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Sources.SettingsTree;

namespace Vostok.ClusterConfig.Core.Tests
{
    internal class TreeBuilder
    {
        private ISettingsNode tree;

        public ISettingsNode Build() => tree;

        public TreeBuilder Add(ClusterConfigPath path, string value)
            => Add(TreeFactory.CreateTreeByMultiLevelKey(null, path.Segments.ToArray(), value));

        public TreeBuilder Add(ClusterConfigPath path, string[] values)
            => Add(TreeFactory.CreateTreeByMultiLevelKey(null, path.Segments.ToArray(), values));

        private TreeBuilder Add(ISettingsNode tree)
        {
            this.tree = SettingsNodeMerger.Merge(this.tree, tree, SettingsMergeOptions.Default);

            return this;
        }
    }
}
