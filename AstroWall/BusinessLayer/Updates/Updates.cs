using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Takes care of software updates.
    /// TODO this class is not yet OS-non-specific.
    /// </summary>
    internal class Updates
    {
        // Refs
        private readonly ApplicationHandler applicationHandler;

        // Log
        private readonly Action<string> log = Logging.GetLogger("Updates");
        private readonly Action<string> logError = Logging.GetLogger("Updates", true);

        // Manifest
        private UpdateLibrary.UpdateManifest manifest;

        // Path to pending pkg if downloaded
        private string pendingUpdatePKGpath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Updates"/> class.
        /// </summary>
        /// <param name="applicationHandlerArg"></param>
        /// <param name="currentVersionString">Takes long version string including commit. See VersionFromString().</param>
        internal Updates(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
            this.CurrentVersion = VersionFromString(currentVersionString);
            this.log("currentVersion: " + VersionFromString(currentVersionString));
        }

        /// <summary>
        /// Gets pending update, if loaded from gh.
        /// </summary>
        internal UpdateLibrary.Release PendingUpdate { get; private set; }

        /// <summary>
        /// Gets current version as instance.
        /// </summary>
        internal Version CurrentVersion { get; private set; }

        /// <summary>
        /// Helper that returns version stripped for text. E.g. 1.1.1 from 1.1.1-alpha-ar8os4n.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static Version VersionFromString(string str)
        {
            Regex reg = new Regex(@"\d+(?:\.\d+)+");
            if (!reg.IsMatch(str))
            {
                throw new ArgumentException("string not correctly formatted");
            }

            MatchCollection matches = reg.Matches(str);
            if (matches.Count > 1)
            {
                throw new ArgumentException("more than one match");
            }

            return new Version(matches[0].Value);
        }

        /// <summary>
        /// Registers wake handler.
        /// </summary>
        internal void RegisterWakeHandler()
        {
            log("Registering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.RegisterUpdateWakeHandler(this.WakeHandler);
        }

        /// <summary>
        /// Unregisters wake handler.
        /// </summary>
        internal void UnregisterWakeHandler()
        {
            log("Unregistering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterUpdateWakeHandler();
        }

        /// <summary>
        /// The actual wake handler callback action.
        /// </summary>
        internal void WakeHandler(NSNotification not)
        {
            log("Wake, checking for software updates");
            Task updateChecking = Task.Run(async () =>
            {
                try
                {
                    await ConsiderCheckingForUpdates(runAtOnce: true);
                }
                catch (Exception ex)
                {
                    // Rethrow to UI thread for debugging
                    General.RunOnUIThread(() =>
                    {
                        logError("Exception in update check on thread: " + Environment.CurrentManagedThreadId);
                        logError("Ex: " + ex.GetType() + ", " + ex.Message);
                        throw ex;
                    });

                    // This will not bubble up
                    throw;
                }
            });
            updateChecking.Wait();
            log("Update checking done");
        }

        /// <summary>
        /// Downloads manifest to local tmp storage from gh.
        /// </summary>
        /// <returns></returns>
        internal async Task GetManifest()
        {
            string tmpPath;
            tmpPath = await FileHelpers.DownloadUrlToTmpPath("https://wiegell.github.io/AstroWall/assets/manifest.json");
            manifest = FileHelpers.DeSerializeNow<UpdateLibrary.UpdateManifest>(tmpPath);
        }

        /// <summary>
        /// Checks manifest for newer versions.
        /// </summary>
        /// <param name="includePreReleases"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throws if manifest not downloaded.</exception>
        internal UpdateLibrary.Release CheckManifestForNewer(bool includePreReleases = true)
        {
            if (manifest == null)
            {
                throw new InvalidOperationException("manifest not defined");
            }

            UpdateLibrary.Release[] allReleases = manifest.getAllReleasesDateSorted();

            if (allReleases.Length == 0)
            {
                return null;
            }

            UpdateLibrary.Release latestRelease = allReleases[allReleases.Length - 1];

            Version latestVersion = VersionFromString(latestRelease.version);

            log("comparing currentversion: " + CurrentVersion);
            log("to latest version: " + latestVersion);
            if (CurrentVersion < latestVersion)
            {
                return latestRelease;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads the pkg of pending update.
        /// </summary>
        /// <returns></returns>
        internal async Task<string> DownloadPendingUpdate()
        {
            log("Starting pending update download: " + PendingUpdate.DirectPKGurl);
            string pathToPkg = await FileHelpers.DownloadUrlToTmpPath(PendingUpdate.DirectPKGurl);
            log("Downloaded new pkg to path: " + pathToPkg);
            return pathToPkg;
        }

        /// <summary>
        /// Checks if there is an update avail. and downloads it.
        /// If auto install is activated the updater pkg is run.
        /// </summary>
        /// <param name="manualCheck">If manual check a prompt is launched if up to date. Another prompt if an update is found.</param>
        /// <param name="runAtOnce">Bypass idle check if called on app launch.</param>
        /// <returns></returns>
        internal async Task CheckForUpdates(bool manualCheck = false, bool runAtOnce = false)
        {
            bool hasPendingUpdate;
            try
            {
                hasPendingUpdate = await GetUpdateManifestAndCheckIfUpdatePending();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.Net.WebException) && ex.Message == "Error: NameResolutionFailure")
                {
                    log("Is offline");
                    if (manualCheck)
                    {
                        // TODO update UI
                        ApplicationLayer.Updates.AlertNoUpdates("ISOFFLINE");
                    }

                    return;
                }
                else if (ex.GetType() == typeof(System.Net.WebException))
                {
                    log("could not get manifest: " + ex.Message);
                    if (manualCheck)
                    {
                        // TODO update UI
                        ApplicationLayer.Updates.AlertNoUpdates("NETWORKERROR");
                    }

                    return;
                }
                else
                {
                    throw;
                }
            }

            if (hasPendingUpdate)
            {
                log($"Has pending update: {PendingUpdate.version}");

                if (applicationHandler.Prefs.AutoInstallUpdates)
                {
                    await DownloadAndUpdate(runAtOnce);
                }
                else
                {
                    applicationHandler.AppDelegate.LaunchUpdatePrompt(PendingUpdate, UpdatePromptCallBack);
                }
            }
            else
            {
                log("No pending updates, is up to date");
                if (manualCheck)
                {
                    ApplicationLayer.Updates.AlertNoUpdates(CurrentVersion.ToString());
                }
            }
        }

        /// <summary>
        /// Runs updater if the user has activated auto update.
        /// </summary>
        /// <param name="runAtOnce">Does not wait for idle.</param>
        /// <param name="overridePrefs"></param>
        /// <returns></returns>
        internal async Task ConsiderCheckingForUpdates(bool runAtOnce, bool overridePrefs = false)
        {
            if (overridePrefs || applicationHandler.Prefs.CheckUpdatesOnStartup)
            {
                log("User has activated updates on startup, running updater");

                await CheckForUpdates(manualCheck: false, runAtOnce);
            }
        }

        // TODO async void remove
        private async void UpdatePromptCallBack(UpdatePromptResponse resp)
        {
            if (resp.AcceptOrSkipUpdate)
            {
                // Update accepted
                await DownloadAndUpdate(true);
            }
            else
            {
                applicationHandler.Prefs.UserChosenToSkipUpdatesBeforeVersion = resp.SkippedVersion;
            }
        }

        /// <summary>
        /// Downloads update and queues installer (to wait for idle state).
        /// </summary>
        /// <param name="runAtOnce">Bypass idle wait, e.g. on launch.</param>
        /// <returns></returns>
        private async Task DownloadAndUpdate(bool runAtOnce = false)
        {
            applicationHandler.State.SetStateDownloading("Checking for updates", runAtOnce);
            pendingUpdatePKGpath = await DownloadPendingUpdate();
            applicationHandler.State.UnsetStateDownloading();
            if (runAtOnce)
            {
                log("Running pending update");
                RunPendingUpdate();
            }
            else
            {
                log("Queing pending update");
                await QueueUpdateInstall();
            }
        }

        /// <summary>
        /// Queues updater to run if the state has been idle for 10s.
        /// </summary>
        /// <returns></returns>
        private async Task QueueUpdateInstall()
        {
            // Should be idle for 10 seconds before update is installed
            if (applicationHandler.State.IsIdle)
            {
                log("Is idle, rechecking if still idle in 10 sec");
                await Task.Delay(10000);
                if (applicationHandler.State.IsIdle)
                {
                    log("Still idle, running installer");
                    RunPendingUpdate();
                }
            }
            else
            {
                log("Not idle, rechecking in 10 seconds");
                await Task.Delay(10000);
                await DownloadAndUpdate();
            }
        }

        /// <summary>
        /// Runs pending update.
        /// </summary>
        private void RunPendingUpdate()
        {
            lock (applicationHandler.State.Lock)
            {
                // Double check idle
                if (applicationHandler.State.IsIdle)
                {
                    applicationHandler.State.SetStateUpdating();

                    log("Prepping for update, saving prefs and DB");
                    applicationHandler.TerminationPreparations();

                    log("Running PKG update");
                    ApplicationLayer.Updates.RunPKGUpdate(pendingUpdatePKGpath);
                    General.Relaunch();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();

                    // Stay in the lock until terminated
                    Thread.Sleep(10000);
                }
                else
                {
                    log("Not idle for update, state must have been altered by other thread, resetting timer");
                    QueueUpdateInstall();
                }
            }
        }

        /// <summary>
        /// Gets update manifest and checks if there is a pending update.
        /// </summary>
        /// <returns>True if there is a pending update.</returns>
        private async Task<bool> GetUpdateManifestAndCheckIfUpdatePending()
        {
            log("Fetching manifest");

            await GetManifest();
            log("Update manifest downloaded and parsed");
            PendingUpdate = CheckManifestForNewer();
            bool hasPendingUpdate = PendingUpdate == null ? false : true;
            log("Has pending update: " + hasPendingUpdate);
            return hasPendingUpdate;
        }
    }
}