﻿using System;
using System.Threading.Tasks;
using AstroWall.ApplicationLayer;
using Foundation;
using AstroWall.BusinessLayer.Preferences;
using System.Threading;

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

        // Misc
        private string currentVersionStringWithCommit;


        public ApplicationHandler(AppDelegate del)
        {
            AppDelegate = del;
            MenuHandler = new MenuHandler(AppDelegate, this);
            currentVersionStringWithCommit = General.currentVersion();
            Updates = new Updates(this, currentVersionStringWithCommit);
        }

        public async Task Init()
        {
            bool prefsAreLoadedSuccessfully = primaryInitAndCheckIfPrefsAreAvail();

            if (prefsAreLoadedSuccessfully) await secondaryInit(null);
            else
            {
                State.SetStateChoosePrefs();
                // This will only fire if fresh install or user deleted prefs
                AppDelegate.waitForUserToChosePrefs(secondaryInit);
            }
        }

        public void TerminationPreparations()
        {
            Console.WriteLine("Terminate called");
            db.SaveToDisk();
            Prefs.SaveToDisk();
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

            // Check for updates
            // Don't wait on result
            Task updateChecking = Task.Run(async () =>
            {
                await Updates.ConsiderCheckingForUpdates();
            });
            if (Prefs.CheckUpdatesOnStartup)
                Updates.registerWakeHandler();

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

