using System.IO;
using System.Reflection;

using Aliencube.Scissorhands.Services.Configs;

using FluentAssertions;

using NUnit.Framework;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for YAML.
    /// </summary>
    [TestFixture]
    public class YamlTest
    {
        private const string ConfigYml = "Aliencube.Scissorhands.Services.Tests.config.yml";

        /// <summary>
        /// Initialises all resources for test.
        /// </summary>
        [SetUp]
        public void Init()
        {
        }

        /// <summary>
        /// Cleans up all unused resources.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
        }

        /// <summary>
        /// Tests whether <c>config.yml</c> is converted into <see cref="Settings" /> instance or not.
        /// </summary>
        [Test]
        public void ConfigYmlShouldBeConvertedIntoSettings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(ConfigYml))
            using (var reader = new StreamReader(stream))
            {
                var deserialiser = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                var settings = deserialiser.Deserialize<YamlSettings>(reader);
                settings.Directories.Themes.Should().Be("Themes");
            }
        }
    }
}