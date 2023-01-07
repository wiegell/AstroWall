using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer;
using AstroWall.BusinessLayer.Preferences;
using CoreFoundation;
using Darwin;
using Foundation;
using ObjCRuntime;

namespace AstroWall.ApplicationLayer
{
#pragma warning disable CA1711

    /// <summary>
    /// Entry point of MacOS app and bootstrap of the business logic. Divided has partial .menu file
    /// with all the OS-specific handling about the menu.
    /// </summary>
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
#pragma warning restore CA1711
    {
        // Keep windows in state memory to prevent macOS from autoclosing them
        private NSWindowController updatePromptWindowController;
        private NSWindowController postProcessPromptWindowController;
        private NSWindowController aboutWindowController;
        private NSWindowController freshInstallWindowController;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDelegate"/> class.
        /// Default constructor.
        /// </summary>
        public AppDelegate()
        {
            this.AppHandler = new AstroWall.BusinessLayer.ApplicationHandler(this);
        }

        /// <summary>
        /// Gets AppHandler instance which is main entrance to all business logic.
        /// </summary>
        internal ApplicationHandler AppHandler { get; private set; }

        /// <summary>
        /// Macos callback for when app is loaded, used to start business init.
        /// </summary>
        public async override void DidFinishLaunching(NSNotification notification)
        {
            await this.AppHandler.Init();
        }

        /// <summary>
        /// Macos callback for when app should terminate.
        /// </summary>
        /// <param name="notification"></param>
        public override void WillTerminate(NSNotification notification)
        {
            AppHandler.TerminationPreparations();
        }

        /// <summary>
        /// Launches window that warns about wrong installation path.
        /// Used by business logic to disable updates if not corrected.
        /// </summary>
        /// <returns>Returns -1 on correct install path.</returns>
        internal static nint LaunchIncorrectInstallPathAlert()
        {
            var alert = new NSAlert();
            alert.MessageText = "Incorrect Astro Wall install location";
            alert.Icon = NSImage.GetSystemSymbol("folder.fill.badge.questionmark", null);
            alert.InformativeText = "Current install location not suited for updates. Please confirm move app to user applications folder. A link will be created in the regular applications folder";
            var okButton = alert.AddButton("Sure!");
            var cancelButton = alert.AddButton("No thanks");
            okButton.KeyEquivalent = "\r";

            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            return alert.RunModal();
        }

        /// <summary>
        /// Suspends app UI and opens a preference choosing window.
        /// Used mainly after fresh installation.
        /// </summary>
        /// <param name="callback"></param>
        internal void WaitForUserToChosePrefs(Func<Preferences, Task> callback)
        {
            // Launch prefs always on top window
            var storyboard = NSStoryboard.FromName("Main", null);
            freshInstallWindowController = storyboard.InstantiateControllerWithIdentifier("updateswindowcontroller") as NSWindowController;
            var window = freshInstallWindowController.Window;
            window.Delegate = new UpdatesWindowDelegate(window);
            var view = (FreshInstallViewController)freshInstallWindowController.ContentViewController.View;
            view.RegSaveCallback(callback);
            if (AppHandler.IncorrectInstallPath)
            {
                view.DisableUpdatesOptions();
            }

            freshInstallWindowController.ShowWindow(freshInstallWindowController);
            freshInstallWindowController.Window.Level = NSWindowLevel.Status;
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        }

        /// <summary>
        /// Launches update prompt window.
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="callback"></param>
        internal void LaunchUpdatePrompt(UpdateLibrary.Release rel, Action<UpdatePromptResponse> callback)
        {
            // Launch prefs always on top window
            var storyboard = NSStoryboard.FromName("Main", null);
            updatePromptWindowController = storyboard.InstantiateControllerWithIdentifier("updatespromptwindowcontroller") as NSWindowController;
            var window = updatePromptWindowController.Window;
            var view = (UpdaterPrompViewController)updatePromptWindowController.ContentViewController.View;
            view.SetRelease(rel);
            view.RegChoiceCallback(callback);
            updatePromptWindowController.ShowWindow(updatePromptWindowController);
            window.OrderFront(null);
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
        }

        /// <summary>
        /// Launches post process preference window.
        /// </summary>
        /// <param name="oldPrefs"></param>
        /// <param name="callbackWithNewPostProcess"></param>
        internal void LaunchPostProcessPrompt(Preferences oldPrefs, Action<AddTextPreference> callbackWithNewPostProcess)
        {
            // Don't spawn new window if already opened
            if (!CheckIfWindowIsAlreadyOpened(postProcessPromptWindowController))
            {
                // Launch prefs always on top window
                var storyboard = NSStoryboard.FromName("Main", null);
                postProcessPromptWindowController = storyboard.InstantiateControllerWithIdentifier("postprocesswindowcontroller2") as NSWindowController;
                var window = postProcessPromptWindowController.Window;
                var splitViewController = (NSSplitViewController)postProcessPromptWindowController.ContentViewController;
                var contentView = (PostProcessTextSettings)splitViewController.SplitViewItems[1].ViewController.View;
                contentView.SetData(oldPrefs.AddTextPostProcess);
                contentView.RegChangePrefsCallback(callbackWithNewPostProcess);
                postProcessPromptWindowController.ShowWindow(postProcessPromptWindowController);
                window.OrderFront(null);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            }
        }

        /// <summary>
        /// Launches about window.
        /// </summary>
        internal void LaunchAboutWindow()
        {
            // Don't spawn new window if already opened
            if (!CheckIfWindowIsAlreadyOpened(aboutWindowController))
            {
                // Launch about always on top window
                var storyboard = NSStoryboard.FromName("Main", null);
                aboutWindowController = storyboard.InstantiateControllerWithIdentifier("aboutwindowcontroller3") as NSWindowController;
                var window = aboutWindowController.Window;
                var view = window.ContentView;
                aboutWindowController.ShowWindow(aboutWindowController);
                window.OrderFront(null);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            }
        }

        /// <summary>
        /// Checks if window is already open to prevent multiple instances
        /// of same window.
        /// </summary>
        /// <returns>True if already open.</returns>
        private static bool CheckIfWindowIsAlreadyOpened(NSWindowController windowController)
        {
            if (windowController == null)
            {
                return false;
            }

            if (!windowController.Window.IsVisible)
            {
                return false;
            }
            else
            {
                windowController.Window.OrderFront(null);
                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);

                return true;
            }
        }
    }
}