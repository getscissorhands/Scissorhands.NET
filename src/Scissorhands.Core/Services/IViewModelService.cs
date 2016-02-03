using System;

using Microsoft.AspNet.Http;

using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the viewmodel service.
    /// </summary>
    public interface IViewModelService : IDisposable
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PostFormViewModel"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PostFormViewModel"/> instance created.</returns>
        PostFormViewModel CreatePostFormViewModel(HttpRequest request);

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

        /// <summary>
        /// Creates a new instance of the <see cref="PostParseViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostParseViewModel"/> instance created.</returns>
        PostParseViewModel CreatePostParseViewModel();

        /// <summary>
        /// Gets the <see cref="PageMetadataSettings"/> instance.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="publishMode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the <see cref="PageMetadataSettings"/> instance.</returns>
        PageMetadataSettings CreatePageMetadata(PostFormViewModel model, HttpRequest request, PublishMode publishMode);
    }
}