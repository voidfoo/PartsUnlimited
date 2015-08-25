// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Framework.Configuration;
using NSubstitute;
using PartsUnlimited.WebsiteConfiguration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PartsUnlimited.Utils
{
    public class ContentDeliveryNetworkConfigurationTests
    {
        [Fact]
        public void ImageContent()
        {
            const string ImageCdn = "some/test/path";

            var config = Substitute.For<IConfiguration>();
            config["images"].Returns(ImageCdn);

            var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

            Assert.Equal(ImageCdn, cdnConfig.Images);
        }

        [Fact]
        public void ImageContentNull()
        {
            var config = Substitute.For<IConfiguration>();
            config["images"].Returns((string)null);
            var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

            Assert.Null(cdnConfig.Images);
        }

        [Fact]
        public void ScriptStylesParse()
        {
            var values = new[]
            {
                new ConfigPathHelper{ Name="item1", Path="path1 path3  path4"},
                new ConfigPathHelper{ Name="item2", Path="path2"},
            };

            var config = Substitute.For<IConfiguration>();
            var scriptsConfig = CreateConfig(values);
            config.GetSection("Scripts").Returns(scriptsConfig);

            var cdnConfig = new ContentDeliveryNetworkConfiguration(config);

            Assert.Equal(2, cdnConfig.Scripts.Count);
            Assert.Equal(new[] { "path1", "path3", "path4" }, cdnConfig.Scripts["item1"]);
            Assert.Equal(new[] { "path2" }, cdnConfig.Scripts["item2"]);
        }

        private IConfiguration CreateConfig(IEnumerable<ConfigPathHelper> values)
        {
            var config = Substitute.For<IConfiguration>();
            var emptyConfig = Substitute.For<IConfiguration>();

            var subkeys = values.Select(v => emptyConfig);

            config.GetChildren().Returns(subkeys);

            foreach (var value in values)
            {
                config[value.Name].Returns(value.Path);
            }

            return config;
        }

        private class ConfigPathHelper
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }
    }
}