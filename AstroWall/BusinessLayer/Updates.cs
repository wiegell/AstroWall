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
        private Version currentVersion;
        private string pendingUpdatePKGpath;

        public Updates(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
            this.currentVersion = VersionFromString(currentVersionString);
            Console.WriteLine("currentVersion: " + VersionFromString(currentVersionString));
        }

        private UpdateLibrary.UpdateManifest manifest;

        public void registerWakeHandler()
        {
            Console.WriteLine("Registering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.RegisterUpdateWakeHandler(this.wakeHandler);
        }

        public void unregisterWakeHandler()
        {
            Console.WriteLine("Unregistering update wakehandler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterUpdateWakeHandler();
        }

        public void wakeHandler(NSNotification not)
        {
            Console.WriteLine("Wake, checking for software updates");
            GetUpdateManifestAndCheckIfUpdatePending();
        }

        public async Task GetManifest()
        {
            string tmpPath = await FileHelpers.DownloadUrlToTmpPath("https://wiegell.github.io/AstroWall/assets/manifest.json");
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

            Console.WriteLine("comparing currentversion: " + currentVersion);
            Console.WriteLine("to latest version: " + latestVersion);
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
            string pathToPkg = await FileHelpers.DownloadUrlToTmpPath(pendingUpdate.DirectPKGurl);
            Console.Write("Downloaded new pkg to path: " + pathToPkg);
            return pathToPkg;
        }

        public async Task<Boolean> GetUpdateManifestAndCheckIfUpdatePending()
        {
            await GetManifest();
            Console.WriteLine("Update manifest downloaded and parsed");
            pendingUpdate = checkManifestForNewer();
            bool hasPendingUpdate = pendingUpdate == null ? false : true;
            Console.WriteLine("Has pending update: " + hasPendingUpdate);
            return hasPendingUpdate;
        }

        public async Task CheckForUpdates(bool manualCheck = false)
        {
            bool hasPendingUpdate = await GetUpdateManifestAndCheckIfUpdatePending();
            if (hasPendingUpdate)
            {
                Console.WriteLine("Has pending update: {0}", pendingUpdate.version);

                if (applicationHandler.Prefs.AutoInstallUpdates)
                {
                    await downloadAndUpdate();
                }
                else
                {
                    applicationHandler.AppDelegate.launchUpdatePrompt(pendingUpdate, updatePromptCallBack);
                }
            }
            else
            {
                Console.WriteLine("No pending updates, is up to date");
                if (manualCheck)
                    ApplicationLayer.Updates.Instance.AlertNoUpdates(currentVersion.ToString());
            }
        }

        public async Task ConsiderCheckingForUpdates()
        {
            if (applicationHandler.Prefs.CheckUpdatesOnStartup)
            {
                await CheckForUpdates();
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
                runPendingUpdate();
            }
            else
            {
                await queueUpdateInstall();
            }
        }

        private async Task queueUpdateInstall()
        {
            // Should be idle for 10 seconds before update is installed
            if (applicationHandler.State.isIdle)
            {
                Console.WriteLine("Is idle, rechecking if still idle in 10 sec");
                await Task.Delay(10000);
                if (applicationHandler.State.isIdle)
                {
                    Console.WriteLine("Still idle, running installer");
                    runPendingUpdate();
                }
            }
            else
            {
                Console.WriteLine("Not idle, rechecking in 10 seconds");
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

                    Console.WriteLine("Prepping for update, saving prefs and DB");
                    applicationHandler.TerminationPreparations();

                    Console.WriteLine("Running PKG update");
                    ApplicationLayer.Updates.Instance.RunPKGUpdate(pendingUpdatePKGpath);
                    General.Relaunch();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();

                    // Stay in the lock until terminated
                    Thread.Sleep(10000);
                }
                else
                {
                    Console.WriteLine("Not idle for update, state must have been altered by other thread, resetting timer");
                    queueUpdateInstall();
                }
            }
        }
    }
}

