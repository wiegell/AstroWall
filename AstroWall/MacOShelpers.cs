using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall
{

    public class MacOShelpers
    {
        public MacOShelpers()
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




        public static void SetWallpaper(String path, bool onAllScreens = false)
        {
            Console.WriteLine("setting wallpaper: " + path);
            NSWorkspace workspace = NSWorkspace.SharedWorkspace;
            NSScreen[] screens = NSScreen.Screens;
            NSScreen mainScreen = NSScreen.MainScreen;

            if (!onAllScreens)
            {
                workspace.SetDesktopImageUrl(NSUrl.FromFilename(path), mainScreen, new NSDictionary(), new NSError());
            }
            else
                foreach (var screen in screens)
                {
                    bool ret;
                    try
                    {
                        ret = workspace.SetDesktopImageUrl(NSUrl.FromFilename(path), screen, new NSDictionary(), new NSError());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("desk not set");
                    }
                    Console.WriteLine("");
                }
        }

        public static string getCurrentWallpaperPath()
        {
            NSWorkspace workspace = NSWorkspace.SharedWorkspace;
            NSScreen mainScreen = NSScreen.MainScreen;

            return workspace.DesktopImageUrl(mainScreen).Path;
        }

        public static void InitIcon(NSStatusItem item, AppKit.NSMenu menu)
        {
            Action ac = () =>
            {

                var image = NSImage.ImageNamed("staat");
                image.Template = true;
                item.Button.Image = image;
                item.HighlightMode = true;
                item.Menu = menu;
                item.Length = 20;
            };
            RunOnUIThread(ac);
        }

        public static void ChangeIconTo(NSStatusItem item, string iconName)
        {
            Action ac = () =>
            {
                var image = NSImage.ImageNamed(iconName);
                image.Template = true;
                item.Button.Image = image;
                item.HighlightMode = true;
            };
            RunOnUIThread(ac);
        }

        public static void RunOnUIThread(Action ac)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchSync(() => ac());
            }
            else ac();
        }

        public static void RunPKGUpdate(string pathToPkg)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "installer -pkg ~/downloads/Astro\\ Wall-1.0.4.pkg -target CurrentUserHomeDirectory"
            };
            nstask.Launch();
            
            
        }

        //private void consThread()
        //{
        //    Console.WriteLine("Current thread: " + );

        //}
    }
}

