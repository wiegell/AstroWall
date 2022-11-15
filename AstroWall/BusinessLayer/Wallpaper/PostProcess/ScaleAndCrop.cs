using System;
using System.Collections.Generic;
using AstroWall.ApplicationLayer.Helpers;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    partial class PostProcess

    {
        public Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> ScaleAndCropCurry()
        {
            // Not really needed atm, might be later
            return (Dictionary<Screen, SkiaSharp.SKBitmap> dic) =>
            {
                return ScaleAndCrop(dic);
            };
        }

        public static Dictionary<Screen, SkiaSharp.SKBitmap> ScaleAndCrop(Dictionary<Screen, SkiaSharp.SKBitmap> dic)
        {
            return new Dictionary<Screen, SkiaSharp.SKBitmap>();
        }
    }
}

