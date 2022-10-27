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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSMenu StatusMenu { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (StatusMenu != null) {
				StatusMenu.Dispose ();
				StatusMenu = null;
			}
		}
	}
}
