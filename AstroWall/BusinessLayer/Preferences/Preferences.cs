using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    /// <summary>
    /// User preferences class. Care that all "set" operations must be followed with additional
    /// actions. E.g. set CurrentAstroWallpaper will only set the preference and not the actual wallpaper.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Preferences
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Preferences"/> class.
        /// Only meant to be used, if the user has not already a pref. stored in local json.
        /// </summary>
        public Preferences()
        {
            CurrentPathToNonAstroWallpaper = General.GetCurrentWallpaperPath();
            AddTextPostProcess = new AddTextPreference(true);
        }

        /// <summary>Gets prefs instance from json save.</summary>
        /// <returns>Preferences instance. Null if no save.</returns>
        internal static Preferences FromSave
        {
            get
            {
                if (FileHelpers.PrefsExists())
                {
                    Console.WriteLine("prefs exists, deserialize");
                    return FileHelpers.DeSerializeNow<Preferences>(General.GetPrefsPath());
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets current wallpaper of ImgWrap type. Is Null if never set by the app. Still
        /// holds a value if the user choses another wallpaper via
        /// system preferences.
        /// </summary>
        [JsonProperty]
        internal ImgWrap CurrentAstroWallpaper { get; set; }

        /// <summary>
        /// Gets or sets current path to non-ImgWrap wallpaper. This is used
        /// to revert to original wallpaper, if the user only previews wallpapers
        /// via the app but does not actually choose one.
        /// </summary>
        [JsonProperty]
        internal string CurrentPathToNonAstroWallpaper { get; set; }

        /// <summary>
        /// Gets or sets a version threshold that the user has chosen
        /// to skip updates before.
        /// TODO not yet implemented functionality.
        /// </summary>
        [JsonProperty]
        internal string UserChosenToSkipUpdatesBeforeVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether updates on startup is enabled.
        /// </summary>
        [JsonProperty]
        internal bool CheckUpdatesOnStartup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether silent autoinstall is enabled.
        /// </summary>
        [JsonProperty]
        internal bool AutoInstallUpdates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether run at startup (launchagent) is enabled.
        /// </summary>
        [JsonProperty]
        internal bool RunAtStartup { get; set; }

        /// <summary>
        /// Gets or sets daily check pref.
        /// </summary>
        [JsonProperty]
        internal DailyCheckEnum DailyCheck { get; set; }

        /// <summary>
        /// Gets or sets last online image check.
        /// </summary>
        [JsonProperty]
        internal DateTime LastOnlineImageCheck { get; set; }

        /// <summary>
        /// Gets or sets next scheduled check (at noon of some specific date)
        /// is used to check on wake, whether the check has passed or not.
        /// </summary>
        [JsonProperty]
        internal DateTime NextScheduledCheck { get; set; }

        /// <summary>
        /// Gets or sets add text postprocess preference.
        /// </summary>
        [JsonProperty]
        internal AddTextPreference AddTextPostProcess { get; set; }

        /// <summary>
        /// Gets a value indicating whether the user has selected a wallpaper via the app or not.
        /// </summary>
        internal bool HasAstroWall => !(CurrentAstroWallpaper == null);

        /// <summary>
        /// Gets post process settings as dictionary.
        /// </summary>
        internal Dictionary<PostProcessPreferenceEnum, PostProcessPreference> PostProcesses
        {
            get
            {
                var retDict = new Dictionary<PostProcessPreferenceEnum, PostProcessPreference>();
                retDict.Add(PostProcessPreferenceEnum.AddText, AddTextPostProcess);
                return retDict;
            }
        }

        /// <summary>
        /// Saves prefs to json.
        /// </summary>
        internal void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, General.GetPrefsPath());
        }
    }
}