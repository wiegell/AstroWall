using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;

namespace AstroWall.BusinessLayer
{
    public class Updates
    {
        private Version currentVersion;

        public Updates(string currentVersionString)
        {
            this.currentVersion = VersionFromString(currentVersionString);
            Console.WriteLine("currentVersion: " + VersionFromString(currentVersionString));
        }

        private UpdateLibrary.UpdateManifest manifest;

        private string pathToLatestPkg;

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


        public async Task DownloadLatestAndUpdate()
        {
            pathToLatestPkg = await FileHelpers.DownloadUrlToTmpPath("https://github.com/wiegell/AstroWall/releases/download/v0.0.2-alpha/Astro.pkg");
            Console.Write("Downloaded new pkg to path: " + pathToLatestPkg);
            RunPKGUpdate();
        }

        public void RunPKGUpdate()
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "installer -pkg "+pathToLatestPkg+" -target CurrentUserHomeDirectory"
            };
            nstask.Launch();
        }
    }
}

