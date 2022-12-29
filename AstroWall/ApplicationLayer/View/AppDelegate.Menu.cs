using System;
using System.Drawing;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer.Preferences;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    public partial class AppDelegate : NSApplicationDelegate
    {

        private NSStatusBar statusBar;
        private NSStatusItem statusBarItem;

        // TODO
        // Not the best place for this?
        internal bool UpdatesDisabled;

        internal void createStatusBar(string title)
        {
            // Create a Status Bar Menu
            statusBar = NSStatusBar.SystemStatusBar;
            statusBarItem = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            General.InitIcon(statusBarItem, this.StatusMenu);
            setTitle(title);
        }

        internal void UpdateMenuCheckMarks(Preferences prefs)
        {
            General.RunOnUIThread(() =>
            {
                if (!UpdatesDisabled)
                {
                    this.MenuOutletAutoInstallUpdates.State = prefs.AutoInstallUpdates ? NSCellStateValue.On : NSCellStateValue.Off;
                    this.MenuOutletCheckUpdatesOnStartup.State = prefs.CheckUpdatesOnStartup ? NSCellStateValue.On : NSCellStateValue.Off;
                }
                this.MenuOutletRunAtLogin.State = prefs.RunAtStartup ? NSCellStateValue.On : NSCellStateValue.Off;
                this.MenuOutletDailyCheckNewest.State = prefs.DailyCheck == DailyCheckEnum.Newest ? NSCellStateValue.On : NSCellStateValue.Off;
            });
        }

        internal void noAutoEnableMenuItems()
        {
            General.RunOnUIThread(() =>
            {
                StatusMenu.AutoEnablesItems = false;
                SubmenuUpdates.Submenu.AutoEnablesItems = false;
            });
        }

        internal void disableAllItemsExceptQuit()
        {
            General.RunOnUIThread(() =>
            {
                foreach (NSMenuItem item in StatusMenu.Items)
                {
                    item.Enabled = true;
                }
                MenuOutletQuit.Enabled = true;
            });
        }

        internal void enableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = true;
            });
        }

        internal void disableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = false;
            });
        }

        internal void setTitle(string title)
        {
            General.RunOnUIThread(() =>
            {
                MenuTitle.Title = title;
            });
        }

        internal void setSubTitle(string str)
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = false;
                MenuOutletState.Enabled = false;
                MenuOutletState.Title = str;
            });
        }

        internal void HideSubTitle()
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = true;
            });
        }

        internal void changeIconTo(string iconName, bool doubleCheckState = false)
        {
            Action ac = () =>
            {
                //if (doubleCheckState && appHandler.State.state != doubleCheckStateShouldHaveThisValue) return;
                var image = NSImage.ImageNamed(iconName);
                image.Template = true;
                statusBarItem.Button.Image = image;
                statusBarItem.HighlightMode = true;
            };
            // Needed since it sometimes
            // is called from another thread via
            // a task
            General.RunOnUIThread(ac);
        }

        internal void removeAllPictureItemsInSubmenu()
        {
            MenuOutletBrowseLatest.Submenu.RemoveAllItems();
        }

        internal void addPictureSubmenuItemAndRegEventHandlers(
            string title,
            BusinessLayer.State stateRef,
            bool previewIsLoaded,
            Action cancelEndBrowsingStateWithDelayCallback,
            Action setPreviewWallpaperCallback,
            Action setFullWallpaperCallback,
            Action setEndBrowsingStateWithDelayCallback,
            Action onclickCallBack
            )
        {

            General.RunOnUIThread(() =>
            {
                NSMenuItem item = new NSMenuItem(title);
                SubMenuItemHover hoverView = SubMenuItemHover.StdSize(title);

                hoverView.OnDragChange += (sender, e) =>
                {
                    if (e.Description == "Mouse Entered" && previewIsLoaded)
                    {
                        if (!stateRef.isBrowsingWallpapers)
                        {
                            stateRef.SetStateBrowsingWallpapers();
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
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("End browsing cancel");
                        }

                    }
                    if (e.Description == "Mouse Down" && previewIsLoaded)
                    {
                        onclickCallBack();
                        hoverView.DisableBGSelectionColor();
                        setFullWallpaperCallback();
                        StatusMenu.CancelTracking();
                    }
                };
                item.View = hoverView;
                MenuOutletBrowseLatest.Submenu.AddItem(item);
            });

        }

        internal void DeactivateUpdateOptions()
        {
            this.UpdatesDisabled = true;
            this.MenuOutletCheckUpdatesManual.Enabled = false;
            this.MenuOutletAutoInstallUpdates.Enabled = false;
            this.MenuOutletCheckUpdatesOnStartup.Enabled = false;
            this.MenuOutletAutoInstallUpdates.State = NSCellStateValue.Off;
            this.MenuOutletCheckUpdatesManual.State = NSCellStateValue.Off;
            this.MenuOutletCheckUpdatesOnStartup.State = NSCellStateValue.Off;
        }

        partial void MenuActionRunAtLogin(NSObject sender)
        {
            bool newState = !getCheckmarkBoolFromSender(sender);
            menuHandler.changedInMenuRunAtLogin(newState);
        }

        /// <summary>
        /// the argument is meant to disable autoinstall
        /// if checkupdatesonstartup == false
        /// </summary>
        /// <param name="enableAutoInstall"></param>
        public void EnableAllUpdateSubMenuItems(bool enableAutoInstall = true)
        {
            if (!UpdatesDisabled)
            {
                Console.WriteLine("enable all update subitems");
                this.MenuOutletAutoInstallUpdates.Enabled = enableAutoInstall;
                this.MenuOutletCheckUpdatesOnStartup.Enabled = true;
                this.MenuOutletCheckUpdatesManual.Enabled = true;
            }
        }

        partial void MenuActionAutoInstallUpdates(NSObject sender)
        {
            bool newState = !getCheckmarkBoolFromSender(sender);
            menuHandler.changedInMenuAutoInstallUpdates(newState);
        }

        partial void MenuActionCheckUpdatesOnStartup(NSObject sender)
        {
            bool newState = !getCheckmarkBoolFromSender(sender);
            menuHandler.changedInMenuCheckUpdatesAtStartup(newState);

            if (newState == false)
            {
                // Cannot autoinstall updates if they are not checked
                menuHandler.changedInMenuAutoInstallUpdates(newState);
                MenuOutletAutoInstallUpdates.State = NSCellStateValue.Off;

                MenuOutletAutoInstallUpdates.Enabled = false;
                Console.WriteLine("disable auto");
            }
            else
            {
                MenuOutletAutoInstallUpdates.Enabled = true;

            }
        }

        partial void ActionManualCheckForNewPic(NSObject sender)
        {
            menuHandler.ClickedInMenuManualCheckForNewPic();
        }

        partial void MenuActionDailyCheckNewest(NSObject sender)
        {
            bool newState = !getCheckmarkBoolFromSender(sender);
            appHandler.MenuHandler.changedInMenuDailyCheckNewest(newState);
        }

        partial void MenuActionManualCheckUpdates(NSObject sender)
        {
            menuHandler.ClickedInMenuManualCheckUpdates();
        }

        partial void MenuActionPostProcess(NSObject sender)
        {
            appHandler.Wallpaper.launchPostProcessWindow();
        }

        partial void ActionOpenCurrent(NSObject sender)
        {
            menuHandler.OpenCurrentPic();
        }

        partial void ActionOpenCurrentUrl(NSObject sender)
        {
            menuHandler.OpenUrlToCurrentPic();
        }
        partial void ActionOpenCurrentCredits(NSObject sender)
        {
            menuHandler.OpenUrlToCurrentCredits();
        }
        partial void ActionClickAbout(NSObject sender)
        {
            menuHandler.OpenAbout();
        }
        partial void ActionClickTitle(NSObject sender)
        {
            menuHandler.OpenAbout();
        }
        private static bool getCheckmarkBoolFromSender(NSObject sender)
        {
            return ((NSMenuItem)sender).State == NSCellStateValue.On;
        }
    }
}

