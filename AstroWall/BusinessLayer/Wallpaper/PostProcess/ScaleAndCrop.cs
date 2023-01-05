using System;
using System.Collections.Generic;
using System.Drawing;
using AstroWall.ApplicationLayer.Helpers;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    internal partial class PostProcess
    {

        public static Func<Dictionary<Screen, SKBitmap>, Dictionary<Screen, SKBitmap>> ScaleAndCropCurry()
        {
            // Not really needed atm, might be later
            return (Dictionary<Screen, SKBitmap> dic) =>
            {
                return ScaleAndCrop(dic);
            };
        }

        public static Dictionary<Screen, SKBitmap> ScaleAndCrop(Dictionary<Screen, SKBitmap> dic)
        {
            var returnDic = new Dictionary<Screen, SKBitmap>();
            foreach (var KV in dic)
            {
                Screen screen = KV.Key;
                SKBitmap inputBitmap = KV.Value;

                SKImageInfo inputInfo = inputBitmap.Info;
                double resizefactor;
                //if (screen.isHorizontal())
                //{
                if (ratioFromInfo(inputInfo) > screen.Ratio)
                {
                    // Image has a wider aspect than screen
                    // Resize to match heights
                    // (width overflow will be cropped soon)
                    resizefactor = ((double)screen.YRes) / (double)inputInfo.Height;
                }
                else
                {
                    // Image has a taller aspect than screen
                    // Resize to match widths
                    // (height overflow will be cropped soon)
                    resizefactor = ((double)screen.XRes) / (double)inputInfo.Width;
                }

                // Resize
                SKImageInfo newInfo = new SKImageInfo(
                    ((int)Math.Ceiling(inputInfo.Width * resizefactor)),
                    ((int)Math.Ceiling(inputInfo.Height * resizefactor))
                    );
                log($"Resizing postprocess of screen {screen.Id} to size {newInfo.Width}x{newInfo.Height}");
                SKBitmap newBitmap = inputBitmap.Resize(newInfo, SKFilterQuality.High);

                // Crop
                var image = SKImage.FromBitmap(newBitmap);
                SKImage croppedImage;
                if (ratioFromInfo(inputInfo) > screen.Ratio)
                {
                    // Image has a wider aspect than screen
                    // Get the width diff
                    int widthDiff = newBitmap.Info.Width - screen.XRes;
                    int xMargin = widthDiff / 2;
                    var rect = SKRectI.Create(xMargin, 0, screen.XRes, screen.YRes);
                    croppedImage = image.Subset(rect);
                }
                else
                {
                    // Image is too tall, get the diff
                    int heightDiff = newBitmap.Info.Height - screen.YRes;
                    int yMargin = heightDiff / 2;
                    var rect = SKRectI.Create(0, yMargin, screen.XRes, screen.YRes);
                    //var rect = SKRectI.Create(0, yMargin, screen.xRes, screen.yRes);
                    croppedImage = image.Subset(rect);
                }

                // Return
                var returnBM = SKBitmap.FromImage(croppedImage);
                log($"Cropped postprocess of screen {screen.Id} to {croppedImage.Info.Width}x{croppedImage.Info.Height}");

                returnDic.Add(screen, returnBM);
            }
            return returnDic;
        }

        private static double ratioFromInfo(SKImageInfo info)
        {
            return (double)info.Width / (double)info.Height;
        }
    }
}

