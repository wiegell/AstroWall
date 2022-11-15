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
	[Register ("AboutViewController")]
	partial class AboutViewController
	{
		[Outlet]
		AppKit.NSTextField OutletVersion { get; set; }

		[Action ("ActionGithub:")]
		partial void ActionGithub (Foundation.NSObject sender);

		[Action ("ActionIssues:")]
		partial void ActionIssues (Foundation.NSObject sender);

		[Action ("ActionWeb:")]
		partial void ActionWeb (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (OutletVersion != null) {
				OutletVersion.Dispose ();
				OutletVersion = null;
			}
		}
	}
}
