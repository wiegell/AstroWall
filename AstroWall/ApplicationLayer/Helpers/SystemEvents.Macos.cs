using System;
using System.IO;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    /// <summary>
    /// Helper class for stuff related to system events, e.g. wake and login.
    /// </summary>
    public sealed class SystemEvents
    {
        // Singleton related fields
        private static volatile SystemEvents instance;
        private static object syncRoot = new object();

        // Handlers
        private NSObject updateWakeHandlerObserver;
        private NSObject wallpaperLoginHandlerObserver;

        // Should only be available as singleton, therefore priv. constructor
        private SystemEvents()
        {
        }

        /// <summary>
        /// Gets singleton.
        /// </summary>
        internal static SystemEvents Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SystemEvents();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Sets launch agent to launch app on login.
        /// </summary>
        internal static void SetLaunchAgent()
        {
            string ap = GetAgentPath();
            string agentXmlContent =
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
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

        /// <summary>
        /// Removes launch agent to not start app on login.
        /// </summary>
        internal static void RemoveLaunchAgent()
        {
            string ap = GetAgentPath();
            if (File.Exists(ap))
            {
                Console.WriteLine("Deleting agent at: " + ap);
                File.Delete(ap);
            }
        }

        /// <summary>
        /// Registers update action to be called on wake.
        /// </summary>
        internal void RegisterUpdateWakeHandler(Action<NSNotification> ac)
        {
            updateWakeHandlerObserver =
            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DidWakeNotification, ac);
        }

        /// <summary>
        /// Unregisters callback on wake.
        /// </summary>
        internal void UnRegisterUpdateWakeHandler()
        {
            // TODO possible null exception?
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(updateWakeHandlerObserver);
        }

        /// <summary>
        /// Registers wallpaper refresh action to be called on wake.
        /// </summary>
        internal void RegisterWallpaperWakeHandler(Action<NSNotification> ac)
        {
            wallpaperLoginHandlerObserver =
            NSWorkspace.
            SharedWorkspace
            .NotificationCenter.AddObserver(NSWorkspace.DidWakeNotification, ac);
        }

        /// <summary>
        /// Unregisters wallpaper refresh callback on login.
        /// </summary>
        internal void UnRegisterWallpaperLoginHandler()
        {
            // TODO possible null exception?
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(wallpaperLoginHandlerObserver);
        }

        /// <summary>
        /// Gets path of launch agent. Creates the folder if it does not exist.
        /// </summary>
        private static string GetAgentPath()
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