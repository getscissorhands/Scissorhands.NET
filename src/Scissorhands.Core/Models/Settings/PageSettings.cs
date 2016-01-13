using System;
using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the view model entity for page.
    /// </summary>
    public class PageSettings
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Author Author { get; set; }

        public DateTime Date { get; set; }

        public string BaseUrl { get; set; }

        public string Url { get; set; }

        public List<PageSettings> Pages { get; set; }
    }
}