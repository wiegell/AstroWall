using System;
using AppKit;
using System.Collections.Generic;
using AstroWall.ApplicationLayer;
using System.Timers;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Reflection;
using AstroWall.BusinessLayer.Preferences;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// (Almost) platform independent handling of the menu
    /// </summary>
    public class MenuHandler
    {
        // Refs
        AppDelegate appDelegate;
        ApplicationHandler appHandler;

        // Icon related
        private static System.Threading.Timer iconUpdateTimer;
        private int flipCounter;
        private int rotDegOffset;
        // Which way is the counter going
        private bool goingdown = true;
        private AutoResetEvent autoEvent;

        // Browsing state
        private CancellationTokenSource taskCancellationSource;
        private CancellationToken cancellationToken;

        // Log
        private Action<string> log = Logging.GetLogger("Menu handler");

        internal MenuHandler(AppDelegate del, ApplicationHandler app)
        {
            appDelegate = del;
            appHandler = app;
            RenewCancellationSource();
        }

        public void createStatusBar(string title)
        {
            appDelegate.CreateStatusBar(title);
            noAutoEnableMenuItems();
        }

        public void updateMenuCheckMarksToReflectPrefs()
        {
            appDelegate.EnableAllUpdateSubMenuItems(appHandler.Prefs.CheckUpdatesOnStartup);
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public void DisableAllItems()
        {
            appDelegate.DisableAllItemsExceptQuit();
        }

        public void noAutoEnableMenuItems()
        {
            appDelegate.NoAutoEnableMenuItems();
        }

        public void EnableStatusIcon()
        {
            appDelegate.EnableStatusIcon();
        }

        public void DisableStatusIcon()
        {
            appDelegate.DisableStatusIcon();
        }

        public void SetSubTitleInitialising()
        {
            appDelegate.SetSubTitle("Initializing...");
        }
        public void SetSubTitle(string msg)
        {
            appDelegate.SetSubTitle(msg);
        }

        public void RunDownloadIconAnimation()
        {
            log("Download animation about to start");
            if (iconUpdateTimer != null) iconUpdateTimer.Dispose();
            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            goingdown = false;
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
      OnTimedEventDownloadAnimation,
      null,
      0,
      50);
        }

        public void RunSpinnerIconAnimation()
        {
            log("Spinner animation about to start");
            if (iconUpdateTimer != null) iconUpdateTimer.Dispose();
            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            goingdown = false;
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
                OnTimedEventSpinnerAnimation,
                null,
                0,
                17);

        }

        public async Task SetIconToDefault()
        {
            iconUpdateTimer.Dispose();

            // Makes sure that one of the timer will not update icon
            // after this function
            await Task.Delay(50);
            log("Setting icon to default");

            if (appHandler.State.isIdle)
            {
                // Double check that new process has not been started
                appDelegate.ChangeIconTo("MainIcon_rot_400");
                flipCounter = 0;
            }
        }

        public void PopulateSubmenuLatestPictures(List<ImgWrap> imgWrapList, State stateRef)
        {

            // Clear existing items of menu
            ClearAllPictureItemsInSubmenu();

            foreach (ImgWrap iw in imgWrapList)
            {
                string title = iw.Title;

                if (iw.OnlineDataAndPicIsLoaded() && iw.Integrity)
                {
                    appDelegate.AddPictureSubmenuItemAndRegEventHandlers(
                        title,
                        stateRef,
                        iw.PreviewIsLoaded(),
                        CancelEndBrowsingState,
                        () => BusinessLayer.Wallpaper.Wallpaper.SetPreviewWallpaper(iw),
                        () => EndBrowsingWithDelay(),
                        async () =>
                        {
                            // Task wrap to run un non-UI thread
                            appHandler.State.UnsetStateBrowsingWallpapers();
                            appHandler.Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(iw);
                        }


                        );
                }
            }
        }

        public void OpenUrlToCurrentPic()
        {
            General.Open(appHandler.Prefs.CurrentAstroWallpaper.PageUrl);
        }

        public void OpenUrlToCurrentCredits()
        {
            General.Open(appHandler.Prefs.CurrentAstroWallpaper.CreditUrl);
        }

        public void OpenCurrentPic()
        {
            string tmpPth = FileHelpers.GenTmpCopy(appHandler.Prefs.CurrentAstroWallpaper.ImgLocalUrl);
            General.Open(tmpPth);
        }



        public void changedInMenuRunAtLogin(bool newState)
        {
            appHandler.Prefs.RunAtStartup = newState;
            appHandler.State.SetLaunchAgentToReflectPrefs();
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public void changedInMenuAutoInstallUpdates(bool newState)
        {
            appHandler.Prefs.AutoInstallUpdates = newState;
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public void changedInMenuCheckUpdatesAtStartup(bool newState)
        {
            appHandler.Prefs.CheckUpdatesOnStartup = newState;
            if (newState)
            {
                appHandler.Updates.registerWakeHandler();
            }
            else
            {
                appHandler.Updates.unregisterWakeHandler();
            }
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public void changedInMenuDailyCheckNewest(bool newState)
        {
            log("Daily check changed to newest: " + newState);
            appHandler.Prefs.DailyCheck = newState ? DailyCheckEnum.Newest : DailyCheckEnum.None;
            appHandler.Wallpaper.SetDailyCheckToNewest(newState);
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public async void ClickedInMenuManualCheckUpdates()
        {
            await appHandler.Updates.CheckForUpdates(true);
        }

        public void DeactivateUpdateOptions()
        {
            appHandler.AppDelegate.DeactivateUpdateOptions();
        }

        /// <summary>
        /// Method to react to request for manual check for new pics.
        /// </summary>
        internal async void ClickedInMenuManualCheckForNewPic()
        {
            await this.appHandler.checkForNewPics();
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
                goingdown = false;
            }

            if (flipCounter == 9)
            {
                goingdown = true;
            }

            flipCounter = goingdown ? flipCounter - 1 : flipCounter + 1;
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
                goingdown = true;
            }

            if (iconRotationDeg + rotDegOffset == 400) goingdown = false;
            flipCounter = goingdown ? flipCounter - 1 : flipCounter + 1;
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