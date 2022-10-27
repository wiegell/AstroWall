using System;

using AppKit;
using Foundation;

namespace AstroWall
{
    public partial class ViewController : NSViewController
    {

        #region Application Access
        public static AppDelegate App
        {
            get { return (AppDelegate)NSApplication.SharedApplication.Delegate; }
        }
        #endregion

        #region Computed Properties
        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
        #endregion

        public ViewController(IntPtr handle) : base(handle)
        {
            Console.WriteLine("view constructed");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Console.WriteLine("viewloaded");
        }
        public override void ViewWillAppear()
        {
            base.ViewWillAppear();

        }

        public override void ViewWillDisappear()
        {
            base.ViewDidDisappear();

        }

    }
}
