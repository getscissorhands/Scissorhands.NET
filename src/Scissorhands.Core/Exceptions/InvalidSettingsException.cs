﻿using System;

#if NETCOREAPP1_0

using ApplicationException = System.InvalidOperationException;

#endif

namespace Scissorhands.Exceptions
{
    /// <summary>
    /// This represents the exception entity thrown when setting is invalid.
    /// </summary>
    public class InvalidSettingsException : ApplicationException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        public InvalidSettingsException()
            : base()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InvalidSettingsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="InvalidSettingsException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner <see cref="Exception"/> instance.</param>
        public InvalidSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}