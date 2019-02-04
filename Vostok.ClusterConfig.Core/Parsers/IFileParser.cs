﻿using System.IO;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal interface IFileParser
    {
        [CanBeNull]
        ISettingsNode Parse([NotNull] FileInfo file);
    }
}