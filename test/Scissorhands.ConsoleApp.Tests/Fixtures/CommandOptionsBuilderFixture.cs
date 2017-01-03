using System;

namespace Scissorhands.ConsoleApp.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="CommandOptionsBuilderTest"/> class.
    /// </summary>
    public class CommandOptionsBuilderFixture : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandOptionsBuilderFixture"/> class.
        /// </summary>
        public CommandOptionsBuilderFixture()
        {
            this.CommandOptionsBuilder = new CommandOptionsBuilder();
        }

        /// <summary>
        /// Gets the <see cref="ICommandOptionsBuilder"/> instance.
        /// </summary>
        public ICommandOptionsBuilder CommandOptionsBuilder { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}