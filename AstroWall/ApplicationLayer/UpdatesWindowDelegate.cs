using System;
using AppKit;
using Foundation;

namespace AstroWall
{
    public class UpdatesWindowDelegate : NSWindowDelegate
    {
        #region Computed Properties
        public NSWindow Window { get; set; }
        #endregion

        public UpdatesWindowDelegate(NSWindow window)
        {
            // Initialize
            this.Window = window;
        }

        public override bool WindowShouldClose(NSObject sender)
        {
            var view = (FreshInstallViewController)this.Window.ContentViewController.View;
            if (view.GetType() != typeof(FreshInstallViewController))
            {
                throw new Exception("Custom viewcontroller not attached");
            }
            view.runCallback();
            return true;
        }
    }
}

