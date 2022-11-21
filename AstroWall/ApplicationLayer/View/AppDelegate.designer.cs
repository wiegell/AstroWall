// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AstroWall.ApplicationLayer
{
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSMenuItem MenuOutletAutoInstallUpdates { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletBrowseLatest { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletCheckUpdatesManual { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletCheckUpdatesOnStartup { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletDailyCheckNewest { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletPostProcess { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletQuit { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletRunAtLogin { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletState { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuTitle { get; set; }

		[Outlet]
		AppKit.NSMenuItem OutletCurrentDetails { get; set; }

		[Outlet]
		AppKit.NSMenuItem OutletOpenCurrentPic { get; set; }

		[Outlet]
		AppKit.NSMenuItem OutletOpenUrlToCurrentPic { get; set; }

		[Outlet]
		AppKit.NSMenu StatusMenu { get; set; }

		[Outlet]
		AppKit.NSMenuItem SubmenuUpdates { get; set; }

		[Action ("ActionCheckUpdatesAtStartup:")]
		partial void ActionCheckUpdatesAtStartup (Foundation.NSObject sender);

		[Action ("ActionManualCheckForNewPic:")]
		partial void ActionManualCheckForNewPic (Foundation.NSObject sender);

		[Action ("ActionOpenCurrent:")]
		partial void ActionOpenCurrent (Foundation.NSObject sender);

		[Action ("ActionOpenCurrentCredits:")]
		partial void ActionOpenCurrentCredits (Foundation.NSObject sender);

		[Action ("ActionOpenCurrentPic:")]
		partial void ActionOpenCurrentPic (Foundation.NSObject sender);

		[Action ("ActionOpenCurrentUrl:")]
		partial void ActionOpenCurrentUrl (Foundation.NSObject sender);

		[Action ("ActionOpenUrlToCurrentPic:")]
		partial void ActionOpenUrlToCurrentPic (Foundation.NSObject sender);

		[Action ("ManualCheckForNewPic:")]
		partial void ManualCheckForNewPic (Foundation.NSObject sender);

		[Action ("MenuActionAutoInstallUpdates:")]
		partial void MenuActionAutoInstallUpdates (Foundation.NSObject sender);

		[Action ("MenuActionCheckUpdatesOnStartup:")]
		partial void MenuActionCheckUpdatesOnStartup (Foundation.NSObject sender);

		[Action ("MenuActionDailyCheckNewest:")]
		partial void MenuActionDailyCheckNewest (Foundation.NSObject sender);

		[Action ("MenuActionManualCheckUpdates:")]
		partial void MenuActionManualCheckUpdates (Foundation.NSObject sender);

		[Action ("MenuActionPostProcess:")]
		partial void MenuActionPostProcess (Foundation.NSObject sender);

		[Action ("MenuActionRunAtLogin:")]
		partial void MenuActionRunAtLogin (Foundation.NSObject sender);

		[Action ("MenuManualCheckPic:")]
		partial void MenuManualCheckPic (Foundation.NSObject sender);

		[Action ("MenuOutletCheckPicOnLogin:")]
		partial void MenuOutletCheckPicOnLogin (Foundation.NSObject sender);

		[Action ("OutletOpenCurrent:")]
		partial void OutletOpenCurrent (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (MenuOutletAutoInstallUpdates != null) {
				MenuOutletAutoInstallUpdates.Dispose ();
				MenuOutletAutoInstallUpdates = null;
			}

			if (MenuOutletBrowseLatest != null) {
				MenuOutletBrowseLatest.Dispose ();
				MenuOutletBrowseLatest = null;
			}

			if (MenuOutletCheckUpdatesManual != null) {
				MenuOutletCheckUpdatesManual.Dispose ();
				MenuOutletCheckUpdatesManual = null;
			}

			if (MenuOutletCheckUpdatesOnStartup != null) {
				MenuOutletCheckUpdatesOnStartup.Dispose ();
				MenuOutletCheckUpdatesOnStartup = null;
			}

			if (MenuOutletDailyCheckNewest != null) {
				MenuOutletDailyCheckNewest.Dispose ();
				MenuOutletDailyCheckNewest = null;
			}

			if (MenuOutletPostProcess != null) {
				MenuOutletPostProcess.Dispose ();
				MenuOutletPostProcess = null;
			}

			if (MenuOutletQuit != null) {
				MenuOutletQuit.Dispose ();
				MenuOutletQuit = null;
			}

			if (MenuOutletRunAtLogin != null) {
				MenuOutletRunAtLogin.Dispose ();
				MenuOutletRunAtLogin = null;
			}

			if (MenuOutletState != null) {
				MenuOutletState.Dispose ();
				MenuOutletState = null;
			}

			if (MenuTitle != null) {
				MenuTitle.Dispose ();
				MenuTitle = null;
			}

			if (OutletOpenCurrentPic != null) {
				OutletOpenCurrentPic.Dispose ();
				OutletOpenCurrentPic = null;
			}

			if (OutletOpenUrlToCurrentPic != null) {
				OutletOpenUrlToCurrentPic.Dispose ();
				OutletOpenUrlToCurrentPic = null;
			}

			if (StatusMenu != null) {
				StatusMenu.Dispose ();
				StatusMenu = null;
			}

			if (SubmenuUpdates != null) {
				SubmenuUpdates.Dispose ();
				SubmenuUpdates = null;
			}

			if (OutletCurrentDetails != null) {
				OutletCurrentDetails.Dispose ();
				OutletCurrentDetails = null;
			}
		}
	}
}
