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

        private UpdateLibrary.UpdateManifest manifest;

        private string pathToLatestPkg;

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

