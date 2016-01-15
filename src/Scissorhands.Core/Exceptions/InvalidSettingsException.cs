using System;

#if DNXCORE50

using ApplicationException = global::System.InvalidOperationException;

#endif

namespace Scissorhands.Exceptions
{
    /// <summary>
    /// This represents the exception entity thrown when setting is invalid.
    /// </summary>
    public class InvalidSettingsException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        public InvalidSettingsException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InvalidSettingsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner <see cref="Exception"/> instance.</param>
        public InvalidSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}