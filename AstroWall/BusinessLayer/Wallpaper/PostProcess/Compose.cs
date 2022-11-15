using System;
using AstroWall.ApplicationLayer.Helpers;
using System.Collections.Generic;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    partial class PostProcess
    {
        public static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f3)
        {
            return () => f3(f2(f1()));
        }
        public static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
    Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
    Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2)
        {
            return () => f2(f1());
        }
    }
}

