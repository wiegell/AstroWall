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

        public static string getDBPath()
        {
            return getDBDirectory() + "db.json";
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
                    Console.WriteLine("Success");
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
                var image = NSImage.ImageNamed("MainIcon_rot_0");
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

    }
}

