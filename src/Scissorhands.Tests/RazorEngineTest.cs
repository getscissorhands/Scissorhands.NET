using System;
using System.IO;

using Aliencube.Scissorhands.Tests.Models;

using FluentAssertions;

using NUnit.Framework;

using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Aliencube.Scissorhands.Tests
{
    /// <summary>
    /// This is the test class for <see cref="RazorEngine" />.
    /// </summary>
    [TestFixture]
    public class RazorEngineTest
    {
        private static readonly string TemplateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Templates");

        /// <summary>
        /// Initialises all the resources for test.
        /// </summary>
        [SetUp]
        public void Init()
        {
        }

        /// <summary>
        /// Disposes all the resources that are not necessary.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
        }

        /// <summary>
        /// Test whether the razor script can combine model or not.
        /// </summary>
        [Test]
        public void GivenTemplateShouldCombineModel()
        {
            var template = File.ReadAllText(Path.Combine(TemplateFolderPath, @"Sample.cshtml"));
            var model = new SampleModel() { Name = "Joe" };

            string result;
            var config = new TemplateServiceConfiguration();
            using (var ts = RazorEngineService.Create(config))
            {
                result = ts.RunCompile(template, "key", typeof(SampleModel), model, null);
            }

            result.Should().Contain(string.Format("<h1>{0}</h1>", model.Name));
        }
    }
}