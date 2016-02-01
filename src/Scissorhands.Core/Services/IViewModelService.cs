using System;

using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the viewmodel service.
    /// </summary>
    public interface IViewModelService : IDisposable
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PostPreviewViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostPreviewViewModel"/> instance created.</returns>
        PostPreviewViewModel CreatePostPreviewViewModel();

        /// <summary>
        /// Creates a new instance of the <see cref="PostPublishViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostPublishViewModel"/> instance created.</returns>
        PostPublishViewModel CreatePostPublishViewModel();
    }
}