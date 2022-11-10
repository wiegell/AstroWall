using System;
using System.Collections.Generic;
using AppKit;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer
{
    [JsonObject]
    public class Preferences
    {
        [JsonProperty]
        public ImgWrap currentAstroWallpaper;
        [JsonProperty]
        public string currentPathToNonAstroWallpaper;
        [JsonProperty]
        public Version userChosenToSkipUpdatesBeforeVersion;
        public bool checkUpdatesOnLogin;
        public bool autoInstallUpdates;
        public bool runAtLogin;

        public Preferences()
        {
            currentPathToNonAstroWallpaper = GeneralHelpers.getCurrentWallpaperPath();
        }

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, GeneralHelpers.getPrefsPath());
        }

        public static Preferences fromSave()
        {
            if (FileHelpers.PrefsExists())
            {
                Console.WriteLine("prefs exists, deserialize");
                return FileHelpers.DeSerializeNow<Preferences>(GeneralHelpers.getPrefsPath());
            }
            else return null;
        }

        public bool hasAstroWall()
        {
            return !(currentAstroWallpaper == null);
        }
    }
}

