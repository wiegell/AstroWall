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
        [JsonProperty]
        public AddText AddTextPostProcess;

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

