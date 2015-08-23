using System.IO;
using System.Reflection;

using Aliencube.Scissorhands.Tests.Models;

using NUnit.Framework;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Aliencube.Scissorhands.Tests
{
    [TestFixture]
    public class YamlTest
    {
        private const string ConfigYml = "Aliencube.Scissorhands.Tests.config.yml";

        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void Test()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(ConfigYml))
            using (var reader = new StreamReader(stream))
            {
                var deserialiser = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                var settings = deserialiser.Deserialize<Settings>(reader);
            }
        }
    }
}