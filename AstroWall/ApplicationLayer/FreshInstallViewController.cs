// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using System.Threading.Tasks;

namespace AstroWall
{
    public partial class FreshInstallViewController : NSView
    {
        private Func<BusinessLayer.Preferences, Task> callback;

        public FreshInstallViewController(IntPtr handle) : base(handle)
        {
        }

        partial void saveAction(Foundation.NSObject sender)
        {
            this.Window.Close();
            if (callback == null) throw new Exception("Callback not registered");
            callback(createPrefs());
        }

        public void regSaveCallback(Func<BusinessLayer.Preferences, Task> callbackArg)
        {
            this.callback = callbackArg;
        }

        private BusinessLayer.Preferences createPrefs()
        {
            return new BusinessLayer.Preferences()
            {
                autoInstallUpdates = this.autoinstall.State != 0,
                checkUpdatesOnLogin = this.checkupdatesatlogin.State != 0,
                runAtLogin = this.runatlogin.State != 0
            };
        }

        public void runCallback()
        {
            if (callback == null) throw new Exception("Callback not registered");
            callback(createPrefs());
        }
    }
}
