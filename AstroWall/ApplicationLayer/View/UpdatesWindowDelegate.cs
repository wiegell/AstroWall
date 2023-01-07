using System;
using AppKit;
using Foundation;

namespace AstroWall
{
    /// <summary>
    /// Updates window delegate. Main purpose of this class is to enable to handle,
    /// if the user closes the window without making a choice.
    /// </summary>
    public class UpdatesWindowDelegate : NSWindowDelegate
    {
        private NSWindow window;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatesWindowDelegate"/> class.
        /// </summary>
        /// <param name="window"></param>
        public UpdatesWindowDelegate(NSWindow window)
        {
            // Initialize with supplied window.
            this.window = window;
        }

        /// <summary>
        /// Handle user close window. Runs the supplied callback before window close.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>Always returns true.</returns>
        /// <exception cref="FormatException">Throws if the correct window is not attached.</exception>
        public override bool WindowShouldClose(NSObject sender)
        {
            var view = (FreshInstallViewController)this.window.ContentViewController.View;
            if (view.GetType() != typeof(FreshInstallViewController))
            {
                throw new FormatException("Custom viewcontroller not attached");
            }

            view.RunCallback();
            return true;
        }
    }
}