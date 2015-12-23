using System;
using System.Collections.Generic;
using System.Text;

using MarkdownDeep;

namespace Aliencube.Scissorhands.Services.Wrappers
{
    /// <summary>
    /// This provides interfaces to the <see cref="MarkdownWrapper" /> class.
    /// </summary>
    public interface IMarkdownWrapper : IDisposable
    {
        /// <summary>
        /// Gets or sets the format code block.
        /// </summary>
        Func<Markdown, string, string> FormatCodeBlock { get; set; }

        /// <summary>
        /// Gets or sets the get image size.
        /// </summary>
        Func<ImageInfo, bool> GetImageSize { get; set; }

        /// <summary>
        /// Gets or sets the prepare image.
        /// </summary>
        Func<HtmlTag, bool, bool> PrepareImage { get; set; }

        /// <summary>
        /// Gets or sets the prepare link.
        /// </summary>
        Func<HtmlTag, bool> PrepareLink { get; set; }

        /// <summary>
        /// Gets or sets the qualify url.
        /// </summary>
        Func<string, string> QualifyUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether auto heading Ids.
        /// </summary>
        bool AutoHeadingIDs { get; set; }

        /// <summary>
        /// Gets or sets the document location.
        /// </summary>
        string DocumentLocation { get; set; }

        /// <summary>
        /// Gets or sets the document root.
        /// </summary>
        string DocumentRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to extract head blocks.
        /// </summary>
        bool ExtractHeadBlocks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the markdown is extra mode.
        /// </summary>
        bool ExtraMode { get; set; }

        /// <summary>
        /// Gets the head block content.
        /// </summary>
        string HeadBlockContent { get; }

        /// <summary>
        /// Gets or sets the HTML class footnotes.
        /// </summary>
        string HtmlClassFootnotes { get; set; }

        /// <summary>
        /// Gets or sets the HTML class titled images.
        /// </summary>
        string HtmlClassTitledImages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether markdown in HTML.
        /// </summary>
        bool MarkdownInHtml { get; set; }

        /// <summary>
        /// Gets or sets the max image width.
        /// </summary>
        int MaxImageWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether new window for external links.
        /// </summary>
        bool NewWindowForExternalLinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether new window for local links.
        /// </summary>
        bool NewWindowForLocalLinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether no follow links.
        /// </summary>
        bool NoFollowLinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether safe mode.
        /// </summary>
        bool SafeMode { get; set; }

        /// <summary>
        /// Gets or sets the section footer.
        /// </summary>
        string SectionFooter { get; set; }

        /// <summary>
        /// Gets or sets the section header.
        /// </summary>
        string SectionHeader { get; set; }

        /// <summary>
        /// Gets or sets the section heading suffix.
        /// </summary>
        string SectionHeadingSuffix { get; set; }

        /// <summary>
        /// Gets or sets the summary length.
        /// </summary>
        int SummaryLength { get; set; }

        /// <summary>
        /// Gets or sets the url base location.
        /// </summary>
        string UrlBaseLocation { get; set; }

        /// <summary>
        /// Gets or sets the url root location.
        /// </summary>
        string UrlRootLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user breaks.
        /// </summary>
        bool UserBreaks { get; set; }

        /// <summary>
        /// Gets the link definition.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Returns the <see cref="LinkDefinition" /> instance.
        /// </returns>
        LinkDefinition GetLinkDefinition(string id);

        /// <summary>
        /// The on get image size.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="titledImage">
        /// The titled image.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool OnGetImageSize(string url, bool titledImage, out int width, out int height);

        /// <summary>
        /// The on prepare image.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="titledImage">
        /// The titled image.
        /// </param>
        void OnPrepareImage(HtmlTag tag, bool titledImage);

        /// <summary>
        /// The on prepare link.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        void OnPrepareLink(HtmlTag tag);

        /// <summary>
        /// The on qualify url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string OnQualifyUrl(string url);

        /// <summary>
        /// The on section footer.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        void OnSectionFooter(StringBuilder dest, int index);

        /// <summary>
        /// The on section header.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        void OnSectionHeader(StringBuilder dest, int index);

        /// <summary>
        /// The on section heading suffix.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        void OnSectionHeadingSuffix(StringBuilder dest, int index);

        /// <summary>
        /// The transform.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string Transform(string str);

        /// <summary>
        /// The transform.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="definitions">
        /// The definitions.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string Transform(string str, out Dictionary<string, LinkDefinition> definitions);
    }
}