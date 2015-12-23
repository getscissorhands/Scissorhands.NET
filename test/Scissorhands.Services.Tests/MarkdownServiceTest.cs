using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime;

using Aliencube.Scissorhands.Services.Tests.Fixtures;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceTest : IClassFixture<MarkdownServiceFixture>
    {
        private readonly IMarkdownService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceTest"/> class.
        /// </summary>
        public MarkdownServiceTest(MarkdownServiceFixture fixture)
        {
            this._service = fixture.MarkdownService;
        }

        [Fact]
        public void Test()
        {
        }
    }
}
