using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;

using Foundation;


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
            state.SetStateInitializing();

            // Load prefs and image collection from disk
            // Create if non-present
            state.LoadOrCreateDB();
            state.LoadOrCreatePrefs();

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

