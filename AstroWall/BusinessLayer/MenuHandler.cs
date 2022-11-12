using System;
using AppKit;
using System.Collections.Generic;
using AstroWall.ApplicationLayer;
using System.Timers;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Reflection;

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
            appDelegate.updateMenuCheckMarks(appHandler.Prefs);
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

        public void disableStatusIcon()
        {
            appDelegate.disableStatusIcon();
        }

        public void SetTitleInitialising()
        {
            appDelegate.setSubTitle("Initializing...");
        }
        public void SetTitleDownloading(string msg)
        {
            appDelegate.setSubTitle(msg);
        }

        public void RunDownloadIconAnimation()
        {
            if (iconUpdateTimer != null) iconUpdateTimer.Dispose();
            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
      OnTimedEventDownloadAnimation,
      null,
      0,
      500);
        }

        public void RunSpinnerIconAnimation()
        {
            if (iconUpdateTimer != null) iconUpdateTimer.Dispose();
            autoEvent = new AutoResetEvent(false);
            flipCounter = 0;
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Threading.Timer(
                OnTimedEventSpinnerAnimation,
                null,
                0,
                250);

        }

        public void SetIconToDefault()
        {
            iconUpdateTimer.Dispose();
            //Task.Run(() =>
            //{
            //    Task.Delay(500);
            appDelegate.changeIconTo("MainIcon_rot_0");
            flipCounter = 0;
            //});
        }

        public void HideState()
        {

        }

        public void PopulateSubmenuLatestPictures(List<ImgWrap> imgWrapList, State stateRef)
        {

            // Clear existing items of menu
            removeAllPictureItemsInSubmenu();

            foreach (ImgWrap iw in imgWrapList)
            {
                string title = iw.Title;

                if (iw.OnlineDataAndPicIsLoaded() && iw.integrity)
                {
                    appDelegate.addPictureSubmenuItemAndRegEventHandlers(
                        title,
                        stateRef,
                        iw.PreviewIsLoaded(),
                        cancelEndBrowsingStateWithDelay,
                        () => appHandler.Wallpaper.SetPreviewWallpaper(iw),
                        () => appHandler.Wallpaper.SetWallpaperAllScreens(iw),
                        () => setEndBrowsingStateWithDelay(),
                        () =>
                        {
                            appHandler.Prefs.CurrentAstroWallpaper = iw;
                            appHandler.Wallpaper.SetWallpaperAllScreens(iw);
                        }


                        );
                }
            }
        }

        public void changedInMenuRunAtLogin(bool newState)
        {
            appHandler.Prefs.RunAtStartup = newState;
            appHandler.State.SetLaunchAgentToReflectPrefs();
            appDelegate.updateMenuCheckMarks(appHandler.Prefs);
        }

        public void changedInMenuAutoInstallUpdates(bool newState)
        {
            appHandler.Prefs.AutoInstallUpdates = newState;
            appDelegate.updateMenuCheckMarks(appHandler.Prefs);
        }

        public void changedInMenuCheckUpdatesAtStartup(bool newState)
        {
            appHandler.Prefs.CheckUpdatesOnStartup = newState;
            appHandler.Updates.unregisterWakeHandler();
            appDelegate.updateMenuCheckMarks(appHandler.Prefs);
        }

        public async void clickedInMenuManualCheckUpdates()
        {
            await appHandler.Updates.CheckForUpdates(true);
        }

        private void renewCancellationSource()
        {
            this.taskCancellationSource = new CancellationTokenSource();
            this.cancellationToken = taskCancellationSource.Token;
        }

        private void OnTimedEventDownloadAnimation(Object stateInfo)
        {

            int flipCounter1based = flipCounter + 1;
            string iconName = "download" + flipCounter1based;
            appDelegate.changeIconTo("download" + flipCounter1based, true, stateEnum.Downloading);
            flipCounter = (flipCounter + 1) % 3;

        }

        private void OnTimedEventSpinnerAnimation(Object stateInfo)
        {
            int iconRotationDeg = (flipCounter * 15);
            string iconName = "MainIcon_rot_" + iconRotationDeg;
            Console.WriteLine("iconname: " + iconName);
            appDelegate.changeIconTo(iconName, true, BusinessLayer.stateEnum.Initializing);
            flipCounter = (flipCounter + 1) % 6;
        }

        private async Task setEndBrowsingStateWithDelay()
        {
            await Task.Delay(100, this.cancellationToken);
            appHandler.Wallpaper.ResetWallpaper();
            appHandler.State.SetStateIdle();
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

