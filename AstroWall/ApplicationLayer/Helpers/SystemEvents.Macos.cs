using System;
using System.IO;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    public sealed class SystemEvents
    {
        //Singleton
        private static volatile SystemEvents instance;
        private static object syncRoot = new object();
        public static SystemEvents Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SystemEvents();
                    }
                }

                return instance;
            }
        }

        NSObject updateWakeHandlerObserver;
        NSObject wallpaperLoginHandlerObserver;

        private SystemEvents()
        {
        }

        public void UnRegisterUpdateWakeHandler()
        {
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(updateWakeHandlerObserver);
        }

        public void RegisterUpdateWakeHandler(Action<NSNotification> ac)
        {
            updateWakeHandlerObserver =
            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DidWakeNotification, ac);
        }

        public void UnRegisterWallpaperLoginHandler()
        {
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(wallpaperLoginHandlerObserver);
        }

        public void RegisterWallpaperWakeHandler(Action<NSNotification> ac)
        {
            wallpaperLoginHandlerObserver =
            NSWorkspace.
            SharedWorkspace
            .NotificationCenter.AddObserver(NSWorkspace.DidWakeNotification, ac);

        }


        public static void SetAsLaunchAgent()
        {
            string ap = agentPath();
            string agentXmlContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>com.astro.wall.Astro-Wall</string>
    <key>LimitLoadToSessionType</key>
    <string>Aqua</string>
    <key>Program</key>
    <string>{General.GetInstallPath()}/Contents/MacOS/Astro Wall</string>
    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
";
            Console.WriteLine("Writing agent to: " + ap);
            File.WriteAllText(ap, agentXmlContent);
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
            string agentFolder = NSFileManager.HomeDirectory + "/Library/LaunchAgents/";

            if (!Directory.Exists(agentFolder))
            {
                Directory.CreateDirectory(agentFolder);
            }
            return agentFolder + "com.astro.wall.Astro-Wall.plist";
        }
    }
}

