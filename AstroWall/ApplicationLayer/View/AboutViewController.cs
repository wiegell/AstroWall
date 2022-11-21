// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;

namespace AstroWall
{
    public partial class AboutViewController : NSViewController
    {


        public AboutViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            OutletVersion.StringValue = General.currentVersion();
        }

        partial void ActionGithub(NSObject sender)
        {
            General.Open("https://github.com/wiegell/AstroWall/");
        }
        partial void ActionWeb(NSObject sender)
        {
          General.Open("https://wiegell.github.io/AstroWall/");
        }
        partial void ActionIssues(NSObject sender)
        {
            General.Open("https://github.com/wiegell/AstroWall/issues");
        }


    }
}