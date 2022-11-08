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
		AppKit.NSButton autoinstall { get; set; }

		[Outlet]
		AppKit.NSButton checkupdatesatlogin { get; set; }

		[Outlet]
		AppKit.NSButton runatlogin { get; set; }

		[Outlet]
		AppKit.NSButton silentinstall { get; set; }

		[Action ("saveAction:")]
		partial void saveAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (silentinstall != null) {
				silentinstall.Dispose ();
				silentinstall = null;
			}

			if (autoinstall != null) {
				autoinstall.Dispose ();
				autoinstall = null;
			}

			if (checkupdatesatlogin != null) {
				checkupdatesatlogin.Dispose ();
				checkupdatesatlogin = null;
			}

			if (runatlogin != null) {
				runatlogin.Dispose ();
				runatlogin = null;
			}
		}
	}
}
