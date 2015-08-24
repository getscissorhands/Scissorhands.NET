using System.Collections.Generic;

namespace Aliencube.Scissorhands.Services.Tests.Models
{
    public class Settings
    {
        public Directories Directories { get; set; }

        public List<Theme> Themes { get; set; }

        public Contents Contents { get; set; }
    }

    public class Directories
    {
        public string Themes { get; set; }

        public string Posts { get; set; }

        public string Published { get; set; }
    }

    public class Theme
    {
        public string Name { get; set; }

        public string Master { get; set; }

        public string Css { get; set; }

        public string Js { get; set; }

        public string Images { get; set; }

        public string Includes { get; set; }
    }

    public class Contents
    {
        public string Theme { get; set; }

        public Pages Pages { get; set; }

        public string Archives { get; set; }

        public string Tags { get; set; }
    }

    public class Pages
    {
        public int Items { get; set; }

        public string Format { get; set; }
    }
}