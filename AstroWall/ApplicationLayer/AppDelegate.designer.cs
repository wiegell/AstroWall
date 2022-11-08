// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AstroWall
{
	partial class AppDelegate
	{
		[Outlet]
		AppKit.NSMenuItem MenuOutletAutoInstallUpdates { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletCheckUpdatesAtLogin { get; set; }

		[Outlet]
		AppKit.NSMenuItem MenuOutletInstallUpdatesSilently { get; set; }

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

		[Action ("MenuManualCheckPic:")]
		partial void MenuManualCheckPic (Foundation.NSObject sender);

		[Action ("MenuOutletCheckPicOnLogin:")]
		partial void MenuOutletCheckPicOnLogin (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (MenuTitle != null) {
				MenuTitle.Dispose ();
				MenuTitle = null;
			}

			if (StatusMenu != null) {
				StatusMenu.Dispose ();
				StatusMenu = null;
			}

			if (MenuOutletCheckUpdatesAtLogin != null) {
				MenuOutletCheckUpdatesAtLogin.Dispose ();
				MenuOutletCheckUpdatesAtLogin = null;
			}

			if (MenuOutletAutoInstallUpdates != null) {
				MenuOutletAutoInstallUpdates.Dispose ();
				MenuOutletAutoInstallUpdates = null;
			}

			if (MenuOutletInstallUpdatesSilently != null) {
				MenuOutletInstallUpdatesSilently.Dispose ();
				MenuOutletInstallUpdatesSilently = null;
			}
		}
	}
}
