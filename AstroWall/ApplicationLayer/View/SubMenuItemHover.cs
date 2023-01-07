using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using AppKit;
using CoreGraphics;
using Foundation;
using SpriteKit;

namespace AstroWall
{
    /// <summary>
    /// Submenu item, that supports tracking events.
    /// </summary>
    public partial class SubMenuItemHover : AppKit.NSView
    {
        private NSTrackingArea trackingArea;
        private NSTextField outerTextField;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubMenuItemHover"/> class.
        /// XCode default constructor.
        /// </summary>
        /// <param name="handle"></param>
        public SubMenuItemHover(IntPtr handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubMenuItemHover"/> class.
        /// Constructor used when constructed from code. Takes a rect that is used to define tracking area.
        /// </summary>
        /// <param name="rect"></param>
        public SubMenuItemHover(CGRect rect)
            : base(rect)
        {
            LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            trackingArea = new NSTrackingArea(rect, NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseEnteredAndExited, this, null);
            AddTrackingArea(trackingArea);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubMenuItemHover"/> class.
        /// TODO needed???.
        /// Called when created directly from a XIB file.
        /// </summary>
        /// <param name="coder"></param>
        [Export("initWithCoder:")]
        public SubMenuItemHover(NSCoder coder)
            : base(coder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubMenuItemHover"/> class.
        /// XIP constructor.
        /// TODO needed???.
        /// </summary>
        public SubMenuItemHover()
            : base()
        {
        }

        /// <summary>
        /// Drag change event handler.
        /// </summary>
        internal event EventHandler<TrackingEventArgs> OnDragChange = (sender, e) => { };

        /// <summary>
        /// Gets a value indicating whether the view needs update layer.
        /// Override neccesity for mouse tracking.
        /// </summary>
        public override bool WantsUpdateLayer => true;

        /// <summary>
        /// Main way to construct this object from code.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>SubMenuItemHover instance.</returns>
        public static SubMenuItemHover StdSize(string text)
        {
            string trunctext = text.Length > 23 ? text.Substring(0, 23).TrimEnd() + "..." : text;

            CGRect smContainerRect = new CGRect(0, 0, 200, 22);
            CGRect outerTFContainerRect = new CGRect(6, 0, 188, 22);
            CGRect innerTFContainerRect = new CGRect(8, 3, 190, 17);
            SubMenuItemHover sm = new SubMenuItemHover(smContainerRect);

            sm.outerTextField = NSTextField.CreateLabel(string.Empty);
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

            return sm;
        }

        /// <summary>
        /// Handles mouse enter event.
        /// </summary>
        public override void MouseEntered(NSEvent theEvent)
        {
            base.MouseEntered(theEvent);
            EnableBGSelectionColor();
            OnDragChange(this, new TrackingEventArgs("Mouse Entered"));
        }

        /// <summary>
        /// Handles mouse exit event.
        /// </summary>
        public override void MouseExited(NSEvent theEvent)
        {
            base.MouseExited(theEvent);
            DisableBGSelectionColor();
            OnDragChange(this, new TrackingEventArgs("Mouse Exited"));
        }

        /// <summary>
        /// Handles mouse move event.
        /// TODO needed?.
        /// </summary>
        public override void MouseMoved(NSEvent theEvent)
        {
            base.MouseMoved(theEvent);
            OnDragChange(this, new TrackingEventArgs("Mouse Moved"));
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        public override void MouseDown(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Down"));
        }

        /// <summary>
        /// Enables selections color.
        /// </summary>
        public void EnableBGSelectionColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SelectedContentBackground.CGColor;
        }

        /// <summary>
        /// Disables selection color.
        /// </summary>
        public void DisableBGSelectionColor()
        {
            outerTextField.Layer.BackgroundColor = null;
        }
    }
}
