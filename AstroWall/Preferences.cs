using System;
using System.Collections.Generic;
using AppKit;
using Newtonsoft.Json;

namespace AstroWall
{
    [JsonObject]
    public class Preferences
    {
        [JsonProperty]
        public ImgWrap currentAstroWallpaper;
        [JsonProperty]
        public string currentPathToNonAstroWallpaper;
        public bool checkUpdatesOnLogin;
        public bool autoInstallUpdates;
        public bool autoInstallSilent;

        public Preferences()
        {
            currentPathToNonAstroWallpaper = MacOShelpers.getCurrentWallpaperPath();
        }

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, MacOShelpers.getPrefsPath());
        }

        public static Preferences fromSave()
        {
            if (FileHelpers.PrefsExists())
            {
                Console.WriteLine("prefs exists, deserialize");
                return FileHelpers.DeSerializeNow<Preferences>(MacOShelpers.getPrefsPath());
            }
            else return null;
        }

        public bool hasAstroWall()
        {
            return !(currentAstroWallpaper == null);
        }
    }
}

