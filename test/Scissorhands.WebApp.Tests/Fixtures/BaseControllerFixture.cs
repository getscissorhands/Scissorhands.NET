using System;

using Microsoft.AspNet.Mvc;

namespace Aliencube.Scissorhands.WebApp.Tests.Fixtures
{
    /// <summary>
    /// This represents the base entity for controllers. This MUST be inherited.
    /// </summary>
    /// <typeparam name="T">Type inheriting the <see cref="Microsoft.AspNet.Mvc.Controller"/> class.</typeparam>
    public abstract class BaseControllerFixture<T> : IDisposable where T : Controller
    {
        private bool _disposed;

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.AspNet.Mvc.Controller"/> instance.
        /// </summary>
        public T Controller { get; protected set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.Controller.Dispose();

            this._disposed = true;
        }
    }
}