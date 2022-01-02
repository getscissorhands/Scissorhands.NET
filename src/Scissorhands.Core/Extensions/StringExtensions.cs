using Markdig;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Scissorhands.Core.Extensions
{
    /// <summary>
    /// This represents the extension entity for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        private static IDeserializer deserialiser = new DeserializerBuilder()
                                                       .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                                       .Build();

        private static MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                                                      .UseAdvancedExtensions()
                                                      .Build();

        /// <summary>
        /// Deserialise YAML to <see cref="FrontMatter"/>.
        /// </summary>
        /// <param name="yaml">The YAML document.</param>
        /// <returns>Returns the <see cref="FrontMatter"/> object.</returns>
        public static FrontMatter ToFrontMatter(this string yaml)
        {
            var result = deserialiser.Deserialize<FrontMatter>(yaml);

            return result;
        }

        /// <summary>
        /// Converts the markdown to HTML.
        /// </summary>
        /// <param name="markdown">The markdown document.</param>
        /// <returns>Returns the HTML document.</returns>
        public static string ToHtml(this string markdown)
        {
            var result = Markdown.ToHtml(markdown, pipeline);

            return result;
        }
    }
}