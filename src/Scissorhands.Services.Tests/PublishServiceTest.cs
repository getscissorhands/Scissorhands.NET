using System;
using System.Collections.Generic;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Processors;

using FluentAssertions;

using Moq;

using NUnit.Framework;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PublishService" /> class.
    /// </summary>
    [TestFixture]
    public class PublishServiceTest
    {
        private Directories _directories;
        private List<Theme> _themes;
        private Contents _contents;
        private Mock<IYamlSettings> _settings;
        private Mock<IPostProcessor> _processor;
        private Mock<IPublishHelper> _helper;
        private IPublishService _service;

        /// <summary>
        /// Initialises all the resources for test.
        /// </summary>
        [SetUp]
        public void Init()
        {
            this._directories = new Directories() { Themes = "themes", Posts = "posts", Published = "published" };
            this._themes = new List<Theme>()
                               {
                                   new Theme() { Name = "default", Master = "master.cshtml" },
                                   new Theme() { Name = "second", Master = "master.cshtml" },
                               };
            this._contents = new Contents() { Theme = "default" };

            this._settings = new Mock<IYamlSettings>();
            this._settings.SetupGet(p => p.Directories).Returns(this._directories);
            this._settings.SetupGet(p => p.Themes).Returns(this._themes);
            this._settings.SetupGet(p => p.Contents).Returns(this._contents);

            this._processor = new Mock<IPostProcessor>();

            this._helper = new Mock<IPublishHelper>();

            this._service = new PublishService(this._settings.Object, this._processor.Object, this._helper.Object);
        }

        /// <summary>
        /// Releases all the resources no longer necessary.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            if (this._service != null)
            {
                this._service.Dispose();
            }
        }

        /// <summary>
        /// Test whether <see cref="ArgumentNullException" /> is thrown when null parameter is passed.
        /// </summary>
        [Test]
        public void GivenNullParameterShouldThrowArgumentNullException()
        {
            Action action = () => this._service = new PublishService(null, this._processor.Object, this._helper.Object);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._service = new PublishService(this._settings.Object, null, this._helper.Object);
            action.ShouldThrow<ArgumentNullException>();

            action = () => this._service = new PublishService(this._settings.Object, this._processor.Object, null);
            action.ShouldThrow<ArgumentNullException>();
        }
    }
}