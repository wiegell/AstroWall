using System;
using System.Collections.Generic;
using AstroWall.ApplicationLayer.Helpers;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    /// <summary>
    /// Compose different postproccess functions.
    /// </summary>
    internal partial class PostProcess
    {
        /// <summary>
        /// Compose different postproccess functions.
        /// </summary>
        /// <returns>A delegate that takes no arguments and returns postprocessed dictionary. </returns>
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f3)
        {
            return () => f3(f2(f1()));
        }

        /// <summary>
        /// Compose different postproccess functions.
        /// </summary>
        /// <returns>A delegate that takes no arguments and returns postprocessed dictionary. </returns>
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f3,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f4)
        {
            return () => f4(f3(f2(f1())));
        }

        /// <summary>
        /// Compose different postproccess functions.
        /// </summary>
        /// <returns>A delegate that takes no arguments and returns postprocessed dictionary. </returns>
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>> ComposePostProcess(
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>> f1,
            Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> f2)
        {
            return () => f2(f1());
        }
    }
}
