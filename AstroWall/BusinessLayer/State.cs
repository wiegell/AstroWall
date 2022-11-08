using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;

namespace AstroWall.BusinessLayer
{
    public enum stateEnum
    {
        Initializing,
        Idle,
        ResolvingUrl,
        UpdatingDatabase,
        DownloadingNewest,
        PostProcessing,
        SettingWallpaper,
        BrowsingWallpapers
    }

    public class State
    {
        // Refs
        MenuHandler menuHandler;

        // Overall state
        public stateEnum state { get; private set; }
        private Database db;
        public Preferences Prefs { get; private set; }

        // Updates
        public Version currentVersion { private set; get; }
        private Updates updates;
        public UpdateLibrary.Release pendingUpdate { private set; get; }

        // currentVersionString is the long tag from git including commit hash  
        public State(MenuHandler menuHandlerArg, string currentVersionString)
        {
            this.menuHandler = menuHandlerArg;

            updates = new Updates(currentVersionString);
        }

        public void setPrefs(Preferences prefsArg)
        {
            this.Prefs = prefsArg;
        }

        public async Task<Boolean> GetUpdateManifestAndCheckIfUpdatePending()
        {
            await updates.GetManifest();
            Console.WriteLine("Update manifest downloaded and parsed");
            pendingUpdate = updates.checkManifestForNewer();
            bool hasPendingUpdate = pendingUpdate == null ? false : true;
            Console.WriteLine("Has pending update: " + hasPendingUpdate);
            return hasPendingUpdate;
        }

        public void LoadOrCreateDB()
        {
            db = new Database();
        }

        public bool LoadPrefsFromSave()
        {
            Prefs = Preferences.fromSave();
            return Prefs != null;
        }



        public void saveDBToDisk()
        {
            db.SaveToDisk();
        }

        public void savePrefsToDisk()
        {
            Prefs.SaveToDisk();
        }

        public void SetStateInitializing()
        {
            Console.WriteLine("State: Initializing");
            state = stateEnum.Initializing;
            menuHandler.EnableStatusIcon();
            menuHandler.DisableAllItems();
            menuHandler.SetTitleInitialising();
            menuHandler.RunDownloadIconAnimation();
        }

        public void SetStateChoosePrefs()
        {
            Console.WriteLine("State: Choose prefs");
            menuHandler.disableStatusIcon();
        }


        public void SetStateIdle()
        {
            Console.WriteLine("Setting state to idle:");
            menuHandler.EnableStatusIcon();
            menuHandler.SetIconToDefault();
            state = stateEnum.Idle;
            menuHandler.HideState();

        }

        public async Task UpdateStateFromOnline()
        {
            Console.WriteLine("load data");
            await db.LoadDataButNoImgFromOnlineStartingAtDate(10, DateTime.Now);
            Console.WriteLine("load img");
            await db.LoadImgs();
            Console.WriteLine("wraplist: " + db.ImgWrapList.Count);
            foreach (ImgWrap pic in db.ImgWrapList)
            {
                Console.WriteLine("preview url: " + pic.ImgLocalPreviewUrl);
            }
        }

        public async Task FireUpdateHandler()
        {
            bool hasPendingUpdate = await GetUpdateManifestAndCheckIfUpdatePending();
            if (hasPendingUpdate)
            {
                Console.WriteLine("Has pending update: {0}", pendingUpdate.version);
            }
            else
            {
                Console.WriteLine("No pending updates, is up to date");
            }
        }

        public List<ImgWrap> getPresentableImages()
        {
            return db.ImgWrapList.Where((iw) => iw.imgIsGettable).ToList<ImgWrap>();
        }

        public void setStateBrowsing()
        {
            Console.WriteLine("Setting state browsing");
            state = BusinessLayer.stateEnum.BrowsingWallpapers;
        }

        public void SetLaunchAgentToReflectPrefs()
        {
            if (Prefs.runAtLogin) MacOShelpers.SetAsLaunchAgent();
            else MacOShelpers.RemoveLaunchAgent();
        }

    }
}

