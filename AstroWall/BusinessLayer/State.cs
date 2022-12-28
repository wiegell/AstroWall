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
        internal readonly object _lock_ = new Object();

        // All of below should only be modified if locked
        internal bool isInitializing { get; private set; }
        internal bool isPostProcessing { get; private set; }
        internal bool isIdle { get; private set; }
        internal bool isDownloading { get; private set; }
        internal bool isSettingWallpaper { get; private set; }
        internal bool isBrowsingWallpapers { get; private set; }
        internal bool isUpdating { get; private set; }
        internal bool isChoosingPrefs { get; private set; }
        private int nDownloading;

        // Refs
        ApplicationHandler applicationHandler;

        // Log
        private Action<string> log = Logging.GetLogger("State");

        // currentVersionString is the long tag from git including commit hash  
        public State(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
        }

        public void SetStateInitializing()
        {
            lock (_lock_)
            {
                log("State: Initializing");
                isIdle = false;
                isInitializing = true;
                applicationHandler.MenuHandler.EnableStatusIcon();
                applicationHandler.MenuHandler.DisableAllItems();
                applicationHandler.MenuHandler.SetSubTitleInitialising();
                applicationHandler.MenuHandler.RunSpinnerIconAnimation();
            }
        }
        public void UnsetStateInitializing()
        {
            lock (_lock_)
            {
                log("State unset: Initializing");
                isInitializing = false;
                trySetStateIdle();
            }
        }

        public void SetStateDownloading(string downloadingWhat, bool disableIcon = false)
        {
            lock (_lock_)
            {
                log("State: Downloading");
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
                applicationHandler.MenuHandler.SetSubTitle(downloadingWhat);
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
                    log("State unset: Downloading");
                    isDownloading = false;
                    if (isPostProcessing || isInitializing)
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
                log("State: Choose prefs");
                isDownloading = true;
                isIdle = false;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }
        }
        public void UnsetStateChoosePrefs()
        {
            lock (_lock_)
            {
                log("State unset: ChoosePrefs");
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
                    applicationHandler.MenuHandler.SetSubTitle("Processing picture...");
                    log("State: PostProcessing");
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
                log("State unset: PostProcessing");
                isPostProcessing = false;
                trySetStateIdle();
            }
        }

        public void SetStateBrowsingWallpapers()
        {
            lock (_lock_)
            {
                log("State: browsing wallpapers");
                isBrowsingWallpapers = true;
            }
        }
        public void UnsetStateBrowsingWallpapers()
        {
            lock (_lock_)
            {
                log("State unset: browsing wallpapers");
                isBrowsingWallpapers = false;
                trySetStateIdle();
            }
        }

        public void SetStateUpdating()
        {
            lock (_lock_)
            {
                log("State: updating");
                isUpdating = true;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }
            // No unsetter, will kill app after update
        }

        private void trySetStateIdle()
        {
            lock (_lock_)
            {
                log($"Trying to set state idle: {isBrowsingWallpapers},{isChoosingPrefs},{isDownloading},{isInitializing},{isPostProcessing},{isSettingWallpaper}");
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
                    log("Setting state to idle:");
                    isIdle = true;
                    applicationHandler.MenuHandler.EnableStatusIcon();
                    Task t = applicationHandler.MenuHandler.SetIconToDefault();
                    applicationHandler.MenuHandler.HideSubtitle();
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

