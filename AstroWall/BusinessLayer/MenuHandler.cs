using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;
using AstroWall.ApplicationLayer;
using AstroWall.BusinessLayer.Preferences;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Handles all menu operations.
    /// </summary>
    internal class MenuHandler
    {
        // Refs to other class instances.
        private AppDelegate appDelegate;
        private ApplicationHandler appHandler;

        // Icon related
        private System.Threading.Timer iconUpdateTimer;
        private int flipCounter;
        private int rotDegOffset;
        private bool iconCounterGoingDown = true;
        private AutoResetEvent autoEvent;

        // Browsing state
        private CancellationTokenSource taskCancellationSource;
        private CancellationToken cancellationToken;

        // Log
        private Action<string> log = Logging.GetLogger("Menu handler");

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuHandler"/> class.
        /// </summary>
        /// <param name="del"></param>
        /// <param name="app"></param>
        internal MenuHandler(AppDelegate del, ApplicationHandler app)
        {
            appDelegate = del;
            appHandler = app;
            RenewCancellationSource();
        }

        /// <summary>
        /// Creates status bar in application layer.
        /// </summary>
        /// <param name="title"></param>
        internal void CreateStatusBar(string title)
        {
            appDelegate.CreateStatusBar(title);
            NoAutoEnableMenuItems();
        }

        /// <summary>
        /// Updates menu checkmarks to reflect prefs.
        /// </summary>
        internal void UpdateMenuCheckMarksToReflectPrefs()
        {
            appDelegate.EnableAllUpdateSubMenuItems(appHandler.Prefs.CheckUpdatesOnStartup);
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        /// <summary>
        /// Disables all menu items except quit.
        /// </summary>
        internal void DisableAllItems()
        {
            appDelegate.DisableAllItemsExceptQuit();
        }

        /// <summary>
        /// Menu items not autoenabled.
        /// </summary>
        internal void NoAutoEnableMenuItems()
        {
            appDelegate.NoAutoEnableMenuItems();
        }

        /// <summary>
        /// Enables status bar icon.
        /// </summary>
        internal void EnableStatusIcon()
        {
            appDelegate.EnableStatusIcon();
        }

        /// <summary>
        /// Disables status bar icon.
        /// </summary>
        internal void DisableStatusIcon()
        {
            appDelegate.DisableStatusIcon();
        }

        /// <summary>
        /// Sets subtitle of menu to "initializing".
        /// </summary>
        internal void SetSubTitleInitialising()
        {
            appDelegate.SetSubTitle("Initializing...");
        }

        /// <summary>
        /// Sets subtitle of menu.
        /// </summary>
        /// <param name="msg"></param>
        internal void SetSubTitle(string msg)
        {
            appDelegate.SetSubTitle(msg);
        }

        /// <summary>
        /// Runs download animation.
        /// </summary>
        internal void RunDownloadIconAnimation()
        {
            log("Download animation about to start");
            if (iconUpdateTimer != null)
            {
                iconUpdateTimer.Dispose();
            }

            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            iconCounterGoingDown = false;

            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
                OnTimedEventDownloadAnimation,
                null,
                0,
                50);
        }

        /// <summary>
        /// Runs spinner animation.
        /// </summary>
        internal void RunSpinnerIconAnimation()
        {
            log("Spinner animation about to start");
            if (iconUpdateTimer != null)
            {
                iconUpdateTimer.Dispose();
            }

            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            iconCounterGoingDown = false;

            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
                OnTimedEventSpinnerAnimation,
                null,
                0,
                17);
        }

        /// <summary>
        /// Sets icon to default icon.
        /// </summary>
        /// <returns></returns>
        internal async Task SetIconToDefault()
        {
            iconUpdateTimer.Dispose();

            // Makes sure that one of the timer will not update icon
            // after this function
            await Task.Delay(50);
            log("Setting icon to default");

            if (appHandler.State.IsIdle)
            {
                // Double check that new process has not been started
                appDelegate.ChangeIconTo("MainIcon_rot_400");
                flipCounter = 0;
            }
        }

        /// <summary>
        /// Populates submenu with latest items.
        /// </summary>
        /// <param name="imgWrapList">Images to populate into submenu. Is filtered to only include those where an image and data is loaded.</param>
        /// <param name="stateRef"></param>
        internal void PopulateSubmenuLatestPictures(List<ImgWrap> imgWrapList, State stateRef)
        {
            // Clear existing items of menu
            ClearAllPictureItemsInSubmenu();

            foreach (ImgWrap iw in imgWrapList)
            {
                string title = iw.Title;

                if (iw.OnlineDataAndImgIsLoaded && iw.Integrity)
                {
                    appDelegate.AddPictureSubmenuItemAndRegEventHandlers(
                        title,
                        stateRef,
                        iw.PreviewIsLoaded,
                        CancelEndBrowsingState,
                        () => BusinessLayer.Wallpaper.Wallpaper.SetPreviewWallpaper(iw),
                        () => EndBrowsingWithDelay(),
                        async () =>
                        {
                            // Task wrap to run un non-UI thread
                            appHandler.State.UnsetStateBrowsingWallpapers();
                            appHandler.Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(iw);
                        });
                }
            }
        }

        /// <summary>
        /// Opens page to current image.
        /// </summary>
        internal void OpenUrlToCurrentImage()
        {
            General.Open(appHandler.Prefs.CurrentAstroWallpaper.PageUrl);
        }

        /// <summary>
        /// Opens url to current credits.
        /// </summary>
        internal void OpenUrlToCurrentCredits()
        {
            General.Open(appHandler.Prefs.CurrentAstroWallpaper.CreditUrl);
        }

        /// <summary>
        /// Opens current image.
        /// </summary>
        internal void OpenCurrentPic()
        {
            string tmpPth = FileHelpers.GenTmpCopy(appHandler.Prefs.CurrentAstroWallpaper.ImgLocalUrl);
            General.Open(tmpPth);
        }

        /// <summary>
        /// Called from application layer and responds to changes in "Run at login" option.
        /// </summary>
        /// <param name="newState"></param>
        internal void ChangedInMenuRunAtLogin(bool newState)
        {
            appHandler.Prefs.RunAtStartup = newState;
            appHandler.State.SetLaunchAgentToReflectPrefs();
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        /// <summary>
        /// Responds to changes to "auto install updates" made in menu by user.
        /// </summary>
        internal void ChangedInMenuAutoInstallUpdates(bool newState)
        {
            appHandler.Prefs.AutoInstallUpdates = newState;
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        /// <summary>
        /// Responds to changes to "check updates at startup" made in menu by user.
        /// </summary>
        internal void ChangedInMenuCheckUpdatesAtStartup(bool newState)
        {
            appHandler.Prefs.CheckUpdatesOnStartup = newState;
            if (newState)
            {
                appHandler.Updates.RegisterWakeHandler();
            }
            else
            {
                appHandler.Updates.UnregisterWakeHandler();
            }

            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        /// <summary>
        /// Responds to changes to "Daily check for new pic" made in menu by user.
        /// </summary>
        internal void ChangedInMenuDailyCheckNewest(bool newState)
        {
            log("Daily check changed to newest: " + newState);
            appHandler.Prefs.DailyCheck = newState ? DailyCheckEnum.Newest : DailyCheckEnum.None;
            appHandler.Wallpaper.SetDailyCheckToNewest(newState);
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        /// <summary>
        /// Responds to user clicked in menu on "Check for new pic" (now).
        /// </summary>
        internal async void ClickedInMenuManualCheckUpdates()
        {
            await appHandler.Updates.CheckForUpdates(true);
        }

        /// <summary>
        /// Deactivates update options in menu.
        /// </summary>
        internal void DeactivateUpdateOptions()
        {
            appHandler.AppDelegate.DeactivateUpdateOptions();
        }

        /// <summary>
        /// Method to react to request for manual check for new pics.
        /// </summary>
        internal async void ClickedInMenuManualCheckForNewPic()
        {
            await this.appHandler.CheckForNewPics();
        }

        /// <summary>
        /// Hides subtitle in menu.
        /// </summary>
        internal void HideSubtitle()
        {
            this.appHandler.AppDelegate.HideSubTitle();
        }

        /// <summary>
        /// Launch about window.
        /// </summary>
        internal void LaunchAboutWindow()
        {
            this.appHandler.AppDelegate.LaunchAboutWindow();
        }

        private void RenewCancellationSource()
        {
            this.taskCancellationSource = new CancellationTokenSource();
            this.cancellationToken = taskCancellationSource.Token;
        }

        private void OnTimedEventDownloadAnimation(object stateInfo)
        {
            string iconName = "MainIcon_download_" + flipCounter;
            appDelegate.ChangeIconTo(iconName);
            if (flipCounter == 0)
            {
                iconCounterGoingDown = false;
            }

            if (flipCounter == 9)
            {
                iconCounterGoingDown = true;
            }

            flipCounter = iconCounterGoingDown ? flipCounter - 1 : flipCounter + 1;
        }

        private void OnTimedEventSpinnerAnimation(object stateInfo)
        {
            int iconRotationDeg = 400 - (flipCounter * 7);

            // made wrong calculation in naming the files,
            // so these if clauses are needed...
            if (iconRotationDeg > 204)
            {
                rotDegOffset = 0;
            }

            if (iconRotationDeg <= 204)
            {
                rotDegOffset = 2;
            }

            if (iconRotationDeg <= 3)
            {
                rotDegOffset = -1;
            }

            string iconName = "MainIcon_rot_" + (iconRotationDeg + rotDegOffset);

            appDelegate.ChangeIconTo(iconName);
            if (iconRotationDeg + rotDegOffset == 0)
            {
                iconCounterGoingDown = true;
            }

            if (iconRotationDeg + rotDegOffset == 400)
            {
                iconCounterGoingDown = false;
            }

            flipCounter = iconCounterGoingDown ? flipCounter - 1 : flipCounter + 1;
        }

        /// <summary>
        /// Unsets the state from browsing wallpapers and resets wallpaper.
        /// </summary>
        private async Task EndBrowsingWithDelay()
        {
            await Task.Delay(100, this.cancellationToken);
            appHandler.Wallpaper.ResetWallpaper();
            appHandler.State.UnsetStateBrowsingWallpapers();
        }

        /// <summary>
        /// Cancels the "delayed end browsing".
        /// </summary>
        private void CancelEndBrowsingState()
        {
            this.taskCancellationSource.Cancel();
            RenewCancellationSource();
        }

        /// <summary>
        /// Clears all submenu items in "latest images".
        /// </summary>
        private void ClearAllPictureItemsInSubmenu()
        {
            appDelegate.ClearAllPictureItemsInSubmenu();
        }
    }
}