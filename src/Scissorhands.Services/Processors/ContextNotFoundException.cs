using System;
using System.Runtime.Serialization;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This represents the exception entity for the <see cref="ProcessContext" /> class.
    /// </summary>
    public class ContextNotFoundException : ApplicationException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ContextNotFoundException" /> class.
        /// </summary>
        public ContextNotFoundException()
            : this(null, "Invalid context")
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ContextNotFoundException"/> class.
        /// </summary>
        /// <param name="key">
        /// The key missing.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        public ContextNotFoundException(string key, string message = null)
            : base(string.IsNullOrWhiteSpace(message) ? "Key not found in the context" : message)
        {
            this.Key = key;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ContextNotFoundException" /> class.
        /// </summary>
        /// <param name="key">
        /// The key missing.
        /// </param>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public ContextNotFoundException(string key, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Key = key;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ContextNotFoundException" /> class.
        /// </summary>
        /// <param name="info">
        /// The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The contextual information about the source or destination.
        /// </param>
        protected ContextNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; private set; }
    }
}