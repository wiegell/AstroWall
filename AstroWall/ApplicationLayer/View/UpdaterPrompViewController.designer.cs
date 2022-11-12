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
	[Register ("UpdaterPrompViewController")]
	partial class UpdaterPrompViewController
	{
		[Outlet]
		AppKit.NSTextField OutletDescription { get; set; }

		[Outlet]
		AppKit.NSTextField OutletVersion { get; set; }

		[Action ("ActionInstall:")]
		partial void ActionInstall (Foundation.NSObject sender);

		[Action ("ActionSkip:")]
		partial void ActionSkip (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (OutletDescription != null) {
				OutletDescription.Dispose ();
				OutletDescription = null;
			}

			if (OutletVersion != null) {
				OutletVersion.Dispose ();
				OutletVersion = null;
			}
		}
	}
}
