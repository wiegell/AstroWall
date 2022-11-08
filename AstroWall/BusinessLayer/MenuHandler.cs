﻿using System;
using AppKit;
using System.Collections.Generic;
using AstroWall.ApplicationLayer;
using System.Timers;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

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
        private static System.Timers.Timer iconUpdateTimer;
        private int flipCounter = 0;

        // Browsing state
        private Task restoreToIdleWithDelayTask;
        private CancellationTokenSource taskCancellationSource;
        private CancellationToken cancellationToken;

        public MenuHandler(AppDelegate del, ApplicationHandler app)
        {
            appDelegate = del;
            appHandler = app;
            noAutoEnableMenuItems();
            renewCancellationSource();
        }

        public void createStatusBar(string title)
        {
            appDelegate.createStatusBar(title);
        }

        public void updateMenuCheckMarksToReflectState(State state)
        {
            appDelegate.updateMenuCheckMarks(state.prefs);
        }

        public void DisableAllItems()
        {
            appDelegate.disableAllItemsExceptQuit();
        }

        public void noAutoEnableMenuItems()
        {
            appDelegate.noAutoEnableMenuItems();
        }

        public void enableStatusIcon()
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

        public void RunDownloadIconAnimation()
        {
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Timers.Timer(500);
            // Hook up the Elapsed event for the timer. 
            iconUpdateTimer.Elapsed += OnTimedEvent;
            iconUpdateTimer.AutoReset = true;
            iconUpdateTimer.Enabled = true;
        }

        public void SetIconToDefault()
        {
            iconUpdateTimer.Stop();
            Task.Run(() =>
            {
                Task.Delay(500);
                appDelegate.changeIconTo("staat");
            });
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

                if (iw.OnlineDataExceptPicIsLoaded() && iw.integrity)
                {
                    appDelegate.addPictureSubmenuItemAndRegEventHandlers(
                        title,
                        stateRef,
                        iw.PreviewIsLoaded(),
                        cancelEndBrowsingStateWithDelay,
                        appHandler.Wallpaper.SetPreviewWallpaper(iw),
                        appHandler.Wallpaper.SetWallpaperAllScreens(iw),
                        setEndBrowsingStateWithDelay,



                        )
                }
            }
        }

        private void renewCancellationSource()
        {
            this.taskCancellationSource = new CancellationTokenSource();
            this.cancellationToken = taskCancellationSource.Token;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            int flipCounter1based = flipCounter + 1;
            string iconName = "download" + flipCounter1based;
            ChangeIconToDL(flipCounter1based);
            flipCounter = (flipCounter + 1) % 3;
        }

        private void ChangeIconToDL(int counter)
        {
            appDelegate.changeIconTo("download" + counter);
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

