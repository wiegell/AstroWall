using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AstroWall.ApplicationLayer.Helpers;
using SkiaSharp;

namespace AstroWall.BusinessLayer.Wallpaper
{
    partial class PostProcess
    {
        public PostProcess()
        {
        }

        public static Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> AddTextCurry(Preferences.AddText options, string title, string description)
        {
            return (Dictionary<Screen, SkiaSharp.SKBitmap> dic) =>
            {
                return AddText(dic, options, title, description);
            };
        }

        public static Dictionary<Screen, SkiaSharp.SKBitmap> AddText(Dictionary<Screen, SkiaSharp.SKBitmap> dic, Preferences.AddText options, string title, string description)
        {
            SKBitmap mainScreenBitmap;
            Screen mainScreen;
            try
            {
                mainScreen = dic.Where(screen => screen.Key.isMainScreen).ToArray()[0].Key;
                mainScreenBitmap = dic[mainScreen];
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get main screen from input dict", ex);
            }
            if (mainScreenBitmap == null)
            {
                throw new Exception("Could not get main screen from input dict");
            }

            SKBitmap returnBitmap;
            Console.WriteLine("Running AddText");
            try
            {
                returnBitmap = mainScreenBitmap.Copy();
            }
            catch (Exception ex)
            {
                var exx = ex;
                Console.WriteLine("Problem copying bitmap");
                throw ex;
            }

            // Format description
            string descriptionFormatted = description.Replace("\n", " ").Replace("Explanation:", "").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").TrimStart();

            //string desc = "test \n test\n testtesttest";
            Console.WriteLine("desc: " + descriptionFormatted);
            var canvas = new SKCanvas(returnBitmap);
            canvas.DrawBitmap(mainScreenBitmap, 0, 0);
            canvas.ResetMatrix();

            // Paint Title
            PaintToRect(canvas, 1000, 250, 120, 20, 40, false, title
                );
            // Paint description
            PaintToRect(canvas, 1000, 250, 200, 20, 25, true, descriptionFormatted
                );
            canvas.Flush();
            canvas.Dispose();

            // Create shallow dict copy to return
            var returnDic = dic.ToDictionary(x => x.Key, x => x.Value);

            // Replace main screen with new bitmap
            returnDic[mainScreen] = returnBitmap;

            return returnDic;
        }
        private static void PaintToRect(SKCanvas canvas, int width, int x, int y, int margin, int size, bool italic, string text)
        {
            SKTypeface type = SKTypeface.FromFamilyName("Helvetica Neue", SKFontStyleWeight.Light, SKFontStyleWidth.Normal, italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

            using (var paint = new SKPaint()
            {
                Color = SKColors.Black.WithAlpha((byte)170),
                Style = SKPaintStyle.Fill,
                TextSize = size,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Typeface = type
            })
            {
                var tmpRect = SKRect.Create(x, y, width, 3000);
                int height = DrawText(canvas, text, tmpRect, paint, margin, true);
                Console.WriteLine("Calculated rect height: " + height);
                var properRect = SKRect.Create(x, y, width, height);

                // Background
                canvas.DrawRect(properRect, paint);

                paint.Color = SKColors.White.WithAlpha((byte)150);
                DrawText(canvas, text, properRect, paint, margin);
            }
        }

        public class Line
        {
            public string Value { get; set; }

            public float Width { get; set; }
        }

        /// <summary>
        /// Returns needed rect height
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="text"></param>
        /// <param name="rect"></param>
        /// <param name="paint"></param>
        /// <returns></returns>
        private static int DrawText(SKCanvas canvas, string text, SKRect rect, SKPaint paint, int margin, bool dryRun = false)
        {
            float spaceWidth = paint.MeasureText(" ");
            float wordX = rect.Left + margin;
            float wordY = rect.Top + paint.TextSize + margin;
            foreach (string word in text.Split(' '))
            {
                float wordWidth = paint.MeasureText(word);

                if (wordWidth <= rect.Right - wordX - margin && word != "\n")
                {
                    if (!dryRun) canvas.DrawText(word, wordX, wordY, paint);
                    wordX += wordWidth + spaceWidth;
                }
                else
                {
                    wordY += paint.FontSpacing;
                    wordX = rect.Left + margin;
                    if (!dryRun) canvas.DrawText(word, wordX, wordY, paint);
                    wordX += wordWidth + spaceWidth;
                }
            }
            return (int)(wordY - rect.Top + margin);
        }

        private static Line[] SplitLines(string text, SKPaint paint, float maxWidth)
        {
            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<Line>();

                var words = line.Split(new[] { " " }, StringSplitOptions.None);

                var lineResult = new StringBuilder();
                float width = 0;
                foreach (var word in words)
                {
                    var wordWidth = paint.MeasureText(word);
                    var wordWithSpaceWidth = wordWidth + spaceWidth;
                    var wordWithSpace = word + " ";

                    if (width + wordWidth > maxWidth)
                    {
                        result.Add(new Line() { Value = lineResult.ToString(), Width = width });
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add(new Line() { Value = lineResult.ToString(), Width = width });

                return result.ToArray();
            }).ToArray();
        }
    }
}

