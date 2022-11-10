using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace AstroWall
{

    public class GeneralHelpers
    {
        public GeneralHelpers()
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

        public static void RunOnUIThread(Action ac)
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchSync(() => ac());
            }
            else ac();
        }

        public static void SetAsLaunchAgent()
        {
            string ap = agentPath();
            string agentXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.astro.wall.Astro-Wall</string>
    <key>LimitLoadToSessionType</key>
    <string>Aqua</string>
    <key>Program</key>
    <string>/Applications/Astro Wall.app/Contents/MacOS/Astro Wall</string>
    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
";
            Console.WriteLine("Writing agent to: " + ap);
            File.WriteAllText(ap, agentXmlContent);
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

        public static void RemoveLaunchAgent()
        {
            string ap = agentPath();
            if (File.Exists(ap))
            {
                Console.WriteLine("Deleting agent at: " + ap);
                File.Delete(ap);
            };
        }

        private static string agentPath()
        {
            return NSFileManager.HomeDirectory + "/Library/LaunchAgents/com.astro.wall.Astro-Wall.plist";
        }
    }
}

