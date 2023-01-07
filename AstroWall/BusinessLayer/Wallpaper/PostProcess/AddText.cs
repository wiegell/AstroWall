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
    /// <summary>
    /// Post processing. Is divided into general functions and subfunctions.
    /// Should be reflected with a preference class as well.
    /// </summary>
    internal partial class PostProcess
    {
        // Log
        private static Action<string> log = Logging.GetLogger("Post process");
        private static Action<string> logError = Logging.GetLogger("Post process", true);

        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcess"/> class.
        /// </summary>
        internal PostProcess()
        {
        }

        /// <summary>
        /// Make curry AddText delegate. AddText adds text to bitmap of main screen.
        /// </summary>
        /// <param name="addTextpreferences">AddText prefs.</param>
        /// <param name="title">Title text.</param>
        /// <param name="description">Description text.</param>
        /// <param name="credit">Credit text.</param>
        /// <returns>Returns a curried delegate with all arguments set, that
        /// only needs screen/bitmap dictionary as argument.</returns>
        internal static Func<Dictionary<Screen, SkiaSharp.SKBitmap>, Dictionary<Screen, SkiaSharp.SKBitmap>> AddTextCurry(Preferences.AddTextPreference addTextpreferences, string title, string description, string credit)
        {
            return (Dictionary<Screen, SkiaSharp.SKBitmap> dic) =>
            {
                return AddText(dic, addTextpreferences, title, description, credit);
            };
        }

        /// <summary>
        /// AddText adds text to bitmap of main screen.
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="addTextpreferences">AddText prefs.</param>
        /// <param name="title">Title text.</param>
        /// <param name="description">Description text.</param>
        /// <param name="credit">Credit text.</param>
        /// <returns>Returns new dictionary with text added to main screen bitmap.</returns>
        /// <exception cref="InvalidOperationException">Throws if main screen cannot be determinde.</exception>
        internal static Dictionary<Screen, SkiaSharp.SKBitmap> AddText(Dictionary<Screen, SkiaSharp.SKBitmap> dic, Preferences.AddTextPreference addTextpreferences, string title, string description, string credit)
        {
            SKBitmap mainScreenBitmap;
            Screen mainScreen;
            try
            {
                mainScreen = dic.Where(screen => screen.Key.IsMainScreen).ToArray()[0].Key;
                mainScreenBitmap = dic[mainScreen];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not get main screen from input dict", ex);
            }

            if (mainScreenBitmap == null)
            {
                throw new InvalidOperationException("Could not get main screen from input dict");
            }

            SKBitmap returnBitmap;
            log("Running AddText");
            try
            {
                returnBitmap = mainScreenBitmap.Copy();
            }
            catch (Exception ex)
            {
                logError("Problem copying bitmap:" + ex.Message);
                throw;
            }

            if (addTextpreferences.IsEnabled && description != null && description != string.Empty && title != null && title != string.Empty && credit != null && credit != string.Empty)
            {
                // Format description
                string descriptionFormatted = description.Replace("\n", " ").Replace("Explanation:", string.Empty).Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").TrimStart();

                // Format credit
                string creditFormatted = "Credit / copyright: " + credit.Replace("\n", string.Empty).TrimStart().TrimEnd();

                // string desc = "test \n test\n testtesttest";
                log("desc: " + descriptionFormatted);
                var canvas = new SKCanvas(returnBitmap);
                canvas.DrawBitmap(mainScreenBitmap, 0, 0);
                canvas.ResetMatrix();

                // Paint Title
                PaintToRect(canvas, 1000, 250, 120, 20, 40, false, false, title);

                // Paint description
                int height = PaintToRect(canvas, 1000, 250, 200, 20, 25, true, false, descriptionFormatted);

                // Paint credit
                PaintToRect(canvas, 1000, 250, 200 + height, 20, 25, true, true, creditFormatted);
                canvas.Flush();
                canvas.Dispose();

                // Create shallow dict copy to return
                var returnDic = dic.ToDictionary(x => x.Key, x => x.Value);

                // Replace main screen with new bitmap
                returnDic[mainScreen] = returnBitmap;

                return returnDic;
            }
            else
            {
                return dic;
            }
        }

        private static int PaintToRect(SKCanvas canvas, int width, int x, int y, int margin, int size, bool italic, bool isCredit, string text)
        {
            SKTypeface type = SKTypeface.FromFamilyName("Helvetica Neue", SKFontStyleWeight.Light, SKFontStyleWidth.Normal, italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

            using (var paint = new SKPaint()
            {
                Color = SKColors.Black.WithAlpha((byte)170),
                Style = SKPaintStyle.Fill,
                TextSize = size,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left,
                Typeface = type,
            })
            {
                var tmpRect = SKRect.Create(x, y, width, 3000);
                int height = DrawText(canvas, text, tmpRect, paint, margin, false, true);
                log("Calculated rect height: " + height);
                var properRect = SKRect.Create(x, y, width, height);

                // Background
                canvas.DrawRect(properRect, paint);

                paint.Color = SKColors.White.WithAlpha((byte)150);
                DrawText(canvas, text, properRect, paint, margin, isCredit);
                return height;
            }
        }

        /// <summary>
        /// Draws text to image.
        /// </summary>
        /// <param name="canvas">Canvas to be drawed onto.</param>
        /// <param name="text">Text to be drawn.</param>
        /// <param name="rect">Rect to fit text into.</param>
        /// <param name="paint">Describes text, e.g. size.</param>
        /// <param name="margin">Margin inside rect.</param>
        /// <param name="dryRun">Used to only get the return val and not actually paint to canvas.</param>
        /// <returns>Returns needed rect height.</returns>
        private static int DrawText(SKCanvas canvas, string text, SKRect rect, SKPaint paint, int margin, bool isCredit, bool dryRun = false)
        {
            float spaceWidth = paint.MeasureText(" ");
            float wordX = rect.Left + margin;
            float wordY = rect.Top + paint.TextSize + (isCredit ? 0 : margin);
            foreach (string word in text.Split(' '))
            {
                float wordWidth = paint.MeasureText(word);

                if (wordWidth <= rect.Right - wordX - margin && word != "\n")
                {
                    if (!dryRun)
                    {
                        canvas.DrawText(word, wordX, wordY, paint);
                    }

                    wordX += wordWidth + spaceWidth;
                }
                else
                {
                    wordY += paint.FontSpacing;
                    wordX = rect.Left + margin;
                    if (!dryRun)
                    {
                        canvas.DrawText(word, wordX, wordY, paint);
                    }

                    wordX += wordWidth + spaceWidth;
                }
            }

            return (int)(wordY - rect.Top + margin);
        }

        /// <summary>
        /// Splits text to lines to match max width.
        /// </summary>
        /// <returns>Text lines.</returns>
        private static TextLine[] SplitLines(string text, SKPaint paint, float maxWidth)
        {
            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<TextLine>();

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
                        result.Add(new TextLine() { Value = lineResult.ToString(), Width = width });
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add(new TextLine() { Value = lineResult.ToString(), Width = width });

                return result.ToArray();
            }).ToArray();
        }

        /// <summary>
        /// Used in text box sizing.
        /// </summary>
        private struct TextLine
        {
            internal string Value { get; set; }

            internal float Width { get; set; }
        }
    }
}