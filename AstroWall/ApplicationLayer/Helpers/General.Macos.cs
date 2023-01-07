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
    /// <summary>
    /// General OS specific helper methods.
    /// </summary>
    internal class General
    {
        /// <summary>
        /// Gets version string stored in plist file.
        /// </summary>
        /// <returns>E.g. 0.1.21-alpha-1-g3037297</returns>
        internal static string CurrentVersionLongWithCommit()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
        }

        /// <summary>
        /// Get the directory to store images, preferences, logs and db.
        /// Creates the directory if it doesn't exist. On MacOS the folder is located
        /// in ApplicationSupport.
        /// </summary>
        /// <returns>Absolute path to directory.</returns>
        internal static string GetAstroDirectory()
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

        /// <summary>
        /// Get the directory to store db.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        /// <returns>Absolute path to directory.</returns>
        internal static string GetDBDirectory()
        {
            string dbDirectory = GetAstroDirectory() + "db/";
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            return dbDirectory;
        }

        /// <summary>
        /// Get the directory to store logs.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        /// <returns>Absolute path to directory.</returns>
        internal static string GetLogDirectory()
        {
            string dbDirectory = GetAstroDirectory() + "log/";
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            return dbDirectory;
        }

        /// <summary>
        /// Gets path to json db. Creates the folder if it doesn't exist.
        /// </summary>
        /// <returns>Absolute path to json file.</returns>
        internal static string GetDBPath()
        {
            return GetDBDirectory() + "db.json";
        }

        /// <summary>
        /// Gets path to log txt. Creates the folder if it doesn't exist.
        /// </summary>
        /// <returns>Absolute path to txt file.</returns>
        internal static string GetLogPath()
        {
            return GetLogDirectory() + "log.txt";
        }

        /// <summary>
        /// Gets path to json prefs. Creates the folder if it doesn't exist.
        /// </summary>
        /// <returns>Absolute path to json file.</returns>
        internal static string GetPrefsPath()
        {
            return GetDBDirectory() + "prefs.json";
        }

        /// <summary>
        /// Sets wallpaper on all supplied screens.
        /// </summary>
        /// <returns>The return object is a boxed bool, if the setting of wallpaper was successfull.</returns>
        /// <param name="urlToPostProcessedByNSScreen">Dictionary with Screen and path to postprocessed file.</param>
        internal static async Task<object> SetWallpaper(Dictionary<Screen, string> urlToPostProcessedByNSScreen)
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

        /// <summary>
        /// Sets wallpaper on all screen to supplied url. Used for non-processed images.
        /// </summary>
        /// <returns>The return object is a boxed bool, if the setting of wallpaper was successfull.</returns>
        internal static async Task<object> SetWallpaper(Screen screen, string url)
        {
            return await RunOnUIThread<object>(async
           () =>
            {
                bool ret = false;
                object boxedBool = (object)ret;
                Console.Write($"Setting wallpaper {url} to screen {screen.Id}: ");
                try
                {
                    boxedBool = NSWorkspace.SharedWorkspace.SetDesktopImageUrl(NSUrl.FromFilename(url), screen.ToNSScreen, new NSDictionary(), new NSError());
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

        /// <summary>
        /// Gets current wallpaper path.
        /// </summary>
        /// <returns>Path.</returns>
        internal static string GetCurrentWallpaperPath()
        {
            NSWorkspace workspace = NSWorkspace.SharedWorkspace;
            NSScreen mainScreen = NSScreen.MainScreen;
            return workspace.DesktopImageUrl(mainScreen).Path;
        }

        /// <summary>
        /// Sets initial icon
        /// </summary>
        /// <param name="item">Menuitem.</param>
        /// <param name="menu">Menu.</param>
        internal static async void InitIcon(NSStatusItem item, AppKit.NSMenu menu)
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

        /// <summary>
        /// Runs supplied delegate on the UI thread.
        /// The delegate is only awaited if we are already on the UI thread.
        /// </summary>
        /// <typeparam name="T2">Type of return task.</typeparam>
        /// <param name="ac"></param>
        /// <returns>Returns returnval of T2 only if called on the UI thread. Otherwise returns null. TODO Make this truly async.</returns>
        internal static async Task<T2> RunOnUIThread<T2>(Func<Task<T2>> ac)
            where T2 : class
        {
            T2 returnVal = null;
            if (Environment.CurrentManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchSync(async () => { returnVal = await ac(); });
            }
            else
            {
                returnVal = await ac();
            }

            return returnVal;
        }

        /// <summary>
        /// Runs supplied delegate on the UI thread.
        /// The delegate is only awaited if we are already on the UI thread. TODO Make this truly async.
        /// </summary>
        /// <typeparam name="T2">Type of return task.</typeparam>
        /// <param name="ac"></param>
        internal static void RunOnUIThread(Action ac)
        {
            if (Environment.CurrentManagedThreadId != 1)
            {
                CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => ac());
            }
            else
            {
                ac();
            }
        }

        /// <summary>
        /// Relaunch entire app after delay.
        /// </summary>
        /// <param name="delay">Delay in seconds.</param>
        internal static void Relaunch(int delay = 2)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
                $"sleep {delay}; open /Applications/Astro\\ Wall.app",
            };
            nstask.Launch();
        }

        /// <summary>
        /// Run terminal open followed by string. Can be used e.g. to open urls in default browser.
        /// </summary>
        /// <param name="url"></param>
        internal static void Open(string url)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
                "open " + url,
            };

            nstask.Launch();
            nstask.WaitUntilExit();
        }

        /// <summary>
        /// Get installation path.
        /// </summary>
        internal static string GetInstallPath()
        {
            return NSRunningApplication.CurrentApplication.BundleUrl.Path;
        }

        /// <summary>
        /// Gets user application path.
        /// </summary>
        internal static string GetUserApplicationsPath()
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

        /// <summary>
        /// Gets the wanted install path in the user application folder,
        /// which is suited for updates.
        /// </summary>
        internal static string WantedBundleInstallPathInUserApplications()
        {
            return GetUserApplicationsPath() + "/Astro Wall.app";
        }

        /// <summary>
        /// Moves the application bundle to the user application folder.
        /// Then creates link in system applications folder.
        /// </summary>
        internal static void MoveBundleToUserApplicationsFolder()
        {
            string wantedInstallLocation = WantedBundleInstallPathInUserApplications();
            string symlinkPath = SymlinkPathInSystemApplications();
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
                $"ln -s \"{wantedInstallLocation}\" \"{symlinkPath}\"",
            };
            linkNStask.Launch();
            linkNStask.WaitUntilExit();
        }

        /// <summary>
        /// Get the system applications folder path.
        /// </summary>
        private static string SystemApplicationsFolder()
        {
            return "/Applications";
        }

        /// <summary>
        /// Path to symlink in systems applications folder.
        /// </summary>
        private static string SymlinkPathInSystemApplications()
        {
            return SystemApplicationsFolder() + "/Astro Wall.app";
        }
    }
}