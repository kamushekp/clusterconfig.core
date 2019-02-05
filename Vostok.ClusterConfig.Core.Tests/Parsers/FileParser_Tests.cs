using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Core.Parsers;
using Vostok.ClusterConfig.Core.Parsers.Content;
using Vostok.ClusterConfig.Core.Tests.Helpers;
using Vostok.Configuration.Abstractions.SettingsTree;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.ClusterConfig.Core.Tests.Parsers
{
    [TestFixture]
    internal class FileParser_Tests
    {
        private TemporaryDirectory directory;
        private FileParserSettings settings;
        private FileParser parser;

        private IFileContentParser defaultParser;
        private IFileContentParser customParser;

        private ISettingsNode defaultResult;
        private ISettingsNode customResult;

        private string file;

        [SetUp]
        public void TestSetup()
        {
            directory = new TemporaryDirectory();

            file = Guid.NewGuid().ToString();

            defaultParser = Substitute.For<IFileContentParser>();
            defaultParser.Parse(default, default).ReturnsForAnyArgs(defaultResult = Substitute.For<ISettingsNode>());

            customParser = Substitute.For<IFileContentParser>();
            customParser.Parse(default, default).ReturnsForAnyArgs(customResult = Substitute.For<ISettingsNode>());

            settings = new FileParserSettings
            {
                DefaultParser = defaultParser,
                CustomParsers = new Dictionary<string, IFileContentParser>
                {
                    [".custom"] = customParser
                },
                MaximumFileSize = 1000
            };

            parser = new FileParser(settings);
        }

        [TearDown]
        public void TearDown()
        {
            directory.Dispose();
        }

        [Test]
        public void Should_return_null_if_file_does_not_exist()
        {
            Parse().Should().BeNull();
        }

        [Test]
        public void Should_return_null_if_file_is_too_large()
        {
            WriteFile(settings.MaximumFileSize + 1);

            Parse().Should().BeNull();
        }

        [Test]
        public void Should_use_default_parser_if_no_custom_parser_fits()
        {
            WriteFile(1);

            Parse().Should().BeSameAs(defaultResult);
        }

        [Test]
        public void Should_use_custom_parser_if_file_has_appropriate_extension()
        {
            file += ".CUSTOM";

            WriteFile(1);

            Parse().Should().BeSameAs(customResult);
        }

        [Test]
        public void Should_pass_lowercased_file_name_to_parser()
        {
            file = "UPPERCASE";

            WriteFile(1);

            Parse();

            defaultParser.Received(1).Parse("uppercase", Arg.Any<IFileContent>());
        }

        [Test]
        public void Should_be_able_to_read_an_already_opened_file()
        {
            WriteFile(1);

            using (new FileStream(Path.Combine(directory.Path, file), FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                Parse().Should().BeSameAs(defaultResult);
            }
        }

        private void WriteFile(int size)
            => File.WriteAllBytes(Path.Combine(directory.Path, file), new byte[size]);

        private ISettingsNode Parse()
            => parser.Parse(new FileInfo(Path.Combine(directory.Path, file)));
    }
}