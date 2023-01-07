// This file has been autogenerated from a class added in the UI designer.

using System;
using AppKit;
using Foundation;

namespace AstroWall
{
    /// <summary>
    /// View of post process sidebar. Wrongly named "viewcontroller".
    /// Not changed, since cumbersome in XCODE.
    /// </summary>
    public partial class PostProcessSidebarViewController : NSView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessSidebarViewController"/> class.
        /// </summary>
        /// <param name="handle"></param>
        public PostProcessSidebarViewController(IntPtr handle)
            : base(handle)
        {
            PostProcessMenuItem item = PostProcessMenuItem.StdSize(1, "Add text");
            item.SetSelectedColor();
            this.AddSubview(item);
        }
    }
}
