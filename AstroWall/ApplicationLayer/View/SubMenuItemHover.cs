using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using CoreGraphics;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using SpriteKit;

namespace AstroWall
{
    public partial class SubMenuItemHover : AppKit.NSView
    {
        #region Constructors
        NSTrackingArea trackingArea;
        public override bool WantsUpdateLayer => true;
        public event EventHandler<TrackingEventArgs> OnDragChange = delegate { };
        private NSTextField outerTextField;


        // Called when created from unmanaged code
        public SubMenuItemHover(IntPtr handle) : base(handle)
        {

        }

        public SubMenuItemHover(CGRect rect) : base(rect)
        {

            //WantsLayer = true;

            LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            trackingArea = new NSTrackingArea(rect, NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseEnteredAndExited, this, null);
            AddTrackingArea(trackingArea);

        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public SubMenuItemHover(NSCoder coder) : base(coder)
        {

        }

        // XIP
        public SubMenuItemHover() : base()
        {

        }

        public static SubMenuItemHover StdSize(string text)
        {
            string trunctext = (text.Length > 23 ? text.Substring(0, 23).TrimEnd() + "..." : text);

            CGRect smContainerRect = new CGRect(0, 0, 200, 22);
            CGRect outerTFContainerRect = new CGRect(6, 0, 188, 22);
            CGRect innerTFContainerRect = new CGRect(8, 3, 190, 17);
            SubMenuItemHover sm = new SubMenuItemHover(smContainerRect);


            sm.outerTextField = NSTextField.CreateLabel("");
            sm.outerTextField.AutoresizingMask = NSViewResizingMask.NotSizable;
            sm.outerTextField.Frame = outerTFContainerRect;
            sm.outerTextField.WantsLayer = true;
            sm.outerTextField.Layer.CornerRadius = 3;

            NSTextField innerTextField = NSTextField.CreateLabel(trunctext);
            innerTextField.AutoresizingMask = NSViewResizingMask.NotSizable;
            innerTextField.Frame = innerTFContainerRect;


            Console.WriteLine("layer: " + innerTextField.Layer == null);
            sm.AddSubview(sm.outerTextField);
            sm.outerTextField.AddSubview(innerTextField);

            //sm.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;

            return sm;
        }

        public override void MouseEntered(NSEvent theEvent)
        {
            base.MouseEntered(theEvent);
            EnableBGSelectionColor();
            OnDragChange(this, new TrackingEventArgs("Mouse Entered"));
        }

        public override void MouseExited(NSEvent theEvent)
        {
            base.MouseExited(theEvent);
            DisableBGSelectionColor();
            OnDragChange(this, new TrackingEventArgs("Mouse Exited"));
        }

        public override void MouseMoved(NSEvent theEvent)
        {
            base.MouseMoved(theEvent);
            OnDragChange(this, new TrackingEventArgs("Mouse Moved"));
        }

        public override void MouseDown(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Down"));
        }

        public void EnableBGSelectionColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SelectedContentBackground.CGColor;
        }
        public void DisableBGSelectionColor()
        {
            outerTextField.Layer.BackgroundColor = null;
        }

        #endregion
    }

    public class TrackingEventArgs : EventArgs
    {
        public TrackingEventArgs(string description) { Description = description; }

        public string Description { get; set; }
    }
}
