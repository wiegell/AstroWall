using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using GameController;


namespace AstroWall.ApplicationLayer
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {

        // Platform independent handler
        AstroWall.BusinessLayer.ApplicationHandler appHandler;

        public AppDelegate()
        {
            appHandler = new AstroWall.BusinessLayer.ApplicationHandler(this);
        }

        #region Override Methods
        public async override void DidFinishLaunching(NSNotification notification)
        {
            await appHandler.Init();
        }

        public void waitForUserToChosePrefs(Func<Preferences, Task> callback)
        {
            // Launch prefs always on top window
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
            appHandler.Terminate();
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

