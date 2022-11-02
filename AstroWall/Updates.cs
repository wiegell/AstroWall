using System;
using System.Threading.Tasks;
using Foundation;

namespace AstroWall
{
    public class Updates
    {
        public Updates()
        {
        }

        private string pathToLatestPkg;

        public async Task downloadLatest()
        {
            pathToLatestPkg = await FileHelpers.DownloadUrlToTmpPath("https://github.com/wiegell/release_test/releases/download/v1.0.0tag/Astro.Wall-1.0.pkg");
        }

        public static void RunPKGUpdate(string pathToPkg)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "installer -pkg "+pathToPkg+" -target CurrentUserHomeDirectory"
            };
            nstask.Launch();
        }
    }
}

