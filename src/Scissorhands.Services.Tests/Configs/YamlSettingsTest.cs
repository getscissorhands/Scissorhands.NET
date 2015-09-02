using System;

using Aliencube.Scissorhands.Services.Configs;

using FluentAssertions;

using NUnit.Framework;

namespace Aliencube.Scissorhands.Services.Tests.Configs
{
    /// <summary>
    /// This represents the test entity for the <see cref="YamlSettings" /> class.
    /// </summary>
    [TestFixture]
    public class YamlSettingsTest
    {
        private IYamlSettings _settings;

        /// <summary>
        /// Initialises all resources for tests.
        /// </summary>
        [SetUp]
        public void Init()
        {
        }

        /// <summary>
        /// Releases all resources after tests.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            if (this._settings != null)
            {
                this._settings.Dispose();
            }
        }

        /// <summary>
        /// Tests whether <see cref="ArgumentNullException" /> is thrown when the <c>null</c> parameter is passed.
        /// </summary>
        [Test]
        public void GivenNullFilenameShouldThrowArgumentNullException()
        {
            Action action = () => this._settings = YamlSettings.Load(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the <see cref="YamlSettings" /> instances is returned when a YAML filename is passed.
        /// </summary>
        [Test]
        public void GivenFilenameShouldReturnYamlSettings()
        {
            this._settings = YamlSettings.Load();
            this._settings.Should().NotBeNull();
            this._settings.Directories.Themes.Should().BeEquivalentTo("themes");

            this._settings = YamlSettings.Load("config.yml");
            this._settings.Should().NotBeNull();
            this._settings.Directories.Themes.Should().BeEquivalentTo("themes");
        }

        /// <summary>
        /// Tests whether <see cref="ArgumentNullException" /> is thrown when the <c>null</c> parameter is passed.
        /// </summary>
        [Test]
        public void GivenNullFilenameShouldThrowArgumentNullExceptionAsync()
        {
            Action action = () => this._settings = YamlSettings.LoadAsync(null).Result;
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the <see cref="YamlSettings" /> instances is returned when a YAML filename is passed.
        /// </summary>
        [Test]
        public async void GivenFilenameShouldReturnYamlSettingsAsync()
        {
            this._settings = await YamlSettings.LoadAsync();
            this._settings.Should().NotBeNull();
            this._settings.Directories.Themes.Should().BeEquivalentTo("themes");

            this._settings = await YamlSettings.LoadAsync("config.yml");
            this._settings.Should().NotBeNull();
            this._settings.Directories.Themes.Should().BeEquivalentTo("themes");
        }
    }
}