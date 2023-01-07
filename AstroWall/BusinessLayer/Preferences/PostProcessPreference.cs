using System;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    /// <summary>
    /// Post process abstract class meant to be extended
    /// into different types of postprocessing.
    /// </summary>
    [JsonObject]
    public abstract class PostProcessPreference
    {
        [JsonProperty]
        private readonly PostProcessPreferenceEnum name;

        [JsonProperty]
        private readonly bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessPreference"/> class.
        /// </summary>
        internal PostProcessPreference(PostProcessPreferenceEnum name, bool isEnabled)
        {
            this.name = name;
            this.isEnabled = isEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessPreference"/> class.
        /// </summary>
        /// <param name="otherObj">Value copies other instance.</param>
        /// <param name="name">Overwrites value from other object.</param>
        internal PostProcessPreference(PostProcessPreference otherObj, PostProcessPreferenceEnum name)
        {
            this.name = name;
            this.isEnabled = otherObj.isEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessPreference"/> class.
        /// </summary>
        /// <param name="otherObj">Value copies other instance.</param>
        /// <param name="isEnabled">Overwrites value from other object.</param>
        internal PostProcessPreference(PostProcessPreference otherObj, bool isEnabled)
        {
            this.name = otherObj.Name;
            this.isEnabled = isEnabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessPreference"/> class.
        /// </summary>
        /// <param name="otherObj">Value copies other instance.</param>
        internal PostProcessPreference(PostProcessPreference otherObj)
        {
            this.name = otherObj.Name;
            this.isEnabled = otherObj.isEnabled;
        }

        /// <summary>
        /// Gets a value indicating whether gets whether current postprocess is enabled.
        /// </summary>
        internal bool IsEnabled => isEnabled;

        /// <summary>
        /// Gets name enum e.g. "AddText".
        /// </summary>
        internal PostProcessPreferenceEnum Name => name;
    }
}
