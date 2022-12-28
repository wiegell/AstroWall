using System;
using AppKit;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Foundation;
using GameController;

namespace AstroWall.ApplicationLayer.Helpers
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Screen
    {
        // Properties
        [JsonProperty]
        internal string Id { get; }
        [JsonProperty]
        internal int xRes { get; }
        [JsonProperty]
        internal int yRes { get; }
        [JsonProperty]
        internal bool isMainScreen { get; }

        // Constructor
        private Screen(NSScreen nSscreen)
        {
            Id = nSscreen.LocalizedName;
            // Always assume HDPI (x2)
            xRes = (int)(nSscreen.Frame.Size.Width * 2);
            yRes = (int)(nSscreen.Frame.Size.Height * 2);
            isMainScreen = NSScreen.MainScreen == nSscreen;
        }

        // Methods
        internal static Dictionary<string, Screen> FromCurrentConnected()
        {
            return NSScreen.Screens.Select(nsscreen => new Screen(nsscreen)).ToDictionary(screen => screen.Id);
        }
        internal static Screen MainScreen()
        {
            return new Screen(NSScreen.MainScreen);
        }

        /// <summary>
        /// Can possibly return null, if the screen width this.Id is
        /// no longer connected
        /// </summary>
        /// <returns></returns>
        public NSScreen toNSScreen()
        {
            var that = this;
            var filteredArray = NSScreen.Screens.Where(nsscreen => nsscreen.LocalizedName == that.Id).ToArray();
            if (filteredArray.Length != 1) return null;
            else return filteredArray[0];
        }

        internal bool isHorizontal()
        {
            return xRes > yRes;
        }
        internal bool isVertical()
        {
            return yRes > xRes;
        }
        internal double calcRatio()
        {
            return (double)xRes / (double)yRes;
        }
    }


}

