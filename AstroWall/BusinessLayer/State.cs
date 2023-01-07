using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Manages state of application. The application can have multiple concurrent states
    /// (e.g. PostProcessing and BrowsingWallPapers), but some states e.g. idle needs to be
    /// the only active state. Only set state after Lock is performed (via setState methods).
    /// </summary>
    internal class State
    {
        // Refs
        private readonly ApplicationHandler applicationHandler;

        // Log
        private readonly Action<string> log = Logging.GetLogger("State");

        /// <summary>
        /// How many downloads are running? Needed to keep track of
        /// when its possible to go back to idle state.
        /// </summary>
        private int nDownloading;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="currentVersionString">currentVersionString is the long tag from git including commit hash.</param>
        internal State(ApplicationHandler applicationHandler, string currentVersionString)
        {
            this.applicationHandler = applicationHandler;
        }

        /// <summary>
        /// Gets a value indicating whether state is initializing.
        /// </summary>
        internal bool IsInitializing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is post processing.
        /// </summary>
        internal bool IsPostProcessing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is idle.
        /// </summary>
        internal bool IsIdle { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is idle.
        /// </summary>
        internal bool IsDownloading { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is setting wallpaper.
        /// TODO still used?.
        /// </summary>
        internal bool IsSettingWallpaper { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is browsing wallpapers.
        /// </summary>
        internal bool IsBrowsingWallpapers { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is updating the software.
        /// </summary>
        internal bool IsUpdating { get; private set; }

        /// <summary>
        /// Gets a value indicating whether state is choosing prefs (on fresh install).
        /// </summary>
        internal bool IsChoosingPrefs { get; private set; }

        /// <summary>
        /// Gets thread sync lock to prevent state changes from other threads.
        /// </summary>
        internal object Lock { get; private set; } = new object();

        /// <summary>
        /// Sets state to initializing.
        /// </summary>
        internal void SetStateInitializing()
        {
            lock (Lock)
            {
                log("State: Initializing");
                IsIdle = false;
                IsInitializing = true;
                applicationHandler.MenuHandler.EnableStatusIcon();
                applicationHandler.MenuHandler.DisableAllItems();
                applicationHandler.MenuHandler.SetSubTitleInitialising();
                applicationHandler.MenuHandler.RunSpinnerIconAnimation();
            }
        }

        /// <summary>
        /// Unsets state to be initializing.
        /// </summary>
        internal void UnsetStateInitializing()
        {
            lock (Lock)
            {
                log("State unset: Initializing");
                IsInitializing = false;
                TrySetStateIdle();
            }
        }

        /// <summary>
        /// Sets state to downloadingg.
        /// </summary>
        /// <param name="downloadingWhat">What is downloading?</param>
        /// <param name="disableIcon">Whether status icon should be disabled.</param>
        internal void SetStateDownloading(string downloadingWhat, bool disableIcon = false)
        {
            lock (Lock)
            {
                log("State: Downloading");
                IsDownloading = true;
                nDownloading++;
                IsIdle = false;
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

        /// <summary>
        /// Unsets state from downloading. At least if it has been called
        /// the same amount of times as SetStateDownloading().
        /// </summary>
        internal void UnsetStateDownloading()
        {
            lock (Lock)
            {
                nDownloading--;
                if (nDownloading == 0)
                {
                    log("State unset: Downloading");
                    IsDownloading = false;
                    if (IsPostProcessing || IsInitializing)
                    {
                        applicationHandler.MenuHandler.RunSpinnerIconAnimation();
                    }

                    TrySetStateIdle();
                }
            }

            // TODO make the postprocess animmation take over.
        }

        /// <summary>
        /// Sets state to choose prefs (on fresh install).
        /// </summary>
        internal void SetStateChoosePrefs()
        {
            lock (Lock)
            {
                log("State: Choose prefs");
                IsDownloading = true;
                IsIdle = false;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }
        }

        /// <summary>
        /// Unsets choose prefs state.
        /// </summary>
        internal void UnsetStateChoosePrefs()
        {
            lock (Lock)
            {
                log("State unset: ChoosePrefs");
                IsChoosingPrefs = false;
                TrySetStateIdle();
            }
        }

        /// <summary>
        /// Sets state to post processing.
        /// </summary>
        internal void SetStatePostProcessing()
        {
            lock (Lock)
            {
                // No reason for multiple sets
                if (!IsPostProcessing)
                {
                    applicationHandler.MenuHandler.SetSubTitle("Processing picture...");
                    log("State: PostProcessing");
                    IsPostProcessing = true;
                    IsIdle = false;

                    // Check to see if animation already running
                    if (!IsInitializing && !IsDownloading)
                    {
                        applicationHandler.MenuHandler.RunSpinnerIconAnimation();
                    }
                }
            }
        }

        /// <summary>
        /// Unsets state from post processing.
        /// </summary>
        internal void UnsetStatePostProcessing()
        {
            lock (Lock)
            {
                log("State unset: PostProcessing");
                IsPostProcessing = false;
                TrySetStateIdle();
            }
        }

        /// <summary>
        /// Sets state to browsing wallpapers.
        /// </summary>
        internal void SetStateBrowsingWallpapers()
        {
            lock (Lock)
            {
                log("State: browsing wallpapers");
                IsBrowsingWallpapers = true;
            }
        }

        /// <summary>
        /// Unsets state to browsing wallpapers.
        /// </summary>
        internal void UnsetStateBrowsingWallpapers()
        {
            lock (Lock)
            {
                log("State unset: browsing wallpapers");
                IsBrowsingWallpapers = false;
                TrySetStateIdle();
            }
        }

        /// <summary>
        /// Sets state to updating.
        /// </summary>
        internal void SetStateUpdating()
        {
            lock (Lock)
            {
                log("State: updating");
                IsUpdating = true;
                applicationHandler.MenuHandler.DisableStatusIcon();
            }

            // No unsetter, will kill app after update
        }

        /// <summary>
        /// Sets launch agent to reflect preferences.
        /// </summary>
        internal void SetLaunchAgentToReflectPrefs()
        {
            if (applicationHandler
                .Prefs.RunAtStartup)
            {
                ApplicationLayer.SystemEvents.SetLaunchAgent();
            }
            else
            {
                ApplicationLayer.SystemEvents.RemoveLaunchAgent();
            }
        }

        /// <summary>
        /// Thies to set state to idle, if no other jobs are running.
        /// </summary>
        private void TrySetStateIdle()
        {
            lock (Lock)
            {
                log($"Trying to set state idle: {IsBrowsingWallpapers},{IsChoosingPrefs},{IsDownloading},{IsInitializing},{IsPostProcessing},{IsSettingWallpaper}");
                if (
                    !(
                    IsBrowsingWallpapers ||
                    IsChoosingPrefs ||
                    IsDownloading ||
                    IsInitializing ||
                    IsPostProcessing ||
                    IsUpdating ||
                    IsSettingWallpaper))
                {
                    log("Setting state to idle:");
                    IsIdle = true;
                    applicationHandler.MenuHandler.EnableStatusIcon();
                    Task t = applicationHandler.MenuHandler.SetIconToDefault();
                    applicationHandler.MenuHandler.HideSubtitle();
                }
            }
        }
    }
}