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
		AppKit.NSMenuItem MenuOutletCheckUpdatesAtLogin { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletCheckUpdatesManual { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletInstallUpdatesSilently { get; set; }

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
		AppKit.NSMenu StatusMenu { get; set; }

		[Action ("ManualCheckForNewPic:")]
		partial void ManualCheckForNewPic (Foundation.NSObject sender);

		[Action ("MenuActionAutoInstallUpdates:")]
		partial void MenuActionAutoInstallUpdates (Foundation.NSObject sender);

		[Action ("MenuActionCheckPicOnLogin:")]
		partial void MenuActionCheckPicOnLogin (Foundation.NSObject sender);

		[Action ("MenuActionCheckUpdatesAtLogin:")]
		partial void MenuActionCheckUpdatesAtLogin (Foundation.NSObject sender);

		[Action ("MenuActionInstallUpdatesSilently:")]
		partial void MenuActionInstallUpdatesSilently (Foundation.NSObject sender);

		[Action ("MenuActionManualCheckUpdates:")]
		partial void MenuActionManualCheckUpdates (Foundation.NSObject sender);

		[Action ("MenuActionRunAtLogin:")]
		partial void MenuActionRunAtLogin (Foundation.NSObject sender);

		[Action ("MenuManualCheckPic:")]
		partial void MenuManualCheckPic (Foundation.NSObject sender);

		[Action ("MenuOutletCheckPicOnLogin:")]
		partial void MenuOutletCheckPicOnLogin (Foundation.NSObject sender);
		
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

			if (MenuOutletCheckUpdatesAtLogin != null) {
				MenuOutletCheckUpdatesAtLogin.Dispose ();
				MenuOutletCheckUpdatesAtLogin = null;
			}

			if (MenuOutletInstallUpdatesSilently != null) {
				MenuOutletInstallUpdatesSilently.Dispose ();
				MenuOutletInstallUpdatesSilently = null;
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

			if (MenuOutletCheckUpdatesManual != null) {
				MenuOutletCheckUpdatesManual.Dispose ();
				MenuOutletCheckUpdatesManual = null;
			}

			if (MenuTitle != null) {
				MenuTitle.Dispose ();
				MenuTitle = null;
			}

			if (StatusMenu != null) {
				StatusMenu.Dispose ();
				StatusMenu = null;
			}
		}
	}
}
