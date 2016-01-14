using System;

namespace Scissorhands.Extensions
{
    /// <summary>
    /// This specifies the extension types.
    /// </summary>
    [Flags]
    public enum ExtensionTypes
    {
        /// <summary>
        /// Indicates that no extension type is defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the extension is applicable to posts.
        /// </summary>
        Post = 1 << 0,

        /// <summary>
        /// Indicates that the extension is applicable to archives pages.
        /// </summary>
        Archive = 1 << 1,

        /// <summary>
        /// Indicates that the extension is applicable to admin pages.
        /// </summary>
        Admin = 1 << 2,
    }
}