using System;
using AppKit;
using CoreGraphics;
using Foundation;
using static System.Net.Mime.MediaTypeNames;

namespace AstroWall
{

    public partial class PostProcessMenuItem : AppKit.NSView
    {

        private bool selected = true;
        NSTrackingArea trackingArea;
        public override bool WantsUpdateLayer => true;
        public event EventHandler<TrackingEventArgs> OnDragChange = delegate { };
        private CGRect textFieldRect;
        private NSTextField outerTextField;

        // Called when created from unmanaged code
        public PostProcessMenuItem(IntPtr handle) : base(handle)
        {
            Console.WriteLine("called unmanaged");

        }

        public PostProcessMenuItem(CGRect rect) : base(rect)
        {
            //WantsLayer = true;
            Console.WriteLine("called rect");
            LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;
            trackingArea = new NSTrackingArea(new CGRect(0, 0, rect.Width, rect.Height), NSTrackingAreaOptions.ActiveAlways | NSTrackingAreaOptions.MouseEnteredAndExited, this, null);
            AddTrackingArea(trackingArea);
        }

        public static PostProcessMenuItem StdSize(int itemnumber, string text)
        {
            string trunctext = (text.Length > 23 ? text.Substring(0, 23).TrimEnd() + "..." : text);

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
                itemContainerHeight
                );
            CGRect outerTFContainerRect = new CGRect(
                itemOuterTFX,
                itemOuterTFY,
                itemOuterTFWidth,
                itemOuterTFHeight
                );
            CGRect innerTFContainerRect = new CGRect(
                itemInnerTFX,
                itemInnerTFY,
                itemInnerTFWidth,
                itemInnerTFHeight
                );
            CGRect imageRect = new CGRect(
    0,
    0,
   itemContainerHeight,
    itemContainerHeight
    );
            //string trunctext = (text.Length > 23 ? text.Substring(0, 23).TrimEnd() + "..." : text);

            //CGRect smContainerRect = new CGRect(0, 10, 200, 40);
            //CGRect outerTFContainerRect = new CGRect(6, 10, 188, 34);
            //CGRect innerTFContainerRect = new CGRect(8, 13, 190, 32);


            PostProcessMenuItem sm = new PostProcessMenuItem(smContainerRect);

            // Settings of outer TF
            sm.outerTextField = NSTextField.CreateLabel("");
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

            //sm.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;

            return sm;
        }

        public override void MouseEntered(NSEvent theEvent)
        {
            base.MouseEntered(theEvent);
            setHoverColor();
            Console.WriteLine("mouseenter");
            OnDragChange(this, new TrackingEventArgs("Mouse Entered"));
        }

        public override void MouseExited(NSEvent theEvent)
        {
            base.MouseExited(theEvent);
            if (selected) setSelectedColor();
            else setNoColor();
            OnDragChange(this, new TrackingEventArgs("Mouse Exited"));
        }

        public override void MouseDown(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Down"));
            setClickColor();
        }

        public override void MouseUp(NSEvent theEvent)
        {
            OnDragChange(this, new TrackingEventArgs("Mouse Up"));
            setSelectedColor();
        }

        public void setClickColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SelectedContentBackground.CGColor;
        }

        public void setSelectedColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SystemGray.ColorWithAlphaComponent(new nfloat(0.4)).CGColor;
        }

        public void setHoverColor()
        {
            outerTextField.Layer.BackgroundColor = NSColor.SystemGray.ColorWithAlphaComponent(new nfloat(0.7)).CGColor;

        }
        public void setNoColor()
        {
            outerTextField.Layer.BackgroundColor = null;
        }
    }

}

