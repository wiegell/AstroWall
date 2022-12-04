using System;
using Newtonsoft.Json;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;
using AstroWall.BusinessLayer;
using System.Collections.Generic;
using System.Linq;
using AstroWall.ApplicationLayer.Helpers;
using System.Threading;

namespace AstroWall.BusinessLayer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ImgWrap : IComparable
    {
        [JsonProperty]
        public string PageUrl { get; private set; }
        [JsonProperty]
        public string ImgOnlineUrl { get; private set; }
        [JsonProperty]
        public string ImgLocalUrl { get; private set; }
        [JsonProperty]
        public string ImgLocalPreviewUrl { get; private set; }
        [JsonProperty]
        public Dictionary<string, string> ImgLocalPostProcessedUrlsByScreenId;
        [JsonProperty]
        public string FileType { get; private set; }
        [JsonProperty]
        public DateTime PublishDate { get; private set; }
        [JsonProperty]
        public string Title { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }
        [JsonProperty]
        public bool Integrity = true;
        [JsonProperty]
        public bool ImgIsGettable = true;
        [JsonProperty]
        public bool NotFound = false;
        [JsonProperty]
        public string Credit { get; private set; }
        [JsonProperty]
        public string CreditUrl { get; private set; }

        // Log
        private Action<string> log = Logging.GetLogger("ImgWrap");

        public override bool Equals(object o)
        {
            if (!(o is ImgWrap)) return false;
            return DateTimeHelpers.DTEquals(this.PublishDate, ((ImgWrap)o).PublishDate);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(PublishDate.Year, PublishDate.Day, PublishDate.Month);
        }

        public ImgWrap(DateTime date)
        {
            this.PublishDate = date;
            this.PageUrl = HTMLHelpers.genPublishDateUrl(date);
        }

        // Used for JSON deserialize
        public ImgWrap()
        {

        }

        public string ImgLocalUrlNoExtension { get => System.IO.Path.ChangeExtension(ImgLocalUrl, null); }

        public async Task LoadOnlineDataButNotImg()
        {
            if (OnlineDataIsLoaded())
            {
                log("already loaded");
                return;
            };

            try
            {
                log("loadonline url: " + PageUrl);
                urlResponseWrap wrap = await HTMLHelpers.getImgOnlineUrl(PageUrl);
                if (wrap.status == System.Net.HttpStatusCode.NotFound)
                {
                    NotFound = true;
                }
                else
                {
                    this.ImgOnlineUrl = wrap.url;
                    string[] splitUrl = ImgOnlineUrl.Split(".");
                    FileType = splitUrl[splitUrl.Length - 1];
                }
            }
            catch (Exception ex)
            {
                log("could not get pic url, integrity fail of: " + PageUrl + " Ex type: " + ex.GetType() + " Ex message: " + ex.Message);

                Integrity = false;
                ImgIsGettable = false;
                if (ex.Message.Contains("No such host is known"))
                {
                    // Throw up http exception, gets caught up the chain
                    throw ex;
                }
            }



            string[] tmp = new string[3];
            try
            {
                tmp = await HTMLHelpers.getDescTitleAndCreditFromOnlineUrl(PageUrl);
                Title = tmp[0];
                Description = tmp[1];
                Credit = tmp[2];
                CreditUrl = tmp[3];
            }
            catch (Exception ex)
            {
                log("could not get descr., credit or title of:" + PageUrl);
                Integrity = false;
            }

            Integrity = true;
        }

        public async Task LoadImg()
        {
            if (OnlineDataAndPicIsLoaded())
            {
                throw new Exception("already loaded");
            }
            else if (!ImgIsGettable)
            {
                throw new Exception("img not gettable");
            }

            try
            {
                log("Commencing download of date: " + this.PublishDate);
                ImgLocalUrl = await FileHelpers.DownloadUrlToImageStorePath(ImgOnlineUrl);
                await createPreviewFromFullSize();
            }
            catch (Exception ex)
            {
                log("Error: could not load online url - no connection?: " + ImgOnlineUrl);
            }
        }

        private async Task createPreviewFromFullSize(int width = 250, int height = 180)
        {
            SKBitmap image;
            try
            {
                image = await FileHelpers.LoadImageFromLocalUrl(ImgLocalUrl);
            }
            catch (Exception ex)
            {
                log($"error loading file ({ImgLocalUrl}): {ex.Message}");
                return;
            }

            try
            {
                image = image.Resize(new SKSize(500, 360).ToSizeI(), SKFilterQuality.Medium);
            }
            catch (Exception ex)
            {
                log("Error resizing image ({ImgLocalUrl}): {ex.Message}");

            }

            try
            {
                string path = ImgLocalUrl + "_preview." + FileType;
                FileStream f = File.Create(path);
                image.Encode(f, (OnlineUrlIsJPG() ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png), 90);
                ImgLocalPreviewUrl = path;
                log($"preview saved ({FileType}): {path}");
            }
            catch (Exception ex)
            {
                log($"Error saving preview ({ImgLocalUrl}): {ex.Message}");
            }
            return;
        }

        public async Task createPostProcessedImages(Dictionary<string, Screen> screens, Dictionary<Preferences.PostProcessType, Preferences.PostProcess> postProcessPrefsDictionary)
        {
            log("creating postprocessed images on thread: " + Thread.CurrentThread.ManagedThreadId);

            // Load full res image
            SKBitmap image;
            try
            {
                log("Trying to load image at local url: " + ImgLocalUrl);
                log(FileHelpers.SerializeNow(this));
                image = await FileHelpers.LoadImageFromLocalUrl(ImgLocalUrl);
            }
            catch (Exception ex)
            {
                log("error loading file ({ImgLocalUrl}): {ex.Message}");
                return;
            }

            // Prep Dictionary vars for postprocess
            Dictionary<Screen, SKBitmap> unProcessedImagesByScreen =
                screens.ToDictionary(
                    // Set Screen instance as key
                    screenKV => screenKV.Value,
                    // Value as unprocessed full res image loaded above
                    screenKV => image
                    );
            Dictionary<Screen, SKBitmap> postProcessedImagesByScreenId;

            // Prep postprocess chain
            Func<Dictionary<Screen, SKBitmap>> postProcessChain =
                Wallpaper.PostProcess.ComposePostProcess(
                    () => unProcessedImagesByScreen,
                    Wallpaper.PostProcess.ScaleAndCrop,
                    Wallpaper.PostProcess.AddTextCurry(
                        (Preferences.AddText)postProcessPrefsDictionary[Preferences.PostProcessType.AddText],
                        Title,
                        Description,
                        Credit
                        )

                    );

            // Do the postprocessing
            try
            {
                postProcessedImagesByScreenId = postProcessChain();
            }
            catch (Exception ex)
            {
                log($"Error postprocessing image ({ImgLocalUrl}): {ex.Message}, {ex.StackTrace}");
                throw new Exception("error postprocessing", ex);
            }

            //Remove old postprocessed files
            //(needs to be saved under new name to prevent wrong cache
            log("Commencing deleting old postproccesed images");
            if (ImgLocalPostProcessedUrlsByScreenId != null)
            {

                foreach (var KV in ImgLocalPostProcessedUrlsByScreenId)
                {
                    log($"Trying to delete postprocessed image at {KV.Value}");
                    try
                    {
                        FileHelpers.DeleteFile(KV.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Problem deleting file" + ex.Message);
                    }
                }
            }

            // Save files and register file paths
            log("Commencing postprocess save");
            try
            {
                // This is the format saved in prefs
                ImgLocalPostProcessedUrlsByScreenId = postProcessedImagesByScreenId.ToDictionary(
                    bitmapKV => bitmapKV.Key.Id,
                    bitmapKV =>
                    {
                        Random rnd = new Random();
                        string path = $"{ImgLocalUrlNoExtension}_postprocessed_{bitmapKV.Key.Id}_{rnd.Next(999)}.{FileType}";

                        log($"Commencing save of postprocess of screen {bitmapKV.Key.Id} to path {path}");

                        FileStream f = File.Create(path);
                        bitmapKV.Value.Encode(f, (OnlineUrlIsJPG() ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png), 90);
                        log($"Postprocess for screen {bitmapKV.Key} saved to path {path}");
                        f.Close();
                        return path;
                    }
                    );
            }
            catch (Exception ex)
            {
                log($"Error saving postprocess ({ImgLocalUrl}): {ex.Message}");
            }

        }

        public bool OnlineDataAndPicIsLoaded()
        {
            return
                OnlineDataIsLoaded() &&
                ImgLocalUrl != null;
        }

        public bool OnlineDataExceptPicIsLoaded()
        {
            return
                OnlineDataIsLoaded() &&
                !ImgIsLoaded();
        }

        public bool ImgIsLoaded()
        {
            return ImgLocalUrl != null;
        }

        public bool OnlineDataIsLoaded()
        {
            return
                (ImgOnlineUrl != null &&
                FileType != null &&
                Title != null &&
                Description != null &&
                Integrity);
        }

        public bool OnlineDataIsLoadedOrUngettable()
        {
            return
                (ImgOnlineUrl != null &&
                FileType != null &&
                Title != null &&
                Description != null &&
                Integrity) || !ImgIsGettable;
        }

        public bool OnlineUrlIsValidImg()
        {
            return (this.OnlineUrlIsJPG() || this.OnlineUrlIsPNG());
        }

        public bool OnlineUrlIsJPG()
        {
            return FileType.Equals("jpg", StringComparison.OrdinalIgnoreCase) || FileType.Equals("jpeg", StringComparison.OrdinalIgnoreCase);
        }

        public bool OnlineUrlIsPNG()
        {
            return FileType.Equals("png", StringComparison.OrdinalIgnoreCase);
        }

        public bool PreviewIsLoaded()
        {
            return ImgLocalPreviewUrl != null;
        }

        public bool FullResIsLoaded()
        {
            return ImgLocalUrl != null;
        }

        public bool ImgsAreLoadedOrUngettable()
        {
            return (PreviewIsLoaded() && FullResIsLoaded()) || !ImgIsGettable;
        }

        public int CompareTo(Object o)
        {
            if (o.GetType() != this.GetType())
                throw new Exception("only compare to ImgWrap");
            // - is to reverse sort order
            return -this.PublishDate.CompareTo(((ImgWrap)o).PublishDate);
        }
    }
}

