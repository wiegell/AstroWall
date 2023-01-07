namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Update prompts response.
    /// </summary>
    internal struct UpdatePromptResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user accepts or skips update.
        /// </summary>
        internal bool AcceptOrSkipUpdate { get; set; }

        /// <summary>
        /// Gets or sets the version that is skipped by the user.
        /// </summary>
        internal string SkippedVersion { get; set; }
    }
}