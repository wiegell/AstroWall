using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            Console.WriteLine("running postprocess");
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

            var canvas = new SKCanvas(returnBitmap);
            canvas.DrawBitmap(mainScreenBitmap, 0, 0);
            canvas.ResetMatrix();

            PaintToRect(canvas, 1000, 500, 80, returnBitmap.Height - 120, description.Replace("\n","")
                );
            canvas.Flush();
            canvas.Dispose();

            // Create shallow dict copy to return
            var returnDic = dic.ToDictionary(x => x.Key, x => x.Value);

            // Replace main screen with new bitmap
            returnDic[mainScreen] = returnBitmap;

            return returnDic;
        }
        private static void PaintToRect(SKCanvas canvas, int width, int height, int x, int y, string text)
        {
            using (var paint = new SKPaint() { Color = SKColors.Red, Style = SKPaintStyle.Fill, TextSize = 30, })
            {
                var area = SKRect.Create(x, y, width, height);
                // Background
                canvas.DrawRect(area, paint);

                paint.Color = SKColors.White;
                DrawText(canvas, text, area, paint);
            }
        }

        public class Line
        {
            public string Value { get; set; }

            public float Width { get; set; }
        }

        private static void DrawText(SKCanvas canvas, string text, SKRect area, SKPaint paint)
        {
            float lineHeight = paint.TextSize * 1.2f;
            var lines = SplitLines(text, paint, area.Width);
            var height = lines.Count() * lineHeight;

            var y = area.MidY - height / 2;

            foreach (var line in lines)
            {
                y += lineHeight;
                var x = area.MidX - line.Width / 2;
                canvas.DrawText(line.Value, x, y, paint);
            }
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

