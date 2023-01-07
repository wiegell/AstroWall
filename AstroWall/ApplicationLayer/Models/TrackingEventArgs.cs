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
    /// Event with embedded description.
    /// </summary>
    internal class TrackingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingEventArgs"/> class.
        /// </summary>
        /// <param name="description"></param>
        public TrackingEventArgs(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets description.
        /// </summary>
        public string Description { get; }
    }
}
