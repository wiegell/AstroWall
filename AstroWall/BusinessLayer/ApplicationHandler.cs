using System;
using System.Threading.Tasks;
using AstroWall.ApplicationLayer;
using Foundation;
using AstroWall.BusinessLayer.Preferences;
using System.Threading;
using AppKit;

namespace AstroWall.BusinessLayer
{
    public class ApplicationHandler
    {

        // Refs
        public AppDelegate AppDelegate { get; private set; }
        public MenuHandler MenuHandler { get; private set; }
        public Wallpaper.Wallpaper Wallpaper { get; private set; }
        public State State { get; private set; }
        public Updates Updates { private set; get; }
        public Database db;
        public Preferences.Preferences Prefs { get; private set; }
        public bool IncorrectInstallPath { get; private set; }

        // Misc
        private string currentVersionStringWithCommit;

        // Log
        private Action<string> log = Logging.GetLogger("App handler");
        private Action<string> logError = Logging.GetLogger("App handler", true);


        public ApplicationHandler(AppDelegate del)
        {
            AppDelegate = del;
            MenuHandler = new MenuHandler(AppDelegate, this);
            currentVersionStringWithCommit = General.currentVersion();
            Updates = new Updates(this, currentVersionStringWithCommit);
        }

        public async Task Init()
        {
            // Evaluate install location
            this.IncorrectInstallPath = checkIfInstallLocationIsIncorrect();
            this.AppDelegate.UpdatesDisabled = IncorrectInstallPath;

            nint installLocationCheck = -1;
            if (IncorrectInstallPath) installLocationCheck = promptUserToChangeInstallLocation();
            if (installLocationCheck == 1000)
            {
                // User agrees to move application to user applications folder
                General.moveBundleToUserApplicationsFolder();
                General.Relaunch();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                // Sleep until killed
                Thread.Sleep(10000);
            }

            log("Starting primary init");
            bool prefsAreLoadedSuccessfully = primaryInitAndCheckIfPrefsAreAvail();

            log("Starting sec. init");
            if (prefsAreLoadedSuccessfully) await secondaryInit(null);
            else
            {
                State.SetStateChoosePrefs();
                // This will only fire if fresh install or user deleted prefs
                AppDelegate.waitForUserToChosePrefs(secondaryInit);
            }
        }

        /// <summary>
        /// return -1 on correct install path, otherwise returns
        /// nint response from modal
        /// </summary>
        /// <returns></returns>
        private nint promptUserToChangeInstallLocation()
        {
            log("Install location not suited for updates, prompting user to move");
            return this.AppDelegate.launchIncorrectInstallPathAlert();
        }

        private bool checkIfInstallLocationIsIncorrect()
        {
            string installPath = General.GetInstallPath();
            string wantedInstallPath = General.WantedBundleInstallPathInUserApplications();
            log("Wanted install path: " + wantedInstallPath);
            log("Current install path: " + installPath);
            return (installPath != wantedInstallPath);
        }

        public void TerminationPreparations()
        {
            log("Terminate called");
            try
            {
                db.SaveToDisk();
                Prefs.SaveToDisk();
            }
            catch (Exception ex)
            {
                logError("Could not save prefs and db (expected if terminated during startup update)");
            }
        }

        private bool primaryInitAndCheckIfPrefsAreAvail()
        {

            // Create status bar icon / menu
            MenuHandler.createStatusBar("Astrowall v" + Updates.currentVersion);

            // Init state
            State = new State(this, currentVersionStringWithCommit);

            // Load prefs. If non-present halt further actions until
            // preft are confirmed by user
            Prefs = Preferences.Preferences.fromSave();
            bool prefsAreLoadedSuccessfully = Prefs != null;
            return prefsAreLoadedSuccessfully;
        }

        private async Task secondaryInit(Preferences.Preferences prefsFromPostInstallPrompt)
        {

            // Check for updates.
            if (!IncorrectInstallPath)
            {
                // Is checked syncronously to be able to push updates before app crashes
                log("Checking for updates at startup");
                Task updateChecking = Task.Run(async () =>
                {
                    await Updates.ConsiderCheckingForUpdates(runAtOnce: true);
                });
                updateChecking.Wait();
            }

            // Set prefs from post-install welcome screen,
            // if calls comes from there.
            // If prefs == null it means they are not created
            // but instead already loaded to state from json.
            if (prefsFromPostInstallPrompt != null)
            {
                Prefs = prefsFromPostInstallPrompt;
            };
            Wallpaper = new Wallpaper.Wallpaper(this);
            MenuHandler.updateMenuCheckMarksToReflectPrefs();

            // Set run at login agent
            State.SetLaunchAgentToReflectPrefs();

            // Init state and db
            State.SetStateInitializing();
            db = new Database();

            // Populate submenu
            MenuHandler.PopulateSubmenuLatestPictures(db.getPresentableImages(), State);

            // Check for new pics
            if (Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                await checkForNewPics();
                Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(db.ImgWrapList[0]);
            }

            // Update menus if updates are avail. and set wake handler
            if (!IncorrectInstallPath)
            {
                if (Prefs.CheckUpdatesOnStartup)
                    Updates.registerWakeHandler();
            }
            else disableUpdateOptions();

            // Check if wallpaper wakehandler and noon check
            // should be set
            switch (Prefs.DailyCheck)
            {
                case DailyCheckEnum.Newest:
                    {
                        Wallpaper.registerWakeHandler();
                        Wallpaper.createNoonCheck();
                        break;
                    }
                case DailyCheckEnum.None:
                    {
                        break;
                    }
            }
            State.UnsetStateInitializing();
        }

        private void disableUpdateOptions()
        {
            Prefs.AutoInstallUpdates = false;
            Prefs.CheckUpdatesOnStartup = false;
            MenuHandler.DeactivateUpdateOptions();
        }

        public async Task checkForNewPics()
        {

            // Set state downloading
            State.SetStateDownloading("Checking for new pics...");
            // Update db from online site
            bool successfullOnlinCheck = await db.LoadDataButNoImgFromOnlineStartingAtDate(20, DateTime.Now);
            if (successfullOnlinCheck)
            {

                // Register successfull check in prefs
                Prefs.LastOnlineCheck = DateTime.Now;

                db.Sort();
                State.SetStateDownloading("Downloading pictures...");
                await db.LoadImgs();

                // Update submenu
                MenuHandler.PopulateSubmenuLatestPictures(db.getPresentableImages(), State);
                State.UnsetStateDownloading();
            }

            State.UnsetStateDownloading();
        }

    }
}

