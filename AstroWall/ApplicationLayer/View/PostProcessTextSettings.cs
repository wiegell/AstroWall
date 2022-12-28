// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using AstroWall.BusinessLayer.Preferences;

namespace AstroWall
{
    public partial class PostProcessTextSettings : NSView
    {
        Action<AddText> changePrefsCallback;
        AddText addText;

        public PostProcessTextSettings(IntPtr handle) : base(handle)
        {
        }
        public PostProcessTextSettings() : base()
        {
        }

        public void setData(AddText at)
        {
            if (at != null)
            {
                addText = at;
                OutletEnabled.State = at.isEnabled ? NSCellStateValue.On : NSCellStateValue.Off;
            }
        }

        public void regChangePrefsCallback(Action<AddText> ac)
        {
            changePrefsCallback = ac;
        }

        partial void ActionSave(NSObject sender)
        {
            if (changePrefsCallback == null)
                throw new InvalidOperationException("callback not defined");
            else if (addText == null)
                throw new InvalidOperationException("old AddText not defined");
            else
            {
                bool newStateOfEnabled = getCheckmarkBoolFromOutlet(this.OutletEnabled);
                Console.WriteLine("Registering new state from view: " + newStateOfEnabled);
                changePrefsCallback(new AddText(addText, newStateOfEnabled));
                this.Window.Close();
            }
        }

        private static bool getCheckmarkBoolFromOutlet(NSButton sender)
        {
            return sender.State == NSCellStateValue.On;
        }
    }
}
