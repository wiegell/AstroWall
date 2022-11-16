using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer;
using AstroWall.BusinessLayer.Preferences;
using Foundation;



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
        NSWindowController postProcessPromptWindowController;
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

        public void waitForUserToChosePrefs(Func<Preferences, Task> callback)
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
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        }

        public void launchPostProcessPrompt(Preferences oldPrefs, Action<AddText> callbackWithNewPostProcess)
        {
            // Don't spawn new window if already opened
            if (!checkIfWindowIsAlreadyOpened(postProcessPromptWindowController))
            {

                // Launch prefs always on top window
                var storyboard = NSStoryboard.FromName("Main", null);
                postProcessPromptWindowController = storyboard.InstantiateControllerWithIdentifier("postprocesswindowcontroller2") as NSWindowController;
                var window = postProcessPromptWindowController.Window;
                var splitViewController = ((NSSplitViewController)postProcessPromptWindowController.ContentViewController);
                var contentView = (PostProcessTextSettings)splitViewController.SplitViewItems[1].ViewController.View;
                //view.SetRelease(rel);
                //view.RegChoiceCallback(callback);
                contentView.setData(oldPrefs.AddTextPostProcess);
                contentView.regChangePrefsCallback(callbackWithNewPostProcess);
                postProcessPromptWindowController.ShowWindow(postProcessPromptWindowController);
                window.OrderFront(null);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            }
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
            appHandler.TerminationPreparations();
        }
        #endregion

        private bool checkIfWindowIsAlreadyOpened(NSWindowController windowController)
        {
            if (windowController == null) return false;
            if (!windowController.Window.IsVisible) return false;
            if (windowController.Window.IsVisible)
            {
                windowController.Window.OrderFront(null);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

                return true;
            }
            else throw new Exception("should never be reached");
        }




    }
}

