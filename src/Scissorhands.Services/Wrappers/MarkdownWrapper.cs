using System;
using System.Collections.Generic;
using System.Text;

using MarkdownDeep;

namespace Aliencube.Scissorhands.Services.Wrappers
{
    /// <summary>
    /// This represents the wrapper entity for the <see cref="Markdown" /> class.
    /// </summary>
    public class MarkdownWrapper : IMarkdownWrapper
    {
        private readonly Markdown _md;

        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="MarkdownWrapper" /> class.
        /// </summary>
        /// <param name="md">
        /// The md.
        /// </param>
        public MarkdownWrapper(Markdown md = null)
        {
            if (md == null)
            {
                this._md = new Markdown();
                return;
            }

            this._md = md;
        }

        /// <summary>
        /// Gets or sets the format code block.
        /// </summary>
        public Func<Markdown, string, string> FormatCodeBlock
        {
            get
            {
                return this._md.FormatCodeBlock;
            }

            set
            {
                this._md.FormatCodeBlock = value;
            }
        }

        /// <summary>
        /// Gets or sets the get image size.
        /// </summary>
        public Func<ImageInfo, bool> GetImageSize
        {
            get
            {
                return this._md.GetImageSize;
            }

            set
            {
                this._md.GetImageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the prepare image.
        /// </summary>
        public Func<HtmlTag, bool, bool> PrepareImage
        {
            get
            {
                return this._md.PrepareImage;
            }

            set
            {
                this._md.PrepareImage = value;
            }
        }

        /// <summary>
        /// Gets or sets the prepare link.
        /// </summary>
        public Func<HtmlTag, bool> PrepareLink
        {
            get
            {
                return this._md.PrepareLink;
            }

            set
            {
                this._md.PrepareLink = value;
            }
        }

        /// <summary>
        /// Gets or sets the qualify url.
        /// </summary>
        public Func<string, string> QualifyUrl
        {
            get
            {
                return this._md.QualifyUrl;
            }

            set
            {
                this._md.QualifyUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto heading Ids.
        /// </summary>
        public bool AutoHeadingIDs
        {
            get
            {
                return this._md.AutoHeadingIDs;
            }

            set
            {
                this._md.AutoHeadingIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the document location.
        /// </summary>
        public string DocumentLocation
        {
            get
            {
                return this._md.DocumentLocation;
            }

            set
            {
                this._md.DocumentLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the document root.
        /// </summary>
        public string DocumentRoot
        {
            get
            {
                return this._md.DocumentRoot;
            }

            set
            {
                this._md.DocumentRoot = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to extract head blocks.
        /// </summary>
        public bool ExtractHeadBlocks
        {
            get
            {
                return this._md.ExtractHeadBlocks;
            }

            set
            {
                this._md.ExtractHeadBlocks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the markdown is extra mode.
        /// </summary>
        public bool ExtraMode
        {
            get
            {
                return this._md.ExtraMode;
            }

            set
            {
                this._md.ExtraMode = value;
            }
        }

        /// <summary>
        /// Gets the head block content.
        /// </summary>
        public string HeadBlockContent
        {
            get
            {
                return this._md.HeadBlockContent;
            }
        }

        /// <summary>
        /// Gets or sets the HTML class footnotes.
        /// </summary>
        public string HtmlClassFootnotes
        {
            get
            {
                return this._md.HtmlClassFootnotes;
            }

            set
            {
                this._md.HtmlClassFootnotes = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTML class titled images.
        /// </summary>
        public string HtmlClassTitledImages
        {
            get
            {
                return this._md.HtmlClassTitledImages;
            }

            set
            {
                this._md.HtmlClassTitledImages = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether markdown in HTML.
        /// </summary>
        public bool MarkdownInHtml
        {
            get
            {
                return this._md.MarkdownInHtml;
            }

            set
            {
                this._md.MarkdownInHtml = value;
            }
        }

        /// <summary>
        /// Gets or sets the max image width.
        /// </summary>
        public int MaxImageWidth
        {
            get
            {
                return this._md.MaxImageWidth;
            }

            set
            {
                this._md.MaxImageWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether new window for external links.
        /// </summary>
        public bool NewWindowForExternalLinks
        {
            get
            {
                return this._md.NewWindowForExternalLinks;
            }

            set
            {
                this._md.NewWindowForExternalLinks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether new window for local links.
        /// </summary>
        public bool NewWindowForLocalLinks
        {
            get
            {
                return this._md.NewWindowForLocalLinks;
            }

            set
            {
                this._md.NewWindowForLocalLinks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether no follow links.
        /// </summary>
        public bool NoFollowLinks
        {
            get
            {
                return this._md.NoFollowLinks;
            }

            set
            {
                this._md.NoFollowLinks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether safe mode.
        /// </summary>
        public bool SafeMode
        {
            get
            {
                return this._md.SafeMode;
            }

            set
            {
                this._md.SafeMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the section footer.
        /// </summary>
        public string SectionFooter
        {
            get
            {
                return this._md.SectionFooter;
            }

            set
            {
                this._md.SectionFooter = value;
            }
        }

        /// <summary>
        /// Gets or sets the section header.
        /// </summary>
        public string SectionHeader
        {
            get
            {
                return this._md.SectionHeader;
            }

            set
            {
                this._md.SectionHeader = value;
            }
        }

        /// <summary>
        /// Gets or sets the section heading suffix.
        /// </summary>
        public string SectionHeadingSuffix
        {
            get
            {
                return this._md.SectionHeadingSuffix;
            }

            set
            {
                this._md.SectionHeadingSuffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the summary length.
        /// </summary>
        public int SummaryLength
        {
            get
            {
                return this._md.SummaryLength;
            }

            set
            {
                this._md.SummaryLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the url base location.
        /// </summary>
        public string UrlBaseLocation
        {
            get
            {
                return this._md.UrlBaseLocation;
            }

            set
            {
                this._md.UrlBaseLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the url root location.
        /// </summary>
        public string UrlRootLocation
        {
            get
            {
                return this._md.UrlRootLocation;
            }

            set
            {
                this._md.UrlRootLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether user breaks.
        /// </summary>
        public bool UserBreaks
        {
            get
            {
                return this._md.UserBreaks;
            }

            set
            {
                this._md.UserBreaks = value;
            }
        }

        /// <summary>
        /// Joins the list of sections.
        /// </summary>
        /// <param name="sections">
        /// The list of sections.
        /// </param>
        /// <returns>
        /// Returns the sections all joined.
        /// </returns>
        public static string JoinSections(List<string> sections)
        {
            return Markdown.JoinSections(sections);
        }

        /// <summary>
        /// Joins user sections.
        /// </summary>
        /// <param name="sections">
        /// The sections.
        /// </param>
        /// <returns>
        /// Returns the sections all joined.
        /// </returns>
        public static string JoinUserSections(List<string> sections)
        {
            return Markdown.JoinUserSections(sections);
        }

        /// <summary>
        /// Splits sections.
        /// </summary>
        /// <param name="markdown">
        /// The markdown.
        /// </param>
        /// <returns>
        /// Returns the list of strings.
        /// </returns>
        public static List<string> SplitSections(string markdown)
        {
            return Markdown.SplitSections(markdown);
        }

        /// <summary>
        /// Splits user sections.
        /// </summary>
        /// <param name="markdown">
        /// The markdown.
        /// </param>
        /// <returns>
        /// Returns the list of strings.
        /// </returns>
        public static List<string> SplitUserSections(string markdown)
        {
            return Markdown.SplitUserSections(markdown);
        }

        /// <summary>
        /// Gets the link definition.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Returns the <see cref="LinkDefinition" /> instance.
        /// </returns>
        public LinkDefinition GetLinkDefinition(string id)
        {
            return this._md.GetLinkDefinition(id);
        }

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
        public bool OnGetImageSize(string url, bool titledImage, out int width, out int height)
        {
            return this._md.OnGetImageSize(url, titledImage, out width, out height);
        }

        /// <summary>
        /// The on prepare image.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="titledImage">
        /// The titled image.
        /// </param>
        public void OnPrepareImage(HtmlTag tag, bool titledImage)
        {
            this._md.OnPrepareImage(tag, titledImage);
        }

        /// <summary>
        /// The on prepare link.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        public void OnPrepareLink(HtmlTag tag)
        {
            this._md.OnPrepareLink(tag);
        }

        /// <summary>
        /// The on qualify url.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string OnQualifyUrl(string url)
        {
            return this._md.OnQualifyUrl(url);
        }

        /// <summary>
        /// The on section footer.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        public void OnSectionFooter(StringBuilder dest, int index)
        {
            this._md.OnSectionFooter(dest, index);
        }

        /// <summary>
        /// The on section header.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        public void OnSectionHeader(StringBuilder dest, int index)
        {
            this._md.OnSectionHeader(dest, index);
        }

        /// <summary>
        /// The on section heading suffix.
        /// </summary>
        /// <param name="dest">
        /// The dest.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        public void OnSectionHeadingSuffix(StringBuilder dest, int index)
        {
            this._md.OnSectionHeadingSuffix(dest, index);
        }

        /// <summary>
        /// The transform.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Transform(string str)
        {
            return this._md.Transform(str);
        }

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
        public string Transform(string str, out Dictionary<string, LinkDefinition> definitions)
        {
            return this._md.Transform(str, out definitions);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}