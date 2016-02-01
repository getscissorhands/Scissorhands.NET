using System;
using System.Collections.Generic;

using FluentAssertions;

using Scissorhands.Core.Tests.Fixtures;
using Scissorhands.Exceptions;
using Scissorhands.Models.Settings;

using Xunit;

namespace Scissorhands.Core.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="SiteMetadataSettings"/> class.
    /// </summary>
    public class SiteMetadataSettingsTest : IClassFixture<SiteMetadataSettingsFixture>
    {
        private readonly ISiteMetadataSettings _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteMetadataSettingsTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="SiteMetadataSettingsFixture"/> instance.</param>
        public SiteMetadataSettingsTest(SiteMetadataSettingsFixture fixture)
        {
            this._metadata = fixture.SiteMetadataSettings;
        }

        /// <summary>
        /// Tests whether the GutAuthor should return <c>null</c> or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetAuthor_ShouldReturn_Null()
        {
            var result = this._metadata.GetAuthor(null);
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the GetAuthor method should return <c>null</c> or not.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        [Theory]
        [InlineData("John Bloggs")]
        public void Given_UnlisedName_GetAuthor_ShouldReturn_Null(string name)
        {
            var authors = new List<Author>()
                              {
                                  new Author() { Name = "Joe Bloggs", IsDefault = true },
                                  new Author() { Name = "Johne Doe", IsDefault = false }
                              };
            this._metadata.Authors = authors;

            var result = this._metadata.GetAuthor(name);
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the GetAuthor method should return result or not.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        [Theory]
        [InlineData("Joe Bloggs")]
        public void Given_ListedName_GeteAuthor_ShouldReturn_Result(string name)
        {
            var authors = new List<Author>()
                              {
                                  new Author() { Name = "Joe Bloggs", IsDefault = true },
                                  new Author() { Name = "Johne Doe", IsDefault = false }
                              };
            this._metadata.Authors = authors;

            var result = this._metadata.GetAuthor(name);
            result.Should().NotBeNull();
            result.Name.Should().BeEquivalentTo(name);
        }

        /// <summary>
        /// Tests whether the GetDefaultAuthorName method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_GetDefaultAuthorName_ShouldThrow_AuthorNotFoundException()
        {
            var authors = new List<Author>()
                              {
                                  new Author() { Name = "Joe Bloggs", IsDefault = false },
                                  new Author() { Name = "Johne Doe", IsDefault = false }
                              };
            this._metadata.Authors = authors;

            Action action = () => { var result = this._metadata.GetDefaultAuthorName(); };
            action.ShouldThrow<AuthorNotFoundException>();
        }

        /// <summary>
        /// Tests whether the GetDefaultAuthorName method should return result or not.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        [Theory]
        [InlineData("Joe Bloggs")]
        public void Given_GetDefaultAuthorName_ShouldReturn_Result(string name)
        {
            var authors = new List<Author>()
                              {
                                  new Author() { Name = "Joe Bloggs", IsDefault = true },
                                  new Author() { Name = "Johne Doe", IsDefault = false }
                              };
            this._metadata.Authors = authors;

            var result = this._metadata.GetDefaultAuthorName();
            result.Should().BeEquivalentTo(name);
        }
    }
}