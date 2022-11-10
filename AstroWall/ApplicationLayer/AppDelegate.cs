using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer;
using Foundation;
using GameController;


namespace AstroWall.ApplicationLayer
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {

        // Refs
        AstroWall.BusinessLayer.ApplicationHandler appHandler;
        AstroWall.BusinessLayer.MenuHandler menuHandler;

        // Keep windows in memory
        NSWindowController updatePromptWindowController;
        NSWindowController freshInstallWindowController;

        public AppDelegate()
        {
            appHandler = new AstroWall.BusinessLayer.ApplicationHandler(this);
            menuHandler = appHandler.MenuHandler;
        }

        #region Override Methods
        public async override void DidFinishLaunching(NSNotification notification)
        {
            await appHandler.Init();
        }

        public void waitForUserToChosePrefs(Func<BusinessLayer.Preferences, Task> callback)
        {
            // Launch prefs always on top window
            var storyboard = NSStoryboard.FromName("Main", null);
            freshInstallWindowController = storyboard.InstantiateControllerWithIdentifier("updateswindowcontroller") as NSWindowController;
            var window = freshInstallWindowController.Window;
            window.Delegate = new UpdatesWindowDelegate(window);
            var view = ((FreshInstallViewController)freshInstallWindowController.ContentViewController.View);
            view.regSaveCallback(callback);
            freshInstallWindowController.ShowWindow(freshInstallWindowController);
            freshInstallWindowController.Window.Level = NSWindowLevel.Status;
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        }

        public void launchUpdatePrompt(UpdateLibrary.Release rel, Action<UpdatePromptResponse> callback)
        {
            // Launch prefs always on top window
            var storyboard = NSStoryboard.FromName("Main", null);
            updatePromptWindowController = storyboard.InstantiateControllerWithIdentifier("updatespromptwindowcontroller") as NSWindowController;
            var window = updatePromptWindowController.Window;
            var view = ((UpdaterPrompViewController)updatePromptWindowController.ContentViewController.View);
            view.SetRelease(rel);
            view.RegChoiceCallback(callback);
            updatePromptWindowController.ShowWindow(updatePromptWindowController);
            window.OrderFront(null);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
            appHandler.TerminationPreparations();
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

