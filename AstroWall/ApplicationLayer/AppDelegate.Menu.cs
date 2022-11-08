using System;
using System.Drawing;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    public partial class AppDelegate : NSApplicationDelegate
    {

        private NSStatusBar statusBar;
        private NSStatusItem statusBarItem;

        public void createStatusBar(string title)
        {
            // Create a Status Bar Menu
            statusBar = NSStatusBar.SystemStatusBar;
            statusBarItem = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            MacOShelpers.InitIcon(statusBarItem, this.StatusMenu);
            MenuTitle.Title = title;
        }

        public void updateMenuCheckMarks(Preferences prefs)
        {
            this.MenuOutletAutoInstallUpdates.State = prefs.autoInstallUpdates ? NSCellStateValue.On : NSCellStateValue.Off;
            this.MenuOutletCheckUpdatesAtLogin.State = prefs.checkUpdatesOnLogin ? NSCellStateValue.On : NSCellStateValue.Off;
            this.MenuOutletInstallUpdatesSilently.State = prefs.autoInstallSilent ? NSCellStateValue.On : NSCellStateValue.Off;
        }

        public void noAutoEnableMenuItems()
        {
            StatusMenu.AutoEnablesItems = false;
        }

        public void disableAllItemsExceptQuit()
        {
            foreach (NSMenuItem item in StatusMenu.Items)
            {
                item.Enabled = true;
            }
            MenuOutletQuit.Enabled = true;
        }

        public void enableStatusIcon()
        {
            statusBarItem.Enabled = true;
        }

        public void disableStatusIcon()
        {
            statusBarItem.Enabled = false;
        }

        public void setTitle(string title)
        {
            MenuTitle.Title = title;
        }

        public void setSubTitle(string str)
        {
            MenuOutletState.Hidden = true;
            MenuOutletState.Title = str;
        }

        public void hideSubTitle()
        {
            MenuOutletState.Hidden = true;
        }

        public void changeIconTo(string iconName)
        {
            Action ac = () =>
            {
                var image = NSImage.ImageNamed(iconName);
                image.Template = true;
                statusBarItem.Button.Image = image;
                statusBarItem.HighlightMode = true;
            };
            // Needed since it sometimes
            // is called from another thread via
            // a task
            MacOShelpers.RunOnUIThread(ac);
        }

        public void removeAllPictureItemsInSubmenu()
        {
            MenuOutletBrowseLatest.Submenu.RemoveAllItems();
        }

        public void addPictureSubmenuItemAndRegEventHandlers(
            string title,
            BusinessLayer.State stateRef,
            bool previewIsLoaded,
            Action cancelEndBrowsingStateWithDelayCallback,
            Action setPreviewWallpaperCallback,
            Action setFullWallpaperCallback,
            Action setEndBrowsingStateWithDelayCallback,
            Action cancelTrackingCallback
            )
        {
            NSMenuItem item = new NSMenuItem(title);
            SubMenuItemHover hoverView = SubMenuItemHover.StdSize(title);

            hoverView.OnDragChange += (sender, e) =>
            {
                if (e.Description == "Mouse Entered" && previewIsLoaded)
                {
                    if (stateRef.state != BusinessLayer.stateEnum.BrowsingWallpapers)
                    {
                        stateRef.setStateBrowsing();
                    }
                    cancelEndBrowsingStateWithDelayCallback();
                    setPreviewWallpaperCallback();
                }
                if (e.Description == "Mouse Exited")
                {
                    Console.WriteLine("Mouse exit");
                    try
                    {
                        setEndBrowsingStateWithDelayCallback();
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine("End browsing cancel");
                    }

                }
                if (e.Description == "Mouse Down" && previewIsLoaded)
                {
                    cancelTrackingCallback();
                    hoverView.DisableBGSelectionColor();
                    setFullWallpaperCallback();
                    //prefs.currentAstroWallpaper = iw;
                    //MacOShelpers.SetWallpaper(iw.ImgLocalUrl, true);
                }
            };
            item.View = hoverView;
            MenuOutletBrowseLatest.Submenu.AddItem(item);
        }
    }
}

