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
		AppKit.NSMenu StatusMenu { get; set; }

		[Action ("ManualCheckForNewPic:")]
		partial void ManualCheckForNewPic (Foundation.NSObject sender);

		[Action ("MenuActionCheckPicOnLogin:")]
		partial void MenuActionCheckPicOnLogin (Foundation.NSObject sender);

		[Action ("MenuManualCheckPic:")]
		partial void MenuManualCheckPic (Foundation.NSObject sender);

		[Action ("MenuOutletCheckPicOnLogin:")]
		partial void MenuOutletCheckPicOnLogin (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (StatusMenu != null) {
				StatusMenu.Dispose ();
				StatusMenu = null;
			}
		}
	}
}
