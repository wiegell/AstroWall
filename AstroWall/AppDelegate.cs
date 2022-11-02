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
            string version = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
            MenuTitle.Title = "Astrowall v" + version;

            // Init state
            state = new State(this.StatusMenu, statusBarItem);
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

