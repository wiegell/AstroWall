using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using AstroWall.ApplicationLayer.Helpers;
using Foundation;

namespace AstroWall
{

    public class General
    {
        public General()
        {
        }
        public static string currentVersion()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
        }

        public static string getAstroDirectory()
        {
            string appsupPath = NSFileManager.DefaultManager
                .GetUrls(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User)[0]
                .Path;
            string astroPath = (appsupPath + "/Astro Wall/").Replace("%20", " ");
            if (!Directory.Exists(astroPath))
            {
                Directory.CreateDirectory(astroPath);
            }
            return astroPath;
        }

        public static string getDBDirectory()
        {
            string dbDirectory = getAstroDirectory() + "db/";
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }
            return dbDirectory;
        }

        public static string getLogDirectory()
        {
            string dbDirectory = getAstroDirectory() + "log/";
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }
            return dbDirectory;
        }

        public static string getDBPath()
        {
            return getDBDirectory() + "db.json";
        }
        public static string getLogPath()
        {
            return getLogDirectory() + "log.txt";
        }
        public static string getPrefsPath()
        {
            return getDBDirectory() + "prefs.json";
        }

        /// <summary>
        /// The return object is a boxed bool, if the setting of wallpaper was successfull
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onAllScreens"></param>
        /// <returns></returns>
        public static async Task<object> SetWallpaper(Dictionary<Screen, string> urlToPostProcessedByNSScreen)
        {
            bool ret = false;
            object boxedBool = (object)ret;

            foreach (KeyValuePair<Screen, string> urlNSScreenKV in urlToPostProcessedByNSScreen)
            {
                string url = urlNSScreenKV.Value;
                Screen screen = urlNSScreenKV.Key;
                boxedBool = await General.SetWallpaper(screen, url);
            }

            return boxedBool;

        }
        public static async Task<object> SetWallpaper(Screen screen, string url)
        {
            return await RunOnUIThread<object>(async
           () =>
            {
                bool ret = false;
                object boxedBool = (object)ret;
                Console.Write($"Setting wallpaper {url} to screen {screen.Id}: ");
                try
                {
                    boxedBool = NSWorkspace.SharedWorkspace.SetDesktopImageUrl(NSUrl.FromFilename(url), screen.toNSScreen(), new NSDictionary(), new NSError());
                    Console.WriteLine("Success: " + boxedBool);
                }
                catch (Exception ex)
                {
                    boxedBool = false;
                    Console.WriteLine("Fail - " + ex.GetType());
                }
                return boxedBool;
            });
        }


        public static string getCurrentWallpaperPath()
        {
            NSWorkspace workspace = NSWorkspace.SharedWorkspace;
            NSScreen mainScreen = NSScreen.MainScreen;
            return workspace.DesktopImageUrl(mainScreen).Path;
        }

        public static async void InitIcon(NSStatusItem item, AppKit.NSMenu menu)
        {
            Func<Task<object>> ac = async () =>
            {
                var image = NSImage.ImageNamed("MainIcon_rot_400");
                image.Template = true;
                item.Button.Image = image;
                item.HighlightMode = true;
                item.Menu = menu;
                item.Length = 20;
                return null;
            };
            await RunOnUIThread<object>(ac);
        }

        public static async Task<T2> RunOnUIThread<T2>(Func<Task<T2>> ac) where T2 : class
        {
            T2 returnVal = null;
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchSync(async () => { returnVal = await ac(); });
            }
            else returnVal = await ac();
            return returnVal;
        }
        public static void RunOnUIThread(Action ac)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => ac());
            }
            else ac();
        }


        public static void Relaunch(int delay = 2)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            $"sleep {delay}; open /Applications/Astro\\ Wall.app"
            };
            nstask.Launch();
        }

        public static void Open(string url)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "open "+url
            };
            //+" "
            nstask.Launch();
            nstask.WaitUntilExit();
        }

        public static string GetInstallPath()
        {
            return NSRunningApplication.CurrentApplication.BundleUrl.Path;
        }

        public static string GetUserApplicationsPath()
        {
            string path = NSFileManager.DefaultManager
     .GetUrls(NSSearchPathDirectory.ApplicationDirectory, NSSearchPathDomain.User)[0]
     .Path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Console.WriteLine("Created user applications folder: " + path);
            }
            else
            {
                Console.WriteLine("user app folder found at: " + path);
            }
            return path;
        }

        public static string WantedBundleInstallPathInUserApplications()
        {
            return GetUserApplicationsPath() + "/Astro Wall.app";
        }

        private static string systemApplicationsFolder()
        {
            return "/Applications";
        }
        private static string symlinkPathInSystemApplications()
        {
            return systemApplicationsFolder() + "/Astro Wall.app";
        }

        public static void moveBundleToUserApplicationsFolder()
        {
            string wantedInstallLocation = WantedBundleInstallPathInUserApplications();
            string symlinkPath = symlinkPathInSystemApplications();
            string currentInstallLocation = GetInstallPath();

            // Remove potential old installation
            if (Directory.Exists(wantedInstallLocation))
            {
                var dir = new DirectoryInfo(wantedInstallLocation);
                dir.Delete(true);
            }

            // Move app to user applications folder
            NSTask mvNStask = new NSTask();
            mvNStask.LaunchPath = "/bin/bash";
            mvNStask.Arguments = new string[]
            {
                "-c",
            $"mv \"{currentInstallLocation}\" \"{wantedInstallLocation}\"",
            };
            mvNStask.Launch();
            mvNStask.WaitUntilExit();
            Console.WriteLine("wanted intern: " + wantedInstallLocation);

            // Link in applications folder
            NSTask linkNStask = new NSTask();
            linkNStask.LaunchPath = "/bin/bash";
            linkNStask.Arguments = new string[]
            {
                "-c",
             $"ln -s \"{wantedInstallLocation}\" \"{symlinkPath}\""
            };
            linkNStask.Launch();
            linkNStask.WaitUntilExit();
        }
    }
}

