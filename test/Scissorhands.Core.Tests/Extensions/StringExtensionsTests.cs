using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Scissorhands.Core.Extensions;

namespace Scissorhands.Core.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [DataTestMethod]
        [DataRow("hello world", "this is hello world", "this-is-slug", 100)]
        public void Given_FullYaml_Then_It_Should_Return_FrontMatter(string title, string description, string slug, int order)
        {
            var yaml = @$"title: {title}
description: {description}
slug: {slug}
order: {order}";

            var result = StringExtensions.ToFrontMatter(yaml);

            result.Title.Should().Be(title);
            result.Description.Should().Be(description);
            result.Slug.Should().Be(slug);
            result.Order.Should().Be(order);
        }

        [DataTestMethod]
        [DataRow("hello world", "this is hello world", "this-is-slug")]
        public void Given_Yaml_Without_Order_Then_It_Should_Return_FrontMatter(string title, string description, string slug)
        {
            var yaml = @$"title: {title}
description: {description}
slug: {slug}";

            var result = StringExtensions.ToFrontMatter(yaml);

            result.Title.Should().Be(title);
            result.Description.Should().Be(description);
            result.Slug.Should().Be(slug);
            result.Order.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow("hello world", "this is hello world")]
        public void Given_Yaml_Without_Slug_Then_It_Should_Return_FrontMatter(string title, string description)
        {
            var yaml = @$"title: {title}
description: {description}";

            var result = StringExtensions.ToFrontMatter(yaml);

            result.Title.Should().Be(title);
            result.Description.Should().Be(description);
            result.Slug.Should().BeNull();
            result.Order.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow("**hello world**", "<strong>hello world</strong>")]
        [DataRow("## hello world", "hello world</h2>")]
        public void Given_Markdown_Then_It_Should_Return_Html(string markdown, string expected)
        {
            var result = StringExtensions.ToHtml(markdown);

            result.Should().ContainEquivalentOf(expected);
        }
    }
}