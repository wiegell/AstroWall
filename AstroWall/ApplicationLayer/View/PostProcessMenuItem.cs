using System;
using AppKit;
using CoreGraphics;
using Foundation;
using static System.Net.Mime.MediaTypeNames;

namespace AstroWall
{
    /// <summary>
    /// View of post process menu item.
    /// </summary>
    public partial class PostProcessMenuItem : AppKit.NSView
    {
        private bool selected = true;
        private NSTrackingArea trackingArea;
        private NSTextField outerTextField;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessMenuItem"/> class.
        /// XCode default constructor.
        /// </summary>
        /// <param name="handle"></param>
        public PostProcessMenuItem(IntPtr handle)
            : base(handle)
        {
            Console.WriteLine("called unmanaged");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessMenuItem"/> class.
        /// Constructor used when constructed from code. Takes a rect that is used to define tracking area.
        /// </summary>
        /// <param name="rect"></param>
        public PostProcessMenuItem(CGRect rect)
            : base(rect)
        {
            Console.WriteLine("called rect");
            LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            trackingArea = new NSTrackingArea(new CGRect(0, 0, rect.Width, rect.Height), NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseEnteredAndExited, this, null);
            AddTrackingArea(trackingArea);
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
        /// Constructs the correct rect via the itemnumber.
        /// </summary>
        /// <param name="itemnumber">Placement in line of submenuitems.</param>
        /// <param name="text">Text to be displayed.</param>
        /// <returns>PostProcessMenuItem instance.</returns>
        public static PostProcessMenuItem StdSize(int itemnumber, string text)
        {
            string trunctext = text.Length > 23 ? text.Substring(0, 23).TrimEnd() + "..." : text;

            // Position vars
            int itemContainerHeight = 40;
            int botToSafeTop = 405;
            int itemContainerX = 0;
            int itemContainerY = botToSafeTop - (itemContainerHeight * itemnumber);
            int itemContainerWidth = 191;
            int itemOuterTFHeight = itemContainerHeight;
            int itemOuterTFX = 10;
            int itemOuterTFY = 0;
            int itemOuterTFWidth = itemContainerWidth - 20;
            int itemInnerTFHeight = itemOuterTFHeight - 10;
            int itemInnerTFX = 40;
            int itemInnerTFY = 10;
            int itemInnerTFWidth = itemContainerWidth - 10;

            CGRect smContainerRect = new CGRect(
                itemContainerX,
                itemContainerY,
                itemContainerWidth,
                itemContainerHeight);
            CGRect outerTFContainerRect = new CGRect(
                itemOuterTFX,
                itemOuterTFY,
                itemOuterTFWidth,
                itemOuterTFHeight);
            CGRect innerTFContainerRect = new CGRect(
                itemInnerTFX,
                itemInnerTFY,
                itemInnerTFWidth,
                itemInnerTFHeight);
            CGRect imageRect = new CGRect(
    0,
    0,
    itemContainerHeight,
    itemContainerHeight);

            PostProcessMenuItem sm = new PostProcessMenuItem(smContainerRect);

            // Settings of outer TF
            sm.outerTextField = NSTextField.CreateLabel(string.Empty);
            sm.outerTextField.AutoresizingMask = NSViewResizingMask.NotSizable;
            sm.outerTextField.Frame = outerTFContainerRect;
            sm.outerTextField.WantsLayer = true;
            sm.outerTextField.Layer.CornerRadius = 5;

            // Settings of inner TF
            NSTextField innerTextField = NSTextField.CreateLabel(trunctext);
            innerTextField.Font = NSFont.SystemFontOfSize(15);
            innerTextField.AutoresizingMask = NSViewResizingMask.NotSizable;
            innerTextField.Frame = innerTFContainerRect;

            // Settings for image
            NSImageView imageView = NSImageView.FromImage(NSImage.GetSystemSymbol("textformat.abc.dottedunderline", null));
            imageView.Frame = imageRect;

            // combine views
            sm.AddSubview(sm.outerTextField);
            sm.outerTextField.AddSubview(imageView);
            sm.outerTextField.AddSubview(innerTextField);

            return sm;
        }

        /// <summary>
        /// Handles mouse enter event.
        /// </summary>
        public override void MouseEntered(NSEvent theEvent)
        {
            base.MouseEntered(theEvent);
            SetHoverColor();
            Console.WriteLine("mouseenter");
            OnDragChange(this, new TrackingEventArgs("Mouse Entered"));
        }

        /// <summary>
        /// Handles mouse exit event.
        /// </summary>
        public override void MouseExited(NSEvent theEvent)
        {
            base.MouseExited(theEvent);
            if (selected)
            {
                SetSelectedColor();
            }
            else
            {
                SetNoColor();
            }

            OnDragChange(this, new TrackingEventArgs("Mouse Exited"));
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        public override void MouseDown(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Down"));
            SetClickColor();
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        public override void MouseUp(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Up"));
            SetSelectedColor();
        }

        /// <summary>
        /// Sets selected color of item.
        /// </summary>
        internal void SetSelectedColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SystemGray.ColorWithAlphaComponent(new nfloat(0.4)).CGColor;
        }

        private void SetClickColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SelectedContentBackground.CGColor;
        }

        private void SetHoverColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SystemGray.ColorWithAlphaComponent(new nfloat(0.7)).CGColor;
        }

        private void SetNoColor()
        {
            outerTextField.Layer.BackgroundColor = null;
        }
    }
}