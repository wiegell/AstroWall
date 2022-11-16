using System;
using AppKit;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Foundation;
using GameController;

namespace AstroWall.ApplicationLayer.Helpers
{
    [JsonObject]
    public struct Screen
    {
        public static Dictionary<string, Screen> FromCurrentConnected()
        {
            return NSScreen.Screens.Select(nsscreen => new Screen(nsscreen)).ToDictionary(screen => screen.Id);
        }
        public static Screen Main()
        {
            return new Screen(NSScreen.MainScreen);
        }

        private Screen(NSScreen nSscreen)
        {
            Id = nSscreen.LocalizedName;
            xRes =  (int)nSscreen.Frame.Size.Width * 2;
            yRes = (int)nSscreen.Frame.Size.Height * 2;
            isMainScreen = NSScreen.MainScreen == nSscreen;
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

        [JsonProperty]
        public string Id;
        [JsonProperty]
        public int xRes;
        [JsonProperty]
        public int yRes;
        [JsonProperty]
        public bool isMainScreen;

        public bool isHorizontal()
        {
            return xRes > yRes;
        }
        public bool isVertical()
        {
            return yRes > xRes;
        }
        public double calcRatio()
        {
            return (double)xRes / (double)yRes;
        }
    }


}

