using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall
{

    public class General
    {
        public General()
        {
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
        /// The return object is a boxed bool
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onAllScreens"></param>
        /// <returns></returns>
        public static async Task<object> SetWallpaper(String path, bool onAllScreens = false)
        {
            return await RunOnUIThread<object>(async
                   () =>
               {
                   Console.WriteLine("setting wallpaper: " + path);
                   NSWorkspace workspace = NSWorkspace.SharedWorkspace;
                   NSScreen[] screens = NSScreen.Screens;
                   NSScreen mainScreen = NSScreen.MainScreen;
                   Console.WriteLine("screen count: " + screens.Length);

                   bool ret = false;
                   object boxedBool = (object)ret;
                   if (!onAllScreens)
                   {
                       boxedBool = workspace.SetDesktopImageUrl(NSUrl.FromFilename(path), mainScreen, new NSDictionary(), new NSError());
                   }
                   else
                       foreach (var screen in screens)
                       {
                           try
                           {
                               boxedBool = workspace.SetDesktopImageUrl(NSUrl.FromFilename(path), screen, new NSDictionary(), new NSError());
                           }
                           catch (Exception)
                           {
                               Console.WriteLine("desk not set, returning false");
                               boxedBool = false;
                           }
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

    }
}

