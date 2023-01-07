using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Threading.Tasks;
using AstroWall.BusinessLayer;
using Newtonsoft.Json;
using SkiaSharp;

namespace AstroWall
{
    /// <summary>
    /// Helper functions for file handling.
    /// </summary>
    internal class FileHelpers
    {
        // Log
        private static Action<string> log = Logging.GetLogger("FileHelpers");
        private static Action<string> logError = Logging.GetLogger("FileHelpers", true);

        /// <summary>
        /// Downloads image to "Astro Directory" (see application layer / general.helpers).
        /// </summary>
        /// <param name="imgurl">Online image url.</param>
        /// <returns>String with the local filename.</returns>
        internal static async Task<string> DownloadUrlToImageStorePath(string imgurl)
        {
            WebClient client = new WebClient();
            Uri uri = new Uri(imgurl);
            string ext = System.IO.Path.GetExtension(imgurl);

            string localFileName = General.GetAstroDirectory() + Guid.NewGuid() + ext;
            log("Downloading file: " + imgurl);
            log("Writing to path: " + localFileName);
            Task t = client.DownloadFileTaskAsync(uri, localFileName);
            await t;
            log("Write complete");
            return localFileName;
        }

        /// <summary>
        /// Download url to tmp path (not "Astro Directory").
        /// </summary>
        /// <returns>String with local path.</returns>
        internal static async Task<string> DownloadUrlToTmpPath(string downloadUrl)
        {
            WebClient client = new WebClient();
            Uri uri = new Uri(downloadUrl);
            string ext = System.IO.Path.GetExtension(downloadUrl);
            string localFileName = Path.GetTempFileName() + ext;
            log("Downloading file: " + downloadUrl);
            log("Writing to tmp path: " + localFileName);
            Task t = client.DownloadFileTaskAsync(uri, localFileName);
            await t;
            log("Write complete");
            return localFileName;
        }

        /// <summary>
        /// Loads SKBitmap from file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static async Task<SKBitmap> LoadImageFromLocalUrl(string path)
        {
            SKBitmap bitmap;
            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    log("Opening file: " + path);
                    FileStream fs = new FileStream(path, FileMode.Open);

                    await fs.CopyToAsync(memStream);
                    fs.Close();
                    log("Closed file: " + path);
                    memStream.Seek(0, SeekOrigin.Begin);

                    SKImage img = SKImage.FromEncodedData(memStream);
                    bitmap = SKBitmap.FromImage(img);
                    memStream.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception ex)
            {
                log("image load error: " + path + ", " + ex.Message);
                throw;
            }

            return bitmap;
        }

        /// <summary>
        /// Serializes object.
        /// </summary>
        /// <param name="c">Object.</param>
        /// <returns>To JSON string.</returns>
        internal static string SerializeNow(object c)
        {
            return JsonConvert.SerializeObject(c);
        }

        /// <summary>
        /// Serialize object.
        /// </summary>
        /// <param name="c">Object.</param>
        /// <param name="path">To JSON file at path.</param>
        internal static void SerializeNow(object c, string path)
        {
            string jsonString = JsonConvert.SerializeObject(c);
            File.WriteAllText(path, jsonString);
        }

        /// <summary>
        /// Deserializes JSON file residing at path.
        /// </summary>
        /// <typeparam name="T">Type of object that is expected.</typeparam>
        /// <returns>Object of type T.</returns>
        internal static T DeSerializeNow<T>(string path)
        {
            string jsonString = string.Join(string.Empty, File.ReadAllLines(path));
            T result = JsonConvert.DeserializeObject<T>(jsonString);
            return result;
        }

        /// <summary>
        /// Find out if DB JSON file exists. Has sideeffect, that it will creata the prefs folder, if it does not exist.
        /// </summary>
        /// <returns>True if DB file exists.</returns>
        internal static bool DBExists()
        {
            log("Checking, if db present at: " + General.GetDBPath());
            return File.Exists(General.GetDBPath());
        }

        /// <summary>
        /// Find out if prefs JSON file exists. Has sideeffect, that it will creata the DB folder, if it does not exist.
        /// </summary>
        /// <returns>True if prefs file exists.</returns>
        internal static bool PrefsExists()
        {
            return File.Exists(General.GetPrefsPath());
        }

        /// <summary>
        /// Deletes file at path.
        /// </summary>
        internal static void DeleteFile(string path)
        {
            File.Delete(path);
            log("File deleted at: " + path);
        }

        /// <summary>
        /// Creates a copy of a file in the tmp folder.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        internal static string GenTmpCopy(string src)
        {
            string ext = System.IO.Path.GetExtension(src);
            string dst = Path.GetTempFileName() + ext;
            File.Copy(src, dst);
            log("File copied from // to: " + src + " // " + dst);
            return dst;
        }
    }
}