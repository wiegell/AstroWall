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
        private int flipCounter = 0;
        private int rotDegOffset = 0;
        // Which way is the counter going
        private bool goingdown = true;
        private AutoResetEvent autoEvent;

        // Browsing state
        private Task restoreToIdleWithDelayTask;
        private CancellationTokenSource taskCancellationSource;
        private CancellationToken cancellationToken;

        public MenuHandler(AppDelegate del, ApplicationHandler app)
        {
            appDelegate = del;
            appHandler = app;
            renewCancellationSource();
        }

        public void createStatusBar(string title)
        {
            appDelegate.createStatusBar(title);
            noAutoEnableMenuItems();
        }

        public void updateMenuCheckMarksToReflectPrefs()
        {
            appDelegate.EnableAllUpdateSubMenuItems(appHandler.Prefs.CheckUpdatesOnStartup);
            appDelegate.UpdateMenuCheckMarks(appHandler.Prefs);
        }

        public void DisableAllItems()
        {
            appDelegate.disableAllItemsExceptQuit();
        }

        public void noAutoEnableMenuItems()
        {
            appDelegate.noAutoEnableMenuItems();
        }

        public void EnableStatusIcon()
        {
            appDelegate.enableStatusIcon();
        }

        public void DisableStatusIcon()
        {
            appDelegate.disableStatusIcon();
        }

        public void SetSubTitleInitialising()
        {
            appDelegate.setSubTitle("Initializing...");
        }
        public void SetSubTitle(string msg)
        {
            appDelegate.setSubTitle(msg);
        }

        public void RunDownloadIconAnimation()
        {
            Console.WriteLine("Download animation about to start");
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
            Console.WriteLine("Spinner animation about to start");
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
            Console.WriteLine("Setting icon to default");

            if (appHandler.State.isIdle)
            {
                // Double check that new process has not been started
                appDelegate.changeIconTo("MainIcon_rot_400");
                flipCounter = 0;
            }
        }

        public void PopulateSubmenuLatestPictures(List<ImgWrap> imgWrapList, State stateRef)
        {

            // Clear existing items of menu
            removeAllPictureItemsInSubmenu();

            foreach (ImgWrap iw in imgWrapList)
            {
                string title = iw.Title;

                if (iw.OnlineDataAndPicIsLoaded() && iw.Integrity)
                {
                    appDelegate.addPictureSubmenuItemAndRegEventHandlers(
                        title,
                        stateRef,
                        iw.PreviewIsLoaded(),
                        cancelEndBrowsingStateWithDelay,
                        () => appHandler.Wallpaper.SetPreviewWallpaper(iw),
                        () =>
                        {
                            //appHandler.Wallpaper.SetWallpaperAllScreens(iw);
                        },
                        () => setEndBrowsingStateWithDelay(),
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
            Console.WriteLine("Daily check changed to newest: " + newState);
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


        public async void ClickedInMenuManualCheckForNewPic()
        {
            await this.appHandler.checkForNewPics();
        }

        public void HideSubtitle()
        {
            this.appHandler.AppDelegate.HideSubTitle();
        }

        public void OpenAbout()
        {
            this.appHandler.AppDelegate.launchAboutWindow();
        }

        private void renewCancellationSource()
        {
            this.taskCancellationSource = new CancellationTokenSource();
            this.cancellationToken = taskCancellationSource.Token;
        }

        private void OnTimedEventDownloadAnimation(Object stateInfo)
        {
            string iconName = "MainIcon_download_" + flipCounter;
            appDelegate.changeIconTo(iconName, true);
            if (flipCounter == 0) goingdown = false;
            if (flipCounter == 9) goingdown = true;
            flipCounter = goingdown ? flipCounter - 1 : flipCounter + 1;
        }

        private void OnTimedEventSpinnerAnimation(Object stateInfo)
        {
            int iconRotationDeg = 400 - (flipCounter * 7);
            // made wrong calc in photoshop
            if (iconRotationDeg > 204) rotDegOffset = 0;
            if (iconRotationDeg <= 204) rotDegOffset = 2;
            if (iconRotationDeg <= 3) rotDegOffset = -1;
            string iconName = "MainIcon_rot_" + (iconRotationDeg + rotDegOffset);
            Console.WriteLine("icon: " + iconName);
            //Console.WriteLine("iconname: " + iconName);
            appDelegate.changeIconTo(iconName, true);
            if (iconRotationDeg + rotDegOffset == 0) goingdown = true;
            if (iconRotationDeg + rotDegOffset == 400) goingdown = false;
            flipCounter = goingdown ? flipCounter - 1 : flipCounter + 1;
        }

        private async Task setEndBrowsingStateWithDelay()
        {
            await Task.Delay(100, this.cancellationToken);
            appHandler.Wallpaper.ResetWallpaper();
            appHandler.State.UnsetStateBrowsingWallpapers();
        }

        private void cancelEndBrowsingStateWithDelay()
        {
            this.taskCancellationSource.Cancel();
            renewCancellationSource();
        }

        private void removeAllPictureItemsInSubmenu()
        {
            appDelegate.removeAllPictureItemsInSubmenu();
        }




    }
}

