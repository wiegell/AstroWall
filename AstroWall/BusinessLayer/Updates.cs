using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall.BusinessLayer
{
    public class UpdatePromptResponse
    {
        public bool acceptOrSkipUpdate;
        public string skippedVersion;
    }

    public class Updates
    {
        // Refs
        private ApplicationHandler applicationHandler;

        // Misc
        public UpdateLibrary.Release pendingUpdate { private set; get; }
        public Version currentVersion { get; private set; }
        private string pendingUpdatePKGpath;

        // Log
        private Action<string> log = Logging.GetLogger("Updates");
        private Action<string> logError = Logging.GetLogger("Updates", true);

        public Updates(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
            this.currentVersion = VersionFromString(currentVersionString);
            log("currentVersion: " + VersionFromString(currentVersionString));
        }

        private UpdateLibrary.UpdateManifest manifest;

        public void registerWakeHandler()
        {
            log("Registering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.RegisterUpdateWakeHandler(this.wakeHandler);
        }

        public void unregisterWakeHandler()
        {
            log("Unregistering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterUpdateWakeHandler();
        }

        public void wakeHandler(NSNotification not)
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
                    Exception newEx = new Exception("Exception in update check", ex);
                    General.RunOnUIThread(() =>
                    {
                        logError("Exception in update check on thread: " + Thread.CurrentThread.ManagedThreadId);
                        logError("Ex: " + ex.GetType() + ", " + ex.Message);
                        throw ex;
                    });

                    // This will not bubble up
                    throw newEx;
                }
            });
            updateChecking.Wait();
            log("Update checking done");
        }

        public async Task GetManifest()
        {
            string tmpPath;
            tmpPath = await FileHelpers.DownloadUrlToTmpPath("https://wiegell.github.io/AstroWall/assets/manifest.json");
            manifest = FileHelpers.DeSerializeNow<UpdateLibrary.UpdateManifest>(tmpPath);
        }

        public UpdateLibrary.Release checkManifestForNewer(bool includePreReleases = true)
        {
            if (manifest == null)
            {
                throw new Exception("manifest not defined");
            }

            UpdateLibrary.Release[] allReleases = manifest.getAllReleasesDateSorted();

            if (allReleases.Length == 0) return null;
            UpdateLibrary.Release latestRelease = allReleases[allReleases.Length - 1];

            Version latestVersion = VersionFromString(latestRelease.version);

            log("comparing currentversion: " + currentVersion);
            log("to latest version: " + latestVersion);
            if (currentVersion < latestVersion) return latestRelease;
            else return null;
        }

        /// <summary>
        /// Helper that returns version stripped for text. E.g. 1.1.1 from 1.1.1-alpha-ar8os4n
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Version VersionFromString(string str)
        {
            Regex reg = new Regex(@"\d+(?:\.\d+)+");
            if (!reg.IsMatch(str)) throw new ArgumentException("string not correctly formatted");
            MatchCollection matches = reg.Matches(str);
            if (matches.Count > 1) throw new ArgumentException("more than one match");
            return new Version(matches[0].Value);
        }


        public async Task<string> DownloadPendingUpdate()
        {
            log("Starting pending update download: " + pendingUpdate.DirectPKGurl);
            string pathToPkg = await FileHelpers.DownloadUrlToTmpPath(pendingUpdate.DirectPKGurl);
            log("Downloaded new pkg to path: " + pathToPkg);
            return pathToPkg;
        }

        public async Task<Boolean> GetUpdateManifestAndCheckIfUpdatePending()
        {
            log("Fetching manifest");

            await GetManifest();
            log("Update manifest downloaded and parsed");
            pendingUpdate = checkManifestForNewer();
            bool hasPendingUpdate = pendingUpdate == null ? false : true;
            log("Has pending update: " + hasPendingUpdate);
            return hasPendingUpdate;
        }

        public async Task CheckForUpdates(bool manualCheck = false, bool runAtOnce = false)
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
                        // TODO update UI
                        ApplicationLayer.Updates.Instance.AlertNoUpdates("ISOFFLINE");
                    return;
                }
                else if (ex.GetType() == typeof(System.Net.WebException))
                {
                    log("could not get manifest: " + ex.Message);
                    if (manualCheck)
                        // TODO update UI
                        ApplicationLayer.Updates.Instance.AlertNoUpdates("NETWORKERROR");
                    return;
                }
                else throw ex;
            }
            if (hasPendingUpdate)
            {
                log($"Has pending update: {pendingUpdate.version}");

                if (applicationHandler.Prefs.AutoInstallUpdates)
                {
                    await downloadAndUpdate(runAtOnce);
                }
                else
                {
                    applicationHandler.AppDelegate.launchUpdatePrompt(pendingUpdate, updatePromptCallBack);
                }
            }
            else
            {
                log("No pending updates, is up to date");
                if (manualCheck)
                    ApplicationLayer.Updates.Instance.AlertNoUpdates(currentVersion.ToString());
            }
        }

        public async Task ConsiderCheckingForUpdates(bool runAtOnce, bool overridePrefs = false)
        {
            if (overridePrefs || applicationHandler.Prefs.CheckUpdatesOnStartup)
            {
                log("User has activated updates on startup, running updater");

                await CheckForUpdates(manualCheck: false, runAtOnce);


            }
        }

        // TODO async void remove
        private async void updatePromptCallBack(UpdatePromptResponse resp)
        {
            if (resp.acceptOrSkipUpdate)
            {
                // Update accepted
                await downloadAndUpdate(true);
            }
            else
            {
                applicationHandler.Prefs.UserChosenToSkipUpdatesBeforeVersion = (resp.skippedVersion);
            }
        }

        private async Task downloadAndUpdate(bool runAtOnce = false)
        {
            applicationHandler.State.SetStateDownloading("Checking for updates", runAtOnce);
            pendingUpdatePKGpath = await DownloadPendingUpdate();
            applicationHandler.State.UnsetStateDownloading();
            if (runAtOnce)
            {
                log("Running pending update");
                runPendingUpdate();
            }
            else
            {
                log("Queing pending update");
                await queueUpdateInstall();
            }
        }

        private async Task queueUpdateInstall()
        {
            // Should be idle for 10 seconds before update is installed
            if (applicationHandler.State.isIdle)
            {
                log("Is idle, rechecking if still idle in 10 sec");
                await Task.Delay(10000);
                if (applicationHandler.State.isIdle)
                {
                    log("Still idle, running installer");
                    runPendingUpdate();
                }
            }
            else
            {
                log("Not idle, rechecking in 10 seconds");
                await Task.Delay(10000);
                await downloadAndUpdate();
            }
        }

        private void runPendingUpdate()
        {
            lock (applicationHandler.State._lock_)
            {
                // Double check idle
                if (applicationHandler.State.isIdle)
                {
                    applicationHandler.State.SetStateUpdating();

                    log("Prepping for update, saving prefs and DB");
                    applicationHandler.TerminationPreparations();

                    log("Running PKG update");
                    ApplicationLayer.Updates.Instance.RunPKGUpdate(pendingUpdatePKGpath);
                    General.Relaunch();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();

                    // Stay in the lock until terminated
                    Thread.Sleep(10000);
                }
                else
                {
                    log("Not idle for update, state must have been altered by other thread, resetting timer");
                    queueUpdateInstall();
                }
            }
        }
    }
}

