using System;
using System.Drawing;
using AppKit;
using AstroWall.BusinessLayer.Preferences;
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
            General.InitIcon(statusBarItem, this.StatusMenu);
            MenuTitle.Title = title;
        }

        public void updateMenuCheckMarks(Preferences prefs)
        {
            General.RunOnUIThread(() =>
            {
                this.MenuOutletAutoInstallUpdates.State = prefs.AutoInstallUpdates ? NSCellStateValue.On : NSCellStateValue.Off;
                this.MenuOutletCheckUpdatesOnStartup.State = prefs.CheckUpdatesOnStartup ? NSCellStateValue.On : NSCellStateValue.Off;
                this.MenuOutletRunAtLogin.State = prefs.RunAtStartup ? NSCellStateValue.On : NSCellStateValue.Off;
                this.MenuOutletDailyCheckNewest.State = prefs.DailyCheck == DailyCheckEnum.Newest ? NSCellStateValue.On : NSCellStateValue.Off;
            });
        }

        public void noAutoEnableMenuItems()
        {
            General.RunOnUIThread(() =>
            {
                StatusMenu.AutoEnablesItems = false;
                SubmenuUpdates.Submenu.AutoEnablesItems = false;
            });
        }

        public void disableAllItemsExceptQuit()
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

        public void enableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = true;
            });
        }

        public void disableStatusIcon()
        {
            General.RunOnUIThread(() =>
            {
                statusBarItem.Enabled = false;
            });
        }

        public void setTitle(string title)
        {
            General.RunOnUIThread(() =>
            {
                MenuTitle.Title = title;
            });
        }

        public void setSubTitle(string str)
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = true;
                MenuOutletState.Title = str;
            });
        }

        public void hideSubTitle()
        {
            General.RunOnUIThread(() =>
            {
                MenuOutletState.Hidden = true;
            });
        }

        public void changeIconTo(string iconName, bool doubleCheckState = false, BusinessLayer.stateEnum doubleCheckStateShouldHaveThisValue = BusinessLayer.stateEnum.BrowsingWallpapers)
        {
            Action ac = () =>
            {
                if (doubleCheckState && appHandler.State.state != doubleCheckStateShouldHaveThisValue) return;
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
            Console.WriteLine("enable all update subitems");
            this.MenuOutletAutoInstallUpdates.Enabled = enableAutoInstall;
            this.MenuOutletCheckUpdatesOnStartup.Enabled = true;
            this.MenuOutletCheckUpdatesManual.Enabled = true;
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


        partial void MenuManualCheckPic(Foundation.NSObject sender)
        {
            //state.setStateIdle();
            //MacOShelpers.InitIcon2(statusBarItem, this.StatusMenu);
            //string imgurl = HTMLHelpers.getImgUrl();
            //Task<string> tmpFilePath = FileHelpers.DownloadUrlToTmpPath(imgurl);
            ////MacOShelpers.SetWallpaper(tmpFilePath);
            //Console.WriteLine("file dl");
            //MacOShelpers.RunPKGUpdate();
        }

        partial void MenuActionDailyCheckNewest(NSObject sender)
        {
            bool newState = !getCheckmarkBoolFromSender(sender);
            appHandler.MenuHandler.changedInMenuDailyCheckNewest(newState);
        }

        partial void MenuActionManualCheckUpdates(NSObject sender)
        {
            menuHandler.clickedInMenuManualCheckUpdates();
        }

        partial void MenuActionPostProcess(NSObject sender)
        {
            appHandler.Wallpaper.launchPostProcessWindow();
        }

        private bool getCheckmarkBoolFromSender(NSObject sender)
        {
            return ((NSMenuItem)sender).State == NSCellStateValue.On;
        }
    }
}

