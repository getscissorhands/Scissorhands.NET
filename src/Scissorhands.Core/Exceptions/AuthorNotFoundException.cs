﻿using System;

#if NETCOREAPP1_0

using ApplicationException = System.InvalidOperationException;

#endif

namespace Scissorhands.Exceptions
{
    /// <summary>
    /// This represents the exception entity thrown when author is not found.
    /// </summary>
    public class AuthorNotFoundException : ApplicationException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AuthorNotFoundException"/> class.
        /// </summary>
        public AuthorNotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="AuthorNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AuthorNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="AuthorNotFoundException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner <see cref="Exception"/> instance.</param>
        public AuthorNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}