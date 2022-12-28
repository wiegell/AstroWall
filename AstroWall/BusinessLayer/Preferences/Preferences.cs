using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Preferences
{
    public enum DailyCheckEnum
    {
        Newest,
        RandomAllTime,
        RandomPostXX,
        None
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Preferences
    {

        [JsonProperty]
        internal ImgWrap CurrentAstroWallpaper { get; set; }
        [JsonProperty]
        internal string CurrentPathToNonAstroWallpaper { get; set; }
        [JsonProperty]
        internal string UserChosenToSkipUpdatesBeforeVersion { get; set; }
        [JsonProperty]
        internal bool CheckUpdatesOnStartup { get; set; }
        [JsonProperty]
        internal bool AutoInstallUpdates { get; set; }
        [JsonProperty]
        internal bool RunAtStartup { get; set; }
        [JsonProperty]
        internal DailyCheckEnum DailyCheck { get; set; }
        [JsonProperty]
        internal DateTime LastOnlineCheck { get; set; }
        [JsonProperty]
        internal DateTime NextScheduledCheck { get; set; }
        [JsonProperty]
        internal AddText AddTextPostProcess { get; set; }

        public Dictionary<PostProcessType, PostProcess> PostProcesses
        {
            get
            {
                var retDict = new Dictionary<PostProcessType, PostProcess>();
                retDict.Add(PostProcessType.AddText, AddTextPostProcess);
                return retDict;
            }
        }

        public Preferences()
        {
            CurrentPathToNonAstroWallpaper = General.getCurrentWallpaperPath();
            AddTextPostProcess = new AddText(true);
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

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(this, General.getPrefsPath());
        }
    }
}

