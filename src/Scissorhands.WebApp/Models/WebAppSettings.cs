namespace Aliencube.Scissorhands.WebApp.Models
{
    public class AppSettings
    {
        public Logging Logging { get; set; }
        public WebAppSettings WebApp { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }

    public class WebAppSettings
    {
        public string Server { get; set; }
    }

}
