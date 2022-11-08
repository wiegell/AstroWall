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

        // Misc
        private string currentVersionString;


        public ApplicationHandler(AppDelegate del)
        {
            AppDelegate = del;
            MenuHandler = new MenuHandler(AppDelegate, this);
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

        public void Terminate()
        {
            Console.WriteLine("Terminate called");
            State.saveDBToDisk();
            State.savePrefsToDisk();
        }

        private bool primaryInitAndCheckIfPrefsAreAvail()
        {
            // Create status bar icon / menu
            currentVersionString = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            MenuHandler.createStatusBar("Astrowall v" + currentVersionString);

            // Init state
            State = new State(MenuHandler, currentVersionString);

            // Load prefs. If non-present halt further actions until
            // preft are confirmed by user
            bool prefsAreLoadedSuccessfully = State.LoadPrefsFromSave();
            return prefsAreLoadedSuccessfully;
        }

        private async Task secondaryInit(Preferences prefs)
        {
            // Set prefs from post-install welcome screen,
            // if calls comes from there.
            // If prefs == null it means they are not created
            // but instead already loaded to state from json.
            if (prefs != null)
            {
                State.setPrefs(prefs);
            };
            Wallpaper = new Wallpaper(State.Prefs);
            MenuHandler.updateMenuCheckMarksToReflectState(State);

            // Set run at login
            State.SetLaunchAgentToReflectPrefs();
            MenuHandler.changeLoginMenuItemState(State.Prefs.runAtLogin);

            // Init state and db
            State.SetStateInitializing();
            State.LoadOrCreateDB();

            // Check online site
            await State.UpdateStateFromOnline();

            // Populate submenu
            MenuHandler.PopulateSubmenuLatestPictures(State.getPresentableImages(), State);

            // Give back control to the user
            State.SetStateIdle();

            // Check for updates
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            State.FireUpdateHandler();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

    }
}

