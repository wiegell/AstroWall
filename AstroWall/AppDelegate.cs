using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;

using Foundation;
using GameController;


namespace AstroWall
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }
        private NSStatusBar statusBar;
        private NSStatusItem statusBarItem;
        private State state;

        #region Override Methods
        public async override void DidFinishLaunching(NSNotification notification)
        {
            // Create a Status Bar Menu
            statusBar = NSStatusBar.SystemStatusBar;
            statusBarItem = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            MacOShelpers.InitIcon(statusBarItem, this.StatusMenu);
            string versionString = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            MenuTitle.Title = "Astrowall v" + versionString;

            // Init state
            state = new State(this.StatusMenu, statusBarItem, versionString);

            // Load prefs. If non-present halt further actions until
            // preft are confirmed by user
            bool prefsAreLoadedSuccessfully = state.LoadPreftFromSave();
            Console.WriteLine("Prefs not found, creating new ones");

            // Define delegate to use as callback
            Func<Task> continueSetup = async () =>
            {
                state.SetStateInitializing();
                state.LoadOrCreateDB();

                // Check online site
                await state.UpdateStateFromOnline();

                // Populate menu
                state.PopulateMenu();

                // Give back control to the user
                state.SetStateIdle();

                // Check for updates
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                state.FireUpdateHandler();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            };

            if (prefsAreLoadedSuccessfully) await continueSetup();
            else
            {
                state.SetStateChoosePrefs();
                waitForUserToChosePrefs(continueSetup);
            }
        }

        private void waitForUserToChosePrefs(Func<Task> callback)
        {
            var storyboard = NSStoryboard.FromName("Main", null);
            var windowController = storyboard.InstantiateControllerWithIdentifier("updateswindowcontroller") as NSWindowController;
            var window = windowController.Window;
            window.Delegate = new UpdatesWindowDelegate(window);
            var view = ((FreshInstallViewController)windowController.ContentViewController.View);
            view.regSaveCallback(callback);
            windowController.ShowWindow(windowController);
            windowController.Window.Level = NSWindowLevel.Status;
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
            Console.WriteLine("term called");
            state.saveDBToDisk();
            state.savePrefsToDisk();
        }
        #endregion

        partial void MenuManualCheckPic(Foundation.NSObject sender)
        {
            //state.setStateIdle();
            //MacOShelpers.InitIcon2(statusBarItem, this.StatusMenu);
            //string imgurl = HTMLHelpers.getImgUrl();
            //Task<string> tmpFilePath = FileHelpers.DownloadUrlToTmpPath(imgurl);
            ////MacOShelpers.SetWallpaper(tmpFilePath);
            //Console.WriteLine("file dl");
            //MacOShelpers.RunPKGUpdate();
        }


    }
}

