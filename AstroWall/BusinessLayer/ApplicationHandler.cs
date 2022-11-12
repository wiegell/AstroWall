using System;
using System.Threading.Tasks;
using AstroWall.ApplicationLayer;
using Foundation;

namespace AstroWall.BusinessLayer
{
    public class ApplicationHandler
    {

        // Refs
        public AppDelegate AppDelegate { get; private set; }
        public MenuHandler MenuHandler { get; private set; }
        public Wallpaper Wallpaper { get; private set; }
        public State State { get; private set; }
        public Updates Updates { private set; get; }
        private Database db;
        public Preferences Prefs { get; private set; }

        // Misc
        public Version CurrentVersion { private set; get; }
        private string currentVersionString = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();


        public ApplicationHandler(AppDelegate del)
        {
            AppDelegate = del;
            MenuHandler = new MenuHandler(AppDelegate, this);
            Updates = new Updates(this, currentVersionString);
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
            MenuHandler.createStatusBar("Astrowall v" + currentVersionString);

            // Init state
            State = new State(this, currentVersionString);

            // Load prefs. If non-present halt further actions until
            // preft are confirmed by user
            Prefs = Preferences.fromSave();
            bool prefsAreLoadedSuccessfully = Prefs != null;
            return prefsAreLoadedSuccessfully;
        }

        private async Task secondaryInit(Preferences prefsFromPostInstallPrompt)
        {
            // Set prefs from post-install welcome screen,
            // if calls comes from there.
            // If prefs == null it means they are not created
            // but instead already loaded to state from json.
            if (prefsFromPostInstallPrompt != null)
            {
                Prefs = prefsFromPostInstallPrompt;
            };
            Wallpaper = new Wallpaper(this);
            MenuHandler.updateMenuCheckMarksToReflectPrefs();

            // Set run at login agent
            State.SetLaunchAgentToReflectPrefs();

            // Init state and db
            State.SetStateInitializing();
            db = new Database();

            // Populate submenu
            MenuHandler.PopulateSubmenuLatestPictures(db.getPresentableImages(), State);

            // Check for new pics
            await checkForNewPics();

            State.SetStateIdle();

            // Check for updates
            Updates.ConsiderCheckingForUpdates();
            if (Prefs.CheckUpdatesOnStartup)
                Updates.registerWakeHandler();

        }

        private async Task checkForNewPics()
        {

            // Set state downloading
            State.SetStateDownloading("Checking for new pics");
            // Update db from online site
            await db.LoadDataButNoImgFromOnlineStartingAtDate(10, DateTime.Now);
            db.Sort();
            State.SetStateDownloading("Downloading pictures");
            await db.LoadImgs();

            // Update submenu
            MenuHandler.PopulateSubmenuLatestPictures(db.getPresentableImages(), State);
        }

    }
}

