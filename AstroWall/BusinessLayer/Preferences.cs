using System;
using System.Collections.Generic;
using AppKit;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer
{
    public enum DailyCheckEnum
    {
        Newest,
        RandomAllTime,
        RandomPostXX,
        None
    }

    [JsonObject]
    public class Preferences
    {
        [JsonProperty]
        public ImgWrap CurrentAstroWallpaper;
        [JsonProperty]
        public string CurrentPathToNonAstroWallpaper;
        [JsonProperty]
        public string UserChosenToSkipUpdatesBeforeVersion;
        [JsonProperty]
        public bool CheckUpdatesOnStartup;
        [JsonProperty]
        public bool AutoInstallUpdates;
        [JsonProperty]
        public bool RunAtStartup;
        [JsonProperty]
        public DailyCheckEnum DailyCheck;
        [JsonProperty]
        public DateTime LastOnlineCheck;
        [JsonProperty]
        public DateTime NextScheduledCheck;

        public Preferences()
        {
            CurrentPathToNonAstroWallpaper = General.getCurrentWallpaperPath();
        }

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, General.getPrefsPath());
        }

        public static Preferences fromSave()
        {
            if (FileHelpers.PrefsExists())
            {
                Console.WriteLine("prefs exists, deserialize");
                return FileHelpers.DeSerializeNow<Preferences>(General.getPrefsPath());
            }
            else return null;
        }

        public bool hasAstroWall()
        {
            return !(CurrentAstroWallpaper == null);
        }
    }
}

