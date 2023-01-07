using System.Net;

namespace AstroWall
{
    /// <summary>
    /// Response wrap containing.
    /// </summary>
    internal struct UrlResponseWrap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlResponseWrap"/> struct.
        /// </summary>
        /// <param name="imageUrl"></param>
        internal UrlResponseWrap(string imageUrl)
            : this()
        {
            this.ImageUrl = imageUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlResponseWrap"/> struct.
        /// </summary>
        /// <param name="imageUrl"></param>
        internal UrlResponseWrap(HttpStatusCode statusCode)
            : this()
        {
            this.PageStatusCode = statusCode;
        }

        /// <summary>
        /// Gets url of image that can be retrieved from "page".
        /// </summary>
        internal string ImageUrl { get; }

        /// <summary>
        /// Gets page status code.
        /// </summary>
        internal HttpStatusCode PageStatusCode { get; }
    }
}