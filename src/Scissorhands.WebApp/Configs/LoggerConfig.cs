using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aliencube.Scissorhands.WebApp.Configs
{
    /// <summary>
    /// This represents the configuration entity for logging.
    /// </summary>
    public static class LoggerConfig
    {
        /// <summary>
        /// Registers logger.
        /// </summary>
        /// <param name="logger"><see cref="ILoggerFactory"/> instance.</param>
        /// <param name="config"><see cref="IConfiguration"/> instance.</param>
        public static void Register(ILoggerFactory logger, IConfiguration config)
        {
            logger.AddConsole(config.GetSection("Logging"));
            logger.AddDebug();
        }
    }
}