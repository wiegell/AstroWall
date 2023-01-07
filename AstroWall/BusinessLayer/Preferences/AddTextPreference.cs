using System;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    /// <summary>
    /// Sub preference regarding the option to add text
    /// to postprocessed image.
    /// </summary>
    [JsonObject]
    public class AddTextPreference : PostProcessPreference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddTextPreference"/> class.
        /// </summary>
        /// <param name="isEnabled">if addtext should be enabled.</param>
        internal AddTextPreference(bool isEnabled)
            : base(PostProcessPreferenceEnum.AddText, isEnabled)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddTextPreference"/> class.
        /// This constructor takes another instance as argument and creates a
        /// value copy.
        /// </summary>
        /// <param name="otherObj"></param>
        /// <param name="isEnabled"></param>
        internal AddTextPreference(AddTextPreference otherObj, bool isEnabled)
            : base(otherObj, isEnabled)
        {
            // Meant to create value copy of object, care
            // not to copy future ref.based properties
        }
    }
}
