using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall.BusinessLayer
{
    public class Updates
    {
        // Refs
        private ApplicationHandler applicationHandler;

        // Platform specific helpers
        private ApplicationLayer.UpdateHelpers updateHelpers = new ApplicationLayer.UpdateHelpers();

        // Misc
        public UpdateLibrary.Release pendingUpdate { private set; get; }
        private Version currentVersion;
        private string pendingUpdatePKGpath;

        public Updates(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
            this.currentVersion = VersionFromString(currentVersionString);
            Console.WriteLine("currentVersion: " + VersionFromString(currentVersionString));
            registerWakeHandler();
        }

        private UpdateLibrary.UpdateManifest manifest;

        public void registerWakeHandler()
        {
            updateHelpers.RegisterWakeHandler(this.wakeHandler);
        }

        public void unregisterWakeHandler()
        {
            updateHelpers.UnRegisterWakeHandler();
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

        public void RunPKGUpdate()
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "installer -pkg "+pendingUpdatePKGpath+" -target CurrentUserHomeDirectory"
            };
            //+" "
            nstask.Launch();
            nstask.WaitUntilExit();
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

                if (applicationHandler.Prefs.autoInstallUpdates &&
                    applicationHandler.Prefs.autoInstallSilent)
                {
                    pendingUpdatePKGpath = await DownloadPendingUpdate();
                    Console.WriteLine("Running PKG update");
                    RunPKGUpdate();
                }
                else
                {
                    // TODO
                    // Make interactive updater
                }
            }
            else
            {
                Console.WriteLine("No pending updates, is up to date");
                if (manualCheck)
                    updateHelpers.AlertNoUpdates(currentVersion.ToString());
            }
        }

        public async void ConsiderCheckingForUpdates()
        {
            if (applicationHandler.Prefs.checkUpdatesOnLogin)
            {
                await CheckForUpdates();
            }
        }
    }
}

