using System;
using System.Collections.Generic;
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

        public Preferences()
        {
            currentPathToNonAstroWallpaper = MacOShelpers.getCurrentWallpaperPath();
        }

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, MacOShelpers.getPrefsPath());
        }

        public static Preferences fromSaveOrNew()
        {
            if (FileHelpers.PrefsExists())
            {
                Console.WriteLine("db exists, deserialize");
                return FileHelpers.DeSerializeNow<Preferences>(MacOShelpers.getPrefsPath());
            }
            else return new Preferences();
        }

        public bool hasAstroWall()
        {
            return !(currentAstroWallpaper == null);
        }
    }
}

