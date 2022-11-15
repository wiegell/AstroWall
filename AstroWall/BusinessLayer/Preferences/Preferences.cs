using System;
using System.Collections.Generic;
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

        public List<PostProcess> PostProcesses
        {
            get
            {
                var retList = new List<PostProcess>();
                retList.Add(AddTextPostProcess);
                return retList;
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

