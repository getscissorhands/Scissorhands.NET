using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Scissorhands.Core.Tests.Utilities
{
    public class TestSettings
    {
        public virtual string? ProjectDirectory { get; set; }

        public static async Task<TestSettings> LoadAsync()
        {
            var settings = default(TestSettings);

            using (var stream = new FileStream("testsettings.json", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                settings = JsonConvert.DeserializeObject<TestSettings>(json);
            }

            return settings;
        }
    }
}