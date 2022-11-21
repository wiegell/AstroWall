using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;

namespace AstroWall.BusinessLayer
{

    public class State
    {
        // Application state
        public readonly object _lock_ = new Object();
        // All of below should only be accessed if locked
        public bool isInitializing;
        public bool isPostProcessing;
        public bool isIdle;
        public bool isDownloading;
        private int nDownloading = 0;
        public bool isSettingWallpaper;
        public bool isBrowsingWallpapers;
        public bool isUpdating;
        public bool isChoosingPrefs;

        // Refs
        ApplicationHandler applicationHandler;

        // currentVersionString is the long tag from git including commit hash  
        public State(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
        }

        public void SetStateInitializing()
        {
            lock (_lock_)
            {
                Console.WriteLine("State: Initializing");
                isIdle = false;
                isInitializing = true;
                applicationHandler.MenuHandler.EnableStatusIcon();
                applicationHandler.MenuHandler.DisableAllItems();
                applicationHandler.MenuHandler.SetTitleInitialising();
                applicationHandler.MenuHandler.RunSpinnerIconAnimation();
            }
        }
        public void UnsetStateInitializing()
        {
            lock (_lock_)
            {
                Console.WriteLine("State unset: Initializing");
                isInitializing = false;
                trySetStateIdle();
            }
        }

        public void SetStateDownloading(string downloadingWhat, bool disableIcon = false)
        {
            lock (_lock_)
            {
                Console.WriteLine("State: Downloading");
                isDownloading = true;
                nDownloading++;
                isIdle = false;
                if (!disableIcon)
                {
                    applicationHandler.MenuHandler.EnableStatusIcon();
                }
                else
                {
                    applicationHandler.MenuHandler.DisableStatusIcon();
                }
                applicationHandler.MenuHandler.DisableAllItems();
                applicationHandler.MenuHandler.SetTitleDownloading(downloadingWhat);
                applicationHandler.MenuHandler.RunDownloadIconAnimation();
            }
        }
        public void UnsetStateDownloading()
        {
            lock (_lock_)
            {
                nDownloading--;
                if (nDownloading == 0)
                {

                    Console.WriteLine("State unset: Downloading");
                    isDownloading = false;
                    if (isPostProcessing)
                    {
                        applicationHandler.MenuHandler.RunSpinnerIconAnimation();
                    }
                    trySetStateIdle();
                }
            }
        }

        public void SetStateChoosePrefs()
        {
            lock (_lock_)
            {
                Console.WriteLine("State: Choose prefs");
                isDownloading = true;
                isIdle = false;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }
        }
        public void UnsetStateChoosePrefs()
        {
            lock (_lock_)
            {
                Console.WriteLine("State unset: ChoosePrefs");
                isChoosingPrefs = false;
                trySetStateIdle();
            }
        }

        public void SetStatePostProcessing()
        {
            lock (_lock_)
            {
                // No reason for multiple sets
                if (!isPostProcessing)
                {

                Console.WriteLine("State: PostProcessing");
                isPostProcessing = true;
                isIdle = false;
                // Check to see if animation already running
                if (!isInitializing && !isDownloading) applicationHandler.MenuHandler.RunSpinnerIconAnimation();
                }
            }
        }
        public void UnsetStatePostProcessing()
        {
            lock (_lock_)
            {
                Console.WriteLine("State unset: PostProcessing");
                isPostProcessing = false;
                trySetStateIdle();
            }
        }

        public void SetStateBrowsingWallpapers()
        {
            lock (_lock_)
            {
                Console.WriteLine("State: browsing wallpapers");
                isBrowsingWallpapers = true;
            }
        }
        public void UnsetStateBrowsingWallpapers()
        {
            lock (_lock_)
            {
                Console.WriteLine("State unset: browsing wallpapers");
                isBrowsingWallpapers = false;
                trySetStateIdle();
            }
        }

        public void SetStateUpdating()
        {
            lock (_lock_)
            {
                Console.WriteLine("State: updating");
                isUpdating = true;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }
            // No unsetter, will kill app after update
        }

        private void trySetStateIdle()
        {
            lock (_lock_)
            {
                Console.WriteLine("Trying to set state idle: {0},{1},{2},{3},{4},{5}", isBrowsingWallpapers, isChoosingPrefs, isDownloading, isInitializing, isPostProcessing, isSettingWallpaper);
                if (
                    !(
                    isBrowsingWallpapers ||
                    isChoosingPrefs ||
                    isDownloading ||
                    isInitializing ||
                    isPostProcessing ||
                    isUpdating ||
                    isSettingWallpaper
                    ))
                {
                    Console.WriteLine("Setting state to idle:");
                    isIdle = true;
                    applicationHandler.MenuHandler.EnableStatusIcon();
                    Task t =applicationHandler.MenuHandler.SetIconToDefault();
                    applicationHandler.MenuHandler.HideState();
                }
            }
        }

        public void SetLaunchAgentToReflectPrefs()
        {
            if (applicationHandler
                .Prefs.RunAtStartup) ApplicationLayer.SystemEvents.SetAsLaunchAgent();
            else ApplicationLayer.SystemEvents.RemoveLaunchAgent();
        }

    }
}

