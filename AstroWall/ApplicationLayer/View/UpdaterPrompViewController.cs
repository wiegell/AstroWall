// This file has been autogenerated from a class added in the UI designer.

using System;
using AppKit;
using Foundation;

namespace AstroWall
{
    /// <summary>
    /// Update view. Wrongly named view controller, not renamed since cumbersome
    /// to rename in xcode.
    /// </summary>
    public partial class UpdaterPrompViewController : NSView
    {
        private Action<BusinessLayer.UpdatePromptResponse> choiceCallback;
        private string ver;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterPrompViewController"/> class.
        /// </summary>
        /// <param name="handle"></param>
        public UpdaterPrompViewController(IntPtr handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Updates GUI with data about the release.
        /// </summary>
        /// <param name="rel"></param>
        public void SetRelease(UpdateLibrary.Release rel)
        {
            OutletDescription.StringValue = rel.Description;
            OutletVersion.StringValue = rel.version;
            ver = rel.version;
        }

        /// <summary>
        /// Registers callback action to be called when user makes a choice
        /// to update or not.
        /// </summary>
        /// <param name="callback"></param>
        internal void RegChoiceCallback(Action<BusinessLayer.UpdatePromptResponse> callback)
        {
            choiceCallback = callback;
        }

        /// <summary>
        /// GUI handler for when user accepts to install.
        /// </summary>
        /// <param name="sender"></param>
        partial void ActionInstall(NSObject sender)
        {
            var callRet = new BusinessLayer.UpdatePromptResponse()
            {
                AcceptOrSkipUpdate = true,
            };
            choiceCallback(callRet);
            this.Window.Close();
        }

        /// <summary>
        /// GUI handler for when the user chooses to skip install.
        /// </summary>
        /// <param name="sender"></param>
        partial void ActionSkip(NSObject sender)
        {
            var callRet = new BusinessLayer.UpdatePromptResponse()
            {
                AcceptOrSkipUpdate = false,
                SkippedVersion = ver,
            };
            choiceCallback(callRet);
            this.Window.Close();
        }
    }
}
