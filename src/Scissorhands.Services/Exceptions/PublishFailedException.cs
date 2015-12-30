using System;

#if DNXCORE50

using ApplicationException = global::System.InvalidOperationException;

#endif

namespace Aliencube.Scissorhands.Services.Exceptions
{
    /// <summary>
    /// This represents the exception entity thrown on publish failure.
    /// </summary>
    public class PublishFailedException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFailedException"/> class.
        /// </summary>
        public PublishFailedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFailedException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public PublishFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFailedException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner <see cref="Exception"/> instance.</param>
        public PublishFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}