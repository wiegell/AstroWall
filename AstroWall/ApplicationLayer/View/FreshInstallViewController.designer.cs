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
	[Register ("FreshInstallViewController")]
	partial class FreshInstallViewController
	{
		[Outlet]
		AppKit.NSButton OutletAutoinstall { get; set; }

		[Outlet]
		AppKit.NSButton OutletCheckUpdatesAtStartup { get; set; }

		[Outlet]
		AppKit.NSButton OutletRunAtStartup { get; set; }

		[Action ("ActionCheckUpdatesAtLogin:")]
		partial void ActionCheckUpdatesAtLogin (Foundation.NSObject sender);

		[Action ("ActionCheckUpdatesAtStartup:")]
		partial void ActionCheckUpdatesAtStartup (Foundation.NSObject sender);

		[Action ("saveAction:")]
		partial void saveAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (OutletRunAtStartup != null) {
				OutletRunAtStartup.Dispose ();
				OutletRunAtStartup = null;
			}

			if (OutletAutoinstall != null) {
				OutletAutoinstall.Dispose ();
				OutletAutoinstall = null;
			}

			if (OutletCheckUpdatesAtStartup != null) {
				OutletCheckUpdatesAtStartup.Dispose ();
				OutletCheckUpdatesAtStartup = null;
			}
		}
	}
}
