using System;
using System.IO;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Models;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PublishHelper" /> class.
    /// </summary>
    [TestFixture]
    public class PublishHelperTest
    {
        private Mock<IYamlSettings> _settings;
        private ITemplateServiceConfiguration _config;
        private IRazorEngineService _engine;
        private IPublishHelper _helper;

        /// <summary>
        /// Initialises all resources for tests.
        /// </summary>
        [SetUp]
        public void Init()
        {
            this._settings = new Mock<IYamlSettings>();
            this._config = new TemplateServiceConfiguration()
                               {
                                   BaseTemplateType = typeof(HtmlTemplate<>),
                                   DisableTempFileLocking = true,
                                   CachingProvider = new DefaultCachingProvider(t => { }),
                               };
            this._engine = RazorEngineService.Create(this._config);
        }

        /// <summary>
        /// Release all resources after tests.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            if (this._helper != null)
            {
                this._helper.Dispose();
            }
        }

        /// <summary>
        /// Tests whether the <see cref="ArgumentNullException" /> is thrown when <c>null</c> parameter is passed.
        /// </summary>
        [Test]
        public void GivenNullParametersShouldThrownArgumentNullException()
        {
            Action action = () => this._helper = new PublishHelper(null, this._engine);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._helper = new PublishHelper(this._settings.Object, null);
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether content can be read from the given filepath or not.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        [Test]
        [TestCase("")]
        [TestCase("config.yml")]
        public void GivenFilePathShouldReturnContent(string filename)
        {
            this._helper = new PublishHelper(this._settings.Object, this._engine);

            string content;
            if (string.IsNullOrWhiteSpace(filename))
            {
                content = this._helper.Read(null);
                content.Should().BeNullOrWhiteSpace();

                return;
            }

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            content = this._helper.Read(filepath);
            content.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests whether content can be read from the given filepath or not.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        [Test]
        [TestCase("")]
        [TestCase("config.yml")]
        public async void GivenFilePathShouldReturnContentAsync(string filename)
        {
            this._helper = new PublishHelper(this._settings.Object, this._engine);

            string content;
            if (string.IsNullOrWhiteSpace(filename))
            {
                content = await this._helper.ReadAsync(null);
                content.Should().BeNullOrWhiteSpace();

                return;
            }

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            content = await this._helper.ReadAsync(filepath);
            content.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests whether the directory is created or not.
        /// </summary>
        /// <param name="directory">
        /// The directory.
        /// </param>
        [Test]
        [TestCase("")]
        [TestCase("published")]
        public void GivenDirectoryShouldCreateDirectory(string directory)
        {
            this._helper = new PublishHelper(this._settings.Object, this._engine);

            if (string.IsNullOrWhiteSpace(directory))
            {
                Action action = () => this._helper.CreatePublishDirectory(null);
                action.ShouldThrow<ArgumentNullException>();

                return;
            }

            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath);
            }

            this._helper.CreatePublishDirectory(directoryPath);
            Directory.Exists(directoryPath).Should().BeTrue();

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath);
            }
        }

        /// <summary>
        /// Tests whether file is published or not.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        [Test]
        [TestCase("")]
        [TestCase("published.txt")]
        public void GivenPublishPathAndContentShouldCreateFile(string filename)
        {
            var content = "Hello World";

            this._helper = new PublishHelper(this._settings.Object, this._engine);

            if (string.IsNullOrWhiteSpace(filename))
            {
                Action action = () => this._helper.Write(null, content);
                action.ShouldThrow<ArgumentNullException>();

                return;
            }

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            this._helper.Write(filepath, null);
            File.Exists(filepath).Should().BeFalse();

            this._helper.Write(filepath, content);
            File.Exists(filepath).Should().BeTrue();

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        /// <summary>
        /// Tests whether file is published or not.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        [Test]
        [TestCase("")]
        [TestCase("published.txt")]
        public async void GivenPublishPathAndContentShouldCreateFileAsync(string filename)
        {
            var content = "Hello World";

            this._helper = new PublishHelper(this._settings.Object, this._engine);

            if (string.IsNullOrWhiteSpace(filename))
            {
                Action action = () => this._helper.Write(null, content);
                action.ShouldThrow<ArgumentNullException>();

                return;
            }

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            await this._helper.WriteAsync(filepath, null);
            File.Exists(filepath).Should().BeFalse();

            await this._helper.WriteAsync(filepath, content);
            File.Exists(filepath).Should().BeTrue();

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        /// <summary>
        /// Tests whether the helper compiles template with model or not.
        /// </summary>
        [Test]
        public void GivenTemplateAndModelShoiuldReturnCompiledContent()
        {
            this._helper = new PublishHelper(this._settings.Object, this._engine);

            var contents = new Contents() { Theme = "default" };
            this._settings.SetupGet(p => p.Contents).Returns(contents);

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes", "default", "master.cshtml");

            var dateReleased = DateTime.UtcNow;
            var template = this._helper.Read(filepath);
            var model = new PageModel()
                            {
                                Title = "title",
                                Author = "author",
                                DateReleased = dateReleased,
                                Post = "<h1>Hello World</h1>"
                            };

            Action action = () => this._helper.Compile(null, model);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._helper.Compile(template, (PageModel)null);
            action.ShouldThrow<ArgumentNullException>();

            var compiled = this._helper.Compile(template, model);
            compiled.Should().NotBeNullOrWhiteSpace();
            compiled.Should().ContainEquivalentOf(model.Title);
            compiled.Should().ContainEquivalentOf(model.Post);
        }

        /// <summary>
        /// Tests whether the helper compiles template with model or not.
        /// </summary>
        [Test]
        public async void GivenTemplateAndModelShoiuldReturnCompiledContentAsync()
        {
            this._helper = new PublishHelper(this._settings.Object, this._engine);

            var contents = new Contents() { Theme = "default" };
            this._settings.SetupGet(p => p.Contents).Returns(contents);

            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes", "default", "master.cshtml");

            var dateReleased = DateTime.UtcNow;
            var template = await this._helper.ReadAsync(filepath);
            var model = new PageModel()
                            {
                                Title = "title",
                                Author = "author",
                                DateReleased = dateReleased,
                                Post = "<h1>Hello World</h1>"
                            };

            string compiled;
            Action action = () => compiled = this._helper.CompileAsync(null, model).Result;
            action.ShouldThrow<ArgumentNullException>();

            action = () => compiled = this._helper.CompileAsync(template, (PageModel)null).Result;
            action.ShouldThrow<ArgumentNullException>();

            compiled = await this._helper.CompileAsync(template, model);
            compiled.Should().NotBeNullOrWhiteSpace();
            compiled.Should().ContainEquivalentOf(model.Title);
            compiled.Should().ContainEquivalentOf(model.Post);
        }
    }
}