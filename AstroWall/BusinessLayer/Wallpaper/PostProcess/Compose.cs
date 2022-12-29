using System;
using AstroWall.ApplicationLayer.Helpers;
using System.Collections.Generic;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    internal partial class PostProcess
    {
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f3)
        {
            return () => f3(f2(f1()));
        }
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f3,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f4)
        {
            return () => f4(f3(f2(f1())));
        }

        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2)
        {
            return () => f2(f1());
        }
    }
}

