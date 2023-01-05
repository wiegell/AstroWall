using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using GameController;
using Newtonsoft.Json;

namespace AstroWall.ApplicationLayer.Helpers
{
    /// <summary>
    /// Struct used to represent a monitor / screen of the user.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal struct Screen
    {
        private Screen(NSScreen nSscreen)
        {
            this.Id = nSscreen.LocalizedName;

            // Always assume HDPI (x2)
            this.XRes = (int)(nSscreen.Frame.Size.Width * 2);
            this.YRes = (int)(nSscreen.Frame.Size.Height * 2);
            this.IsMainScreen = NSScreen.MainScreen == nSscreen;
        }

        /// <summary>
        /// Gets Id of screen.
        /// </summary>
        [JsonProperty]
        internal string Id { get; }

        /// <summary>
        /// Gets horizontal resolution of screen.
        /// </summary>
        [JsonProperty]
        internal int XRes { get; }

        /// <summary>
        /// Gets vertical resolution of screen.
        /// </summary>
        [JsonProperty]
        internal int YRes { get; }

        /// <summary>
        /// Gets a value indicating whether screen is main screen.
        /// </summary>
        [JsonProperty]
        internal bool IsMainScreen { get; }

        /// <summary>
        /// Gets corresponding NSScreen object.
        /// </summary>
        /// <returns>
        /// Can possibly return null, if the screen width this.Id is
        /// no longer connected.
        /// </returns>
        internal NSScreen ToNSScreen
        {
            get
            {
                var that = this;
                var filteredArray = NSScreen.Screens.Where(nsscreen => nsscreen.LocalizedName == that.Id).ToArray();
                if (filteredArray.Length != 1)
                {
                    return null;
                }
                else
                {
                    return filteredArray[0];
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether is horisontal.
        /// </summary>
        internal bool IsHorizontal => XRes > YRes;

        /// <summary>
        /// Gets a value indicating whether is vertical.
        /// </summary>
        internal bool IsVertical => YRes > XRes;

        /// <summary>
        /// Gets XRes / YRes.
        /// </summary>
        internal double Ratio => (double)XRes / (double)YRes;

        /// <summary>
        /// Creates screen objects from the currently connected screens.
        /// </summary>
        /// <returns>Dictionary with ids as key and Screen objects as values.</returns>
        internal static Dictionary<string, Screen> FromCurrentConnected()
        {
            return NSScreen.Screens.Select(nsscreen => new Screen(nsscreen)).ToDictionary(screen => screen.Id);
        }

        /// <summary>
        /// Gets the main screen.
        /// </summary>
        internal static Screen MainScreen()
        {
            return new Screen(NSScreen.MainScreen);
        }
    }
}