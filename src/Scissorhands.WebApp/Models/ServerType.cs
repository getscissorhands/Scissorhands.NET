namespace Aliencube.Scissorhands.WebApp.Models
{
    /// <summary>
    /// This specifies server type.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// Indicates the server type is not specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Indicates that the server is either IIS or IISExpress.
        /// </summary>
        Iis = 1,
    }
}