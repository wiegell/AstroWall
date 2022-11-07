using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;

namespace AstroWall
{
    public enum stateEnum
    {
        Initializing,
        Idle,
        ResolvingUrl,
        UpdatingDatabase,
        DownloadingNewest,
        PostProcessing,
        SettingWallpaper,
        BrowsingWallpapers
    }
    public class State
    {
        // Menu
        private NSMenu menu;
        private NSStatusItem statusItem;
        private Dictionary<string, NSMenuItem> menuItemsById = new Dictionary<string, NSMenuItem>();
        private static System.Timers.Timer iconUpdateTimer;
        private int flipCounter = 0;

        // Overall state
        public stateEnum state { get; private set; }
        private Database db;
        private Preferences prefs;

        // Browsing state
        private Task restoreToIdleWithDelayTask;
        private CancellationTokenSource taskCancellationSource;
        private CancellationToken cancellationToken;

        // Updates
        public Version currentVersion { private set; get; }
        private Updates updates;
        public UpdateLibrary.Release pendingUpdate { private set; get; }

        // currentVersionString is the long tag from git including commit hash  
        public State(NSMenu menuArg, NSStatusItem statusItemArg, string currentVersionString)
        {
            renewCancellationSource();

            menu = menuArg;
            statusItem = statusItemArg;
            menu.AutoEnablesItems = false;
            foreach (NSMenuItem item in menu.Items)
            {
                menuItemsById.Add(item.Identifier, item);
            }

            updates = new Updates(currentVersionString);
        }

        public async Task<Boolean> GetUpdateManifestAndCheckIfUpdatePending()
        {
            await updates.GetManifest();
            Console.WriteLine("Update manifest downloaded and parsed");
            pendingUpdate = updates.checkManifestForNewer();
            bool hasPendingUpdate = pendingUpdate == null ? false : true;
            Console.WriteLine("Has pending update: " + hasPendingUpdate);
            return hasPendingUpdate;
        }

        private void renewCancellationSource()
        {
            this.taskCancellationSource = new CancellationTokenSource();
            this.cancellationToken = taskCancellationSource.Token;
        }

        public void LoadOrCreateDB()
        {
            db = new Database();
        }

        public void LoadOrCreatePrefs()
        {
            prefs = Preferences.fromSaveOrNew();
        }

        private void disableAllItems()
        {
            foreach (NSMenuItem item in menu.Items)
            {
                item.Enabled = true;
            }
            menuItemsById["quit"].Enabled = true;
            menuItemsById["about"].Enabled = true;

        }

        public void saveDBToDisk()
        {
            db.SaveToDisk();
        }

        public void savePrefsToDisk()
        {
            prefs.SaveToDisk();
        }

        public void SetStateInitializing()
        {
            Console.WriteLine("initializing");
            state = stateEnum.Initializing;
            disableAllItems();
            menuItemsById["state"].Title = "Initializing...";
            RunDownloadIconAnimation();
        }

        public async Task UpdateStateFromOnline()
        {
            Console.WriteLine("load data");
            await db.LoadDataButNoImgFromOnlineStartingAtDate(10, DateTime.Now);
            Console.WriteLine("load img");
            await db.LoadImgs();
            Console.WriteLine("wraplist: " + db.ImgWrapList.Count);
            foreach (ImgWrap pic in db.ImgWrapList)
            {
                Console.WriteLine("preview url: " + pic.ImgLocalPreviewUrl);
            }
        }

        public void PopulateMenu()
        {

            // Clear existing items of menu
            menuItemsById["browse"].Submenu.RemoveAllItems();

            foreach (ImgWrap iw in db.ImgWrapList.Where((iw) => iw.imgIsGettable))
            {
                string title = iw.Title;



                NSMenuItem item = new NSMenuItem(title);
                SubMenuItemHover hoverView = SubMenuItemHover.StdSize(title);
                if (!iw.OnlineDataExceptPicIsLoaded() || !iw.integrity)
                {
                    //item.V = false;
                }
                else
                {
                    hoverView.OnDragChange += (sender, e) =>
                    {
                        if (e.Description == "Mouse Entered" && iw.PreviewIsLoaded())
                        {
                            if (state != stateEnum.BrowsingWallpapers)
                            {
                                Console.WriteLine("new browsing, setting img cache");
                                state = stateEnum.BrowsingWallpapers;
                            }
                            cancelEndBrowsingStateWithDelay();
                            MacOShelpers.SetWallpaper(iw.ImgLocalPreviewUrl);
                        }
                        if (e.Description == "Mouse Exited")
                        {
                            Console.WriteLine("Mouse exit");
                            try
                            {
                                restoreToIdleWithDelayTask = setEndBrowsingStateWithDelay();
                            }
                            catch (OperationCanceledException ex)
                            {
                                Console.WriteLine("End browsing cancel");
                            }

                        }
                        if (e.Description == "Mouse Down" && iw.PreviewIsLoaded())
                        {
                            //statusItem.Button.PerformClick(sender as NSObject);
                            statusItem.Menu.CancelTracking();
                            hoverView.DisableBGSelectionColor();
                            prefs.currentAstroWallpaper = iw;
                            MacOShelpers.SetWallpaper(iw.ImgLocalUrl, true);
                        }
                    };
                    item.View = hoverView;
                    menuItemsById["browse"].Submenu.AddItem(item);
                }




            }
        }

        private async Task setEndBrowsingStateWithDelay()
        {
            await Task.Delay(100, this.cancellationToken);
            MacOShelpers.SetWallpaper(
                prefs.hasAstroWall() ? prefs.currentAstroWallpaper.ImgLocalUrl : prefs.currentPathToNonAstroWallpaper
                , true);
            SetStateIdle();
        }

        private void cancelEndBrowsingStateWithDelay()
        {
            this.taskCancellationSource.Cancel();
            renewCancellationSource();
        }

        public void SetStateIdle()
        {
            iconUpdateTimer.Stop();
            Console.WriteLine("set state idle:");
            Task.Run(() =>
            {
                Task.Delay(500);
                MacOShelpers.ChangeIconTo(statusItem, "staat");
            });
            state = stateEnum.Idle;
            menuItemsById["state"].Title = "Idle";
            menuItemsById["state"].Hidden = true;

        }

        private void RunDownloadIconAnimation()
        {
            // Create a timer with a two second interval.
            iconUpdateTimer = new System.Timers.Timer(500);
            // Hook up the Elapsed event for the timer. 
            iconUpdateTimer.Elapsed += OnTimedEvent;
            iconUpdateTimer.AutoReset = true;
            iconUpdateTimer.Enabled = true;
        }

        public async Task FireUpdateHandler()
        {
            bool hasPendingUpdate = await GetUpdateManifestAndCheckIfUpdatePending();
            if (hasPendingUpdate)
            {
                Console.WriteLine("Has pending update: {0}", pendingUpdate.version);
            }
            else
            {
                Console.WriteLine("No pending updates, is up to date");
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            int flipCounter1based = flipCounter + 1;
            string iconName = "download" + flipCounter1based;
            MacOShelpers.ChangeIconTo(statusItem, iconName);

            flipCounter = (flipCounter + 1) % 3;
        }

    }
}

