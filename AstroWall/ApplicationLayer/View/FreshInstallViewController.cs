// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Threading.Tasks;
using AppKit;
using AstroWall.BusinessLayer.Preferences;
using Foundation;

namespace AstroWall
{
    /// <summary>
    /// View controller for modal shown after fresh install, prompting the
    /// user to select prefs.
    /// </summary>
    public partial class FreshInstallViewController : NSView
    {
        private Func<Preferences, Task> callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreshInstallViewController"/> class.
        /// </summary>
        /// <param name="handle"></param>
        internal FreshInstallViewController(IntPtr handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Registers save callback.
        /// </summary>
        /// <param name="callbackArg">Delegate that takes a preference o</param>
        internal void RegSaveCallback(Func<Preferences, Task> callbackArg)
        {
            this.callback = callbackArg;
        }

        /// <summary>
        /// To be used if install location is not correct.
        /// Disables updates options in the UI.
        /// </summary>
        internal void DisableUpdatesOptions()
        {
            this.OutletAutoinstall.Enabled = false;
            this.OutletCheckUpdatesAtStartup.Enabled = false;
            this.OutletAutoinstall.State = NSCellStateValue.Off;
            this.OutletCheckUpdatesAtStartup.State = NSCellStateValue.Off;
        }

        /// <summary>
        /// Creates prefs from currently selected items in UI and calls the callback
        /// with that instance as argument. Meant to be called on window close by user.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if callback not registered.</exception>
        internal void RunCallback()
        {
            if (callback == null)
            {
                throw new InvalidOperationException("Callback not registered");
            }

            callback(CreatePrefs());
        }

        /// <summary>
        /// Creates prefs from selected checkmarks.
        /// </summary>
        /// <returns>New preference instance.</returns>
        private Preferences CreatePrefs()
        {
            var prefs = new Preferences();
            prefs.AutoInstallUpdates = this.OutletAutoinstall.State == NSCellStateValue.On;
            prefs.
            CheckUpdatesOnStartup = this.OutletCheckUpdatesAtStartup.State == NSCellStateValue.On;
            prefs.RunAtStartup = this.OutletRunAtStartup.State == NSCellStateValue.On;

            return prefs;
        }


        /// <summary>
        /// Creates preference instance from the currently checked items in the window, then
        /// supplies that to registered callback.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if callback is not registered.</exception>
        /// Suppressed warning, sinced named in XCode.
#pragma warning disable SA1300 // Element should begin with upper-case letter
        partial void saveAction(Foundation.NSObject sender)
#pragma warning restore SA1300 // Element should begin with upper-case letter
        {
            this.Window.Close();
            if (callback == null)
            {
                throw new InvalidOperationException("Callback not registered");
            }

            var prefs = CreatePrefs();
            callback(prefs);
        }

        /// <summary>
        /// If updates are not checked at startup autoinstall cannot be performed,
        /// therefore is disabled in gui.
        /// </summary>
        partial void ActionCheckUpdatesAtStartup(NSObject sender)
        {
            if (OutletCheckUpdatesAtStartup.State == NSCellStateValue.Off)
            {
                OutletAutoinstall.State = NSCellStateValue.Off;
                OutletAutoinstall.Enabled = false;
            }
            else
            {
                OutletAutoinstall.State = NSCellStateValue.On;
                OutletAutoinstall.Enabled = true;
            }
        }
    }
}
