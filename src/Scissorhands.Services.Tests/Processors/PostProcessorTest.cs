using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Processors;
using Aliencube.Scissorhands.Services.Wrappers;

using FluentAssertions;

using Moq;

using NUnit.Framework;

namespace Aliencube.Scissorhands.Services.Tests.Processors
{
    /// <summary>
    /// This represents the test entity for the <see cref="PostProcessor" /> class.
    /// </summary>
    [TestFixture]
    public class PostProcessorTest
    {
        private Mock<IYamlSettings> _settings;
        private Mock<IMarkdownWrapper> _md;
        private Mock<IPublishHelper> _helper;
        private IPostProcessor _processor;

        /// <summary>
        /// Initialises all resources for tests.
        /// </summary>
        [SetUp]
        public void Init()
        {
            this._settings = new Mock<IYamlSettings>();
            this._md = new Mock<IMarkdownWrapper>();
            this._helper = new Mock<IPublishHelper>();
        }

        /// <summary>
        /// Release all resources after tests.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
        }

        /// <summary>
        /// Test whether <see cref="ArgumentNullException" /> is thrown when null parameter is passed.
        /// </summary>
        [Test]
        public void GivenNullParameterShouldThrowArgumentNullException()
        {
            Action action = () => this._processor = new PostProcessor(null, this._md.Object, this._helper.Object);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._processor = new PostProcessor(this._settings.Object, null, this._helper.Object);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._processor = new PostProcessor(this._settings.Object, this._md.Object, null);
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether template is returned or not.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        [Test]
        [TestCase("default")]
        [TestCase("second")]
        public void GivenTemplateNameShouldReturnTemplate(string themeName)
        {
            var themes = new List<Theme>()
                             {
                                 new Theme() { Name = "default", Master = "master.cshtml" },
                                 new Theme() { Name = "second", Master = "master.cshtml" }
                             };
            var theme = themes.Single(p => p.Name.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
            this._settings.SetupGet(p => p.Themes).Returns(themes);

            var themeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");
            this._settings.SetupGet(p => p.ThemeBasePath).Returns(themeBasePath);

            var filepath = Path.Combine(this._settings.Object.ThemeBasePath, theme.Name, theme.Master);
            var contents = ProcessorHelper.ReadFile(filepath);

            this._helper.Setup(p => p.Read(It.IsAny<string>())).Returns(contents);

            this._processor = new PostProcessor(this._settings.Object, this._md.Object, this._helper.Object);

            var template = this._processor.GetTemplate(themeName);
            template.Should().NotBeNullOrWhiteSpace();
            template.Should().Be(contents);
        }

        /// <summary>
        /// Tests whether template is returned or not.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        [Test]
        [TestCase("default")]
        [TestCase("second")]
        public async void GivenTemplateNameShouldReturnTemplateAsync(string themeName)
        {
            var themes = new List<Theme>()
                             {
                                 new Theme() { Name = "default", Master = "master.cshtml" },
                                 new Theme() { Name = "second", Master = "master.cshtml" }
                             };
            var theme = themes.Single(p => p.Name.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
            this._settings.SetupGet(p => p.Themes).Returns(themes);

            var themeBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes");
            this._settings.SetupGet(p => p.ThemeBasePath).Returns(themeBasePath);

            var filepath = Path.Combine(this._settings.Object.ThemeBasePath, theme.Name, theme.Master);
            var contents = await ProcessorHelper.ReadFileAsync(filepath);

            this._helper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(contents));

            this._processor = new PostProcessor(this._settings.Object, this._md.Object, this._helper.Object);

            var template = await this._processor.GetTemplateAsync(themeName);
            template.Should().NotBeNullOrWhiteSpace();
            template.Should().Be(contents);
        }

        /// <summary>
        /// Tests whether the given Markdown string is converted to HTML or not.
        /// </summary>
        /// <param name="markdown">
        /// The Markdown string.
        /// </param>
        [Test]
        [TestCase("# Hello World #")]
        public void GivenMarkdownShouldReturnPostInHtml(string markdown)
        {
            var postpath = "abc.md";
            var converted = ProcessorHelper.ConvertMarkdownToHtml(markdown);

            this._md.Setup(p => p.Transform(It.IsAny<string>())).Returns(converted);
            this._helper.Setup(p => p.Read(It.IsAny<string>())).Returns(markdown);

            this._processor = new PostProcessor(this._settings.Object, this._md.Object, this._helper.Object);
            
            var contents = this._processor.GetPost(postpath);
            contents.Should().NotBeNullOrWhiteSpace();
            contents.Should().Be(converted);
        }

        /// <summary>
        /// Tests whether the given Markdown string is converted to HTML or not.
        /// </summary>
        /// <param name="markdown">
        /// The Markdown string.
        /// </param>
        [Test]
        [TestCase("# Hello World #")]
        public async void GivenMarkdownShouldReturnPostInHtmlAsync(string markdown)
        {
            var postpath = "abc.md";
            var converted = ProcessorHelper.ConvertMarkdownToHtml(markdown);

            this._md.Setup(p => p.Transform(It.IsAny<string>())).Returns(converted);
            this._helper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(markdown));

            this._processor = new PostProcessor(this._settings.Object, this._md.Object, this._helper.Object);

            var contents = await this._processor.GetPostAsync(postpath);
            contents.Should().NotBeNullOrWhiteSpace();
            contents.Should().Be(converted);
        }
    }
}