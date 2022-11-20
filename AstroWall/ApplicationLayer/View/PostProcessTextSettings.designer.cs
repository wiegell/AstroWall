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
	[Register ("PostProcessTextSettings")]
	partial class PostProcessTextSettings
	{
		[Outlet]
		AppKit.NSButton OutletEnabled { get; set; }

		[Action ("ActionSave:")]
		partial void ActionSave (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (OutletEnabled != null) {
				OutletEnabled.Dispose ();
				OutletEnabled = null;
			}
		}
	}
}
