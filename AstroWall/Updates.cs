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

        public  async Task DownloadLatestAndUpdate()
        {
            pathToLatestPkg = await FileHelpers.DownloadUrlToTmpPath("https://github.com/wiegell/release_test/releases/download/v1.0.0tag/Astro.Wall-1.0.pkg");
            Console.Write("Downloaded new pkg to path: "+pathToLatestPkg);
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

