using System;
using System.Drawing;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer.Preferences;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    /// <summary>
    /// Entry point of MacOS app and bootstrap of the business logic. Divided has partial .menu file
    /// with all the OS-specific handling about the menu.
    /// </summary>
    public partial class AppDelegate : NSApplicationDelegate
    {
        private NSStatusBar statusBar;
        private NSStatusItem statusBarItem;

        /// <summary>
        /// Gets or sets a value indicating whether updates are disabled. This is state from application layer.
        /// Should be generalized to business logic. TODO.
        /// </summary>
        internal bool UpdatesDisabled { get; set; }

        /// <summary>
        /// Creates status bar item.
        /// </summary>
        /// <param name="title"></param>
        internal void CreateStatusBar(string title)
        {
            // Create a Status Bar Menu
            statusBar = NSStatusBar.SystemStatusBar;
            statusBarItem = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            General.InitIcon(statusBarItem, this.StatusMenu);
            SetTitle(title);
        }

        /// <summary>
        /// Updates UI checkmarks in menu to reflect input value.
        /// </summary>
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

        /// <summary>
        /// Prevents macOS from autoenabling menu items.
        /// </summary>
        internal void NoAutoEnableMenuItems()
        {
            General.RunOnUIThread(() =>
            {
                StatusMenu.AutoEnablesItems = false;
                SubmenuUpdates.Submenu.AutoEnablesItems = false;
            });
        }

        /// <summary>
        /// Disable all menu items except quit.
        /// </summary>
        internal void DisableAllItemsExceptQuit()
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

        /// <summary>
        /// Enables status icon.
        /// </summary>
        internal void EnableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = true;
            });
        }

        /// <summary>
        /// Disables status icon.
        /// </summary>
        internal void DisableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = false;
            });
        }

        /// <summary>
        /// Sets title in menu item.
        /// </summary>
        /// <param name="title"></param>
        internal void SetTitle(string title)
        {
            General.RunOnUIThread(() =>
            {
                MenuTitle.Title = title;
            });
        }

        /// <summary>
        /// Sets subtitle in menu as well as shows and enables the item.
        /// </summary>
        /// <param name="str"></param>
        internal void SetSubTitle(string str)
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = false;
                MenuOutletState.Enabled = false;
                MenuOutletState.Title = str;
            });
        }

        /// <summary>
        /// Hides subtitle.
        /// </summary>
        internal void HideSubTitle()
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = true;
            });
        }

        /// <summary>
        /// Changes icon in statusbar to supplied iconName.
        /// </summary>
        /// <param name="iconName">xAsset name</param>
        internal void ChangeIconTo(string iconName)
        {
            Action ac = () =>
            {
                var image = NSImage.ImageNamed(iconName);
                image.Template = true;
                statusBarItem.Button.Image = image;
                statusBarItem.HighlightMode = true;
            };

            General.RunOnUIThread(ac);
        }

        /// <summary>
        /// Clears submenu with "latest" images.
        /// </summary>
        internal void ClearAllPictureItemsInSubmenu()
        {
            MenuOutletBrowseLatest.Submenu.RemoveAllItems();
        }

        /// <summary>
        /// Adds picture to "latest" images submenu.
        /// </summary>
        /// <param name="title">the text displayed in the menu item.</param>
        /// <param name="stateRef">Ref is used to check if state is already "browsing wallpapers".</param>
        /// <param name="previewIsLoaded">Only changes wallpaper if preview is loaded.</param>
        /// <param name="cancelEndBrowsingStateCallback">This callback is meant to cancel a timer that ends the browsing state.</param>
        /// <param name="setPreviewWallpaperCallback">Callback to set preview.</param>
        /// <param name="startTimerToEndBrowsingStateCallback">This timer triggers an end to browsing state, if the user e.g. moves the mouse outside the menu and not directly to another menu item.</param>
        internal void AddPictureSubmenuItemAndRegEventHandlers(
            string title,
            BusinessLayer.State stateRef,
            bool previewIsLoaded,
            Action cancelEndBrowsingStateCallback,
            Action setPreviewWallpaperCallback,
            Action startTimerToEndBrowsingStateCallback,
            Action onclickCallBack)
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

                        cancelEndBrowsingStateCallback();
                        setPreviewWallpaperCallback();
                    }

                    if (e.Description == "Mouse Exited")
                    {
                        Console.WriteLine("Mouse exit");
                        try
                        {
                            startTimerToEndBrowsingStateCallback();
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("End browsing cancel");
                        }
                    }

                    if (e.Description == "Mouse Down" && previewIsLoaded)
                    {
                        // TODO is this a suboptimal order?
                        onclickCallBack();
                        hoverView.DisableBGSelectionColor();
                        StatusMenu.CancelTracking();
                    }
                };
                item.View = hoverView;
                MenuOutletBrowseLatest.Submenu.AddItem(item);
            });
        }

        /// <summary>
        /// Deactivates the option to click updates related menu items.
        /// </summary>
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

        /// <summary>
        /// Enables all submenuitems in the update menu.
        /// </summary>
        /// <param name="enableAutoInstall">
        /// The argument is meant to disable autoinstall
        /// if checkupdatesonstartup == false.
        /// </param>
        internal void EnableAllUpdateSubMenuItems(bool enableAutoInstall = true)
        {
            if (!UpdatesDisabled)
            {
                Console.WriteLine("enable all update subitems");
                this.MenuOutletAutoInstallUpdates.Enabled = enableAutoInstall;
                this.MenuOutletCheckUpdatesOnStartup.Enabled = true;
                this.MenuOutletCheckUpdatesManual.Enabled = true;
            }
        }

        private static bool GetCheckmarkBoolFromSender(NSObject sender)
        {
            return ((NSMenuItem)sender).State == NSCellStateValue.On;
        }

        partial void MenuActionRunAtLogin(NSObject sender)
        {
            bool newState = !GetCheckmarkBoolFromSender(sender);
            this.AppHandler.MenuHandler.changedInMenuRunAtLogin(newState);
        }

        partial void MenuActionAutoInstallUpdates(NSObject sender)
        {
            bool newState = !GetCheckmarkBoolFromSender(sender);
            this.AppHandler.MenuHandler.changedInMenuAutoInstallUpdates(newState);
        }

        partial void MenuActionCheckUpdatesOnStartup(NSObject sender)
        {
            bool newState = !GetCheckmarkBoolFromSender(sender);
            this.AppHandler.MenuHandler.changedInMenuCheckUpdatesAtStartup(newState);

            if (newState == false)
            {
                // Cannot autoinstall updates if they are not checked
                this.AppHandler.MenuHandler.changedInMenuAutoInstallUpdates(newState);
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
            this.AppHandler.MenuHandler.ClickedInMenuManualCheckForNewPic();
        }

        partial void MenuActionDailyCheckNewest(NSObject sender)
        {
            bool newState = !GetCheckmarkBoolFromSender(sender);
            AppHandler.MenuHandler.changedInMenuDailyCheckNewest(newState);
        }

        partial void MenuActionManualCheckUpdates(NSObject sender)
        {
            this.AppHandler.MenuHandler.ClickedInMenuManualCheckUpdates();
        }

        partial void MenuActionPostProcess(NSObject sender)
        {
            AppHandler.Wallpaper.launchPostProcessWindow();
        }

        partial void ActionOpenCurrent(NSObject sender)
        {
            this.AppHandler.MenuHandler.OpenCurrentPic();
        }

        partial void ActionOpenCurrentUrl(NSObject sender)
        {
            this.AppHandler.MenuHandler.OpenUrlToCurrentPic();
        }

        partial void ActionOpenCurrentCredits(NSObject sender)
        {
            this.AppHandler.MenuHandler.OpenUrlToCurrentCredits();
        }

        partial void ActionClickAbout(NSObject sender)
        {
            this.AppHandler.MenuHandler.LaunchAboutWindow();
        }

        partial void ActionClickTitle(NSObject sender)
        {
            this.AppHandler.MenuHandler.LaunchAboutWindow();
        }
    }
}