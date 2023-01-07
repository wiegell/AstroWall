using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using AstroWall.ApplicationLayer;
using AstroWall.BusinessLayer.Preferences;
using Foundation;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Main entrypoint for all business operations.
    /// The business layer is OS-nonspecific and should not
    /// contain OS-specific types. Down the road it might
    /// be split into separate package to prepare for cross platform.
    /// Contains refs to all subclasses of business layer.
    /// </summary>
    internal class ApplicationHandler
    {
        // Log
        private readonly Action<string> log = Logging.GetLogger("App handler");
        private readonly Action<string> logError = Logging.GetLogger("App handler", true);

        // Misc
        private string currentVersionStringWithCommit;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationHandler"/> class.
        /// Initializes MenuHandler and Updates instances, also see Init() method.
        /// </summary>
        /// <param name="del">Takes an appdelegate as argument (needed to ref back to application layer).</param>
        public ApplicationHandler(AppDelegate del)
        {
            AppDelegate = del;
            MenuHandler = new MenuHandler(AppDelegate, this);
            currentVersionStringWithCommit = General.CurrentVersionLongWithCommit();
            Updates = new Updates(this, currentVersionStringWithCommit);
        }

        /// <summary>
        /// Gets a value indicating whether not installed on correct location.
        /// </summary>
        internal bool IncorrectInstallPath { get; private set; }

        /// <summary>
        /// Gets application layer main class.
        /// </summary>
        internal AppDelegate AppDelegate { get; private set; }

        /// <summary>
        /// Gets business layer menu handler.
        /// </summary>
        internal MenuHandler MenuHandler { get; private set; }

        /// <summary>
        /// Gets business layer wallpaper instance.
        /// </summary>
        internal Wallpaper.Wallpaper Wallpaper { get; private set; }

        /// <summary>
        /// Gets business layer application state.
        /// </summary>
        internal State State { get; private set; }

        /// <summary>
        /// Gets business layer updates class.
        /// </summary>
        internal Updates Updates { get; private set; }

        /// <summary>
        /// Gets business layer database (of already loaded images).
        /// </summary>
        internal Database DB { get; private set; }

        /// <summary>
        /// Gets business layer user preferences.
        /// </summary>
        internal Preferences.Preferences Prefs { get; private set; }

        /// <summary>
        /// Evaluates install location, launches primaryInit() and then secondaryInit().
        /// </summary>
        /// <returns>Task that completes when initialized.</returns>
        internal async Task Init()
        {
            // Evaluate install location
            this.IncorrectInstallPath = CheckIfInstallLocationIsIncorrect();
            this.AppDelegate.UpdatesDisabled = IncorrectInstallPath;

            nint installLocationCheck = -1;
            if (IncorrectInstallPath)
            {
                installLocationCheck = PromptUserToChangeInstallLocation();
            }

            if (installLocationCheck == 1000)
            {
                // User agrees to move application to user applications folder
                General.MoveBundleToUserApplicationsFolder();
                General.Relaunch();
                System.Diagnostics.Process.GetCurrentProcess().Kill();

                // Sleep until killed
                Thread.Sleep(10000);
            }

            log("Starting primary init");
            bool prefsAreLoadedSuccessfully = PrimaryInitAndCheckIfPrefsAreAvail();

            log("Starting sec. init");
            if (prefsAreLoadedSuccessfully)
            {
                await SecondaryInit(null);
            }
            else
            {
                State.SetStateChoosePrefs();

                // This will only fire if fresh install or user deleted prefs
                AppDelegate.WaitForUserToChosePrefs(SecondaryInit);
            }
        }

        /// <summary>
        /// What to do before terminating, saves prefs and DB to disk.
        /// </summary>
        internal void TerminationPreparations()
        {
            log("Terminate called");
            try
            {
                DB.SaveToDisk();
                Prefs.SaveToDisk();
            }
            catch (Exception)
            {
                logError("Could not save prefs and db (expected if terminated during startup update)");
            }
        }

        /// <summary>
        /// Checks if new pics are available online and downloads them, then does post process and sets to screens.
        /// </summary>
        /// <returns>Task.</returns>
        internal async Task CheckForNewPics()
        {
            // Cache latest
            var tmp = DB.Latest;

            // Set state downloading
            State.SetStateDownloading("Checking for new pics...");

            // Update db from online site
            bool successfullOnlinCheck = await DB.LoadDataButNoImgFromOnlineStartingAtDate(20, DateTime.Now);
            if (successfullOnlinCheck)
            {
                // Register successfull check in prefs
                Prefs.LastOnlineImageCheck = DateTime.Now;

                DB.Sort();
                State.SetStateDownloading("Downloading pictures...");
                await DB.LoadImgs();

                // Update submenu
                log("Updating menu");
                MenuHandler.PopulateSubmenuLatestPictures(DB.GetGettableImages(), State);
                State.UnsetStateDownloading();

                // Run postProcess and set latest
                if (DB.Latest != null && !DB.Latest.Equals(tmp))
                {
                    await Wallpaper.RunPostProcessAndSetWallpaperAllScreens(DB.Latest);
                }
            }

            State.UnsetStateDownloading();
        }

        /// <summary>
        /// Calls application layer to prompt user to change install path.
        /// </summary>
        /// <returns>return -1 on correct install path, otherwise returns
        /// nint response from modal.</returns>
        private nint PromptUserToChangeInstallLocation()
        {
            // TODO should be generalized to be non-OS-specific.
            log("Install location not suited for updates, prompting user to move");
            return AppDelegate.LaunchIncorrectInstallPathAlert();
        }

        /// <summary>
        /// Checks if install location is correct.
        /// </summary>
        private bool CheckIfInstallLocationIsIncorrect()
        {
            string installPath = General.GetInstallPath();
            string wantedInstallPath = General.WantedBundleInstallPathInUserApplications();
            log("Wanted install path: " + wantedInstallPath);
            log("Current install path: " + installPath);
            return installPath != wantedInstallPath;
        }

        /// <summary>
        /// Initializes status bar icon, application state and preferences.
        /// </summary>
        /// <returns>True if prefs are loaded successfully from file.</returns>
        private bool PrimaryInitAndCheckIfPrefsAreAvail()
        {
            // Create status bar icon / menu
            MenuHandler.CreateStatusBar("Astrowall v" + Updates.CurrentVersion);

            // Init state
            State = new State(this, currentVersionStringWithCommit);

            // Load prefs. If non-present halt further actions until
            // preft are confirmed by user
            Prefs = Preferences.Preferences.FromSave;
            bool prefsAreLoadedSuccessfully = Prefs != null;
            return prefsAreLoadedSuccessfully;
        }

        /// <summary>
        /// Checks for pending updates, sets prefs if not already set, sets launch agent, initializes db, populates menu, checks for new images, sets wake handler.
        /// </summary>
        /// <param name="prefsFromPostInstallPrompt"></param>ns>
        private async Task SecondaryInit(Preferences.Preferences prefsFromPostInstallPrompt)
        {
            // Check for updates.
            if (!IncorrectInstallPath)
            {
                // Is checked syncronously to be able to push updates before app crashes
                log("Correct install path, can check for updates at startup");
                Task updateChecking = Task.Run(async () =>
                {
                    try
                    {
                        // Override prefs if postInstallPrompt has been fired
                        await Updates.ConsiderCheckingForUpdates(runAtOnce: true, overridePrefs: prefsFromPostInstallPrompt != null);
                    }
                    catch (Exception ex)
                    {
                        logError("Exception in update check on thread: " + Environment.CurrentManagedThreadId);
                        logError("Ex: " + ex.GetType() + ", " + ex.Message);

                        // Rethrow to UI thread
                        Exception newEx = new InvalidOperationException("Exception in update check", ex);
                        General.RunOnUIThread(() =>
                        {
                            throw ex;
                        });

                        // This will not bubble up
                        throw newEx;
                    }
                });
                updateChecking.Wait();
                log("Update checking done");
            }

            // Set prefs from post-install welcome screen,
            // if calls comes from there.
            // If prefs == null it means they are not created
            // but instead already loaded to state from json.
            if (prefsFromPostInstallPrompt != null)
            {
                Prefs = prefsFromPostInstallPrompt;
            }

            Wallpaper = new Wallpaper.Wallpaper(this);
            MenuHandler.UpdateMenuCheckMarksToReflectPrefs();

            // Set run at login agent
            log("Set launch agent to reflect prefs");
            State.SetLaunchAgentToReflectPrefs();

            // Init state
            State.SetStateInitializing();

            // Init db
            log("Init db");
            DB = new Database();

            // Populate submenu
            log("Populating menu");
            MenuHandler.PopulateSubmenuLatestPictures(DB.GetGettableImages(), State);

            // Check for new pics
            if (Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                log("Check for pics");
                await CheckForNewPics();
                log("Run post process");
                Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(DB.Latest);
            }

            // Update menus if updates are avail. and set wake handler
            if (!IncorrectInstallPath)
            {
                if (Prefs.CheckUpdatesOnStartup)
                {
                    log("Install path ok and updates on startup enabled, setting wake handler");
                    Updates.RegisterWakeHandler();
                }
            }
            else
            {
                DisableUpdateOptions();
            }

            // Check if wallpaper wakehandler and noon check
            // should be set
            switch (Prefs.DailyCheck)
            {
                case DailyCheckEnum.Newest:
                    {
                        log("Setting wallpaper wake handler");
                        Wallpaper.RegisterWakeHandler();
                        log("Setting wallpaper noon check");
                        Wallpaper.CreateNoonCheck();
                        break;
                    }

                case DailyCheckEnum.None:
                    {
                        break;
                    }
            }

            State.UnsetStateInitializing();
        }

        /// <summary>
        /// Disables update options in prefs and UI.
        /// </summary>
        private void DisableUpdateOptions()
        {
            Prefs.AutoInstallUpdates = false;
            Prefs.CheckUpdatesOnStartup = false;
            MenuHandler.DeactivateUpdateOptions();
        }
    }
}