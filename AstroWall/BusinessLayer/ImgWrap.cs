using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AstroWall.ApplicationLayer.Helpers;
using AstroWall.BusinessLayer;
using Newtonsoft.Json;
using SkiaSharp;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Wrapper for image with data like path, publishdate, etc.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ImgWrap : IComparable
    {
        // Log
        private readonly Action<string> log = Logging.GetLogger("ImgWrap");

        /// <summary>
        /// Initializes a new instance of the <see cref="ImgWrap"/> class.
        /// Fills in PublishDate and PageUrl from supplied DateTime.
        /// </summary>
        /// <param name="date"></param>
        public ImgWrap(DateTime date)
        {
            this.PublishDate = date;
            this.PageUrl = HTMLHelpers.GenPublishDateUrl(date);
        }

        /// <summary>
        /// Gets the url to the online page that contains the image.
        /// </summary>
        [JsonProperty]
        internal string PageUrl { get; private set; }

        /// <summary>
        /// Gets the url to the online image.
        /// </summary>
        [JsonProperty]
        internal string ImgOnlineUrl { get; private set; }

        /// <summary>
        /// Gets the url to local image.
        /// </summary>
        [JsonProperty]
        internal string ImgLocalUrl { get; private set; }

        /// <summary>
        /// Gets the url to local preview image.
        /// </summary>
        [JsonProperty]
        internal string ImgLocalPreviewUrl { get; private set; }

        /// <summary>
        /// Gets the postprocessed urls by screen names.
        /// </summary>
        [JsonProperty]
        internal Dictionary<string, string> ImgLocalPostProcessedUrlsByScreenId { get; private set; }

        /// <summary>
        /// Gets the filetype of the image.
        /// </summary>
        [JsonProperty]
        internal string FileType { get; private set; }

        /// <summary>
        /// Gets the publish DateTime of the image.
        /// </summary>
        [JsonProperty]
        internal DateTime PublishDate { get; private set; }

        /// <summary>
        /// Gets the image title.
        /// </summary>
        [JsonProperty]
        internal string Title { get; private set; }

        /// <summary>
        /// Gets the image description.
        /// </summary>
        [JsonProperty]
        internal string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current value has integrity.
        /// TODO is this used anymore?.
        /// </summary>
        [JsonProperty]
        internal bool Integrity { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether the image is gettable (e.g. not youtube video or the like).
        /// </summary>
        [JsonProperty]
        internal bool ImgIsGettable { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether the image can be found online (e.g. false if not yet published).
        /// </summary>
        [JsonProperty]
        internal bool NotFound { get; private set; } = false;

        /// <summary>
        /// Gets credit text.
        /// </summary>
        [JsonProperty]
        internal string Credit { get; private set; }

        /// <summary>
        /// Gets credit url.
        /// </summary>
        [JsonProperty]
        internal string CreditUrl { get; private set; }

        /// <summary>
        /// Gets local image url without extension.
        /// </summary>
        internal string ImgLocalUrlNoExtension { get => System.IO.Path.ChangeExtension(ImgLocalUrl, null); }

        /// <summary>
        /// Gets a value indicating whether online data (e.g. title text) is loaded and image is loaded.
        /// </summary>
        internal bool OnlineDataAndImgIsLoaded => OnlineDataIsLoaded &&
                ImgLocalUrl != null;

        /// <summary>
        /// Gets a value indicating whether online data (e.g. title text) is loaded, but image is not loaded.
        /// </summary>
        internal bool OnlineDataExceptPicIsLoaded => OnlineDataIsLoaded &&
                !ImgIsLoaded;

        /// <summary>
        /// Gets a value indicating whether the actual image is loaded.
        /// </summary>
        internal bool ImgIsLoaded => ImgLocalUrl != null;

        /// <summary>
        /// Gets a value indicating whether online data is loaded (e.g. title text).
        /// </summary>
        internal bool OnlineDataIsLoaded => ImgOnlineUrl != null &&
                FileType != null &&
                Title != null &&
                Description != null &&
                Integrity;

        /// <summary>
        /// Gets a value indicating whether online data is loaded or not gettable.
        /// </summary>
        internal bool OnlineDataIsLoadedOrUngettable => (ImgOnlineUrl != null &&
                FileType != null &&
                Title != null &&
                Description != null &&
                Integrity) || !ImgIsGettable;

        /// <summary>
        /// Gets a value indicating whether online image url has a valid image extension.
        /// </summary>
        internal bool OnlineUrlIsValidImg => this.OnlineUrlIsJPG || this.OnlineUrlIsPNG;

        /// <summary>
        /// Gets a value indicating whether online image url has filetype jpg.
        /// </summary>
        internal bool OnlineUrlIsJPG => FileType.Equals("jpg", StringComparison.OrdinalIgnoreCase) || FileType.Equals("jpeg", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether online image url has filetype png.
        /// </summary>
        internal bool OnlineUrlIsPNG => FileType.Equals("png", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a value indicating whether image is loaded and preview has been created.
        /// </summary>
        internal bool PreviewIsLoaded => ImgLocalPreviewUrl != null;

        /// <summary>
        /// Gets a value indicating whether image is loaded. Preview not necessarily loaded.
        /// </summary>
        internal bool FullResIsLoaded => ImgLocalUrl != null;

        /// <summary>
        /// Gets a value indicating whether image is loaded. Preview not necessarily loaded.
        /// </summary>
        internal bool ImgsAreLoadedOrUngettable => (PreviewIsLoaded && FullResIsLoaded) || !ImgIsGettable;

        public static bool operator ==(ImgWrap left, ImgWrap right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(ImgWrap left, ImgWrap right)
        {
            return !(left == right);
        }

        public static bool operator <(ImgWrap left, ImgWrap right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(ImgWrap left, ImgWrap right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(ImgWrap left, ImgWrap right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(ImgWrap left, ImgWrap right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Equals override. Uses custom compare function for dates, only comparing year and date, not time.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True on equal.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImgWrap))
            {
                return false;
            }

            return DateTimeHelpers.DTEquals(this.PublishDate, ((ImgWrap)obj).PublishDate);
        }

        /// <summary>
        /// Hashcode on same terms as Equals().
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(PublishDate.Year, PublishDate.Day, PublishDate.Month);
        }

        /// <summary>
        /// Compares via PublishDates.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws if object is not ImgWrap type.</exception>
        public int CompareTo(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                throw new ArgumentException("only compare to ImgWrap");
            }

            // minus is to reverse sort order
            return -this.PublishDate.CompareTo(((ImgWrap)obj).PublishDate);
        }

        /// <summary>
        /// Loads online data, e.g. text and description but no image data. Loads the data to current this instance.
        /// </summary>
        /// <returns>Task on complete load.</returns>
        internal async Task LoadOnlineDataButNotImg()
        {
            if (OnlineDataIsLoaded)
            {
                log("already loaded");
                return;
            }

            try
            {
                log("loadonline url: " + PageUrl);
                UrlResponseWrap wrap = await HTMLHelpers.GetImgOnlineUrl(PageUrl);
                if (wrap.PageStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotFound = true;
                }
                else
                {
                    this.ImgOnlineUrl = wrap.ImageUrl;
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
                    throw;
                }
            }

            string[] tmp = new string[3];
            try
            {
                tmp = await HTMLHelpers.ParseTitleDescriptiontAndCreditFromOnlineUrl(PageUrl);
                Title = tmp[0];
                Description = tmp[1];
                Credit = tmp[2];
                CreditUrl = tmp[3];
            }
            catch (Exception)
            {
                log("could not get descr., credit or title of:" + PageUrl);
                Integrity = false;
            }

            Integrity = true;
        }

        /// <summary>
        /// Downloads the actual image, then stores it to astro path (see Application layer / General).
        /// Offline connection gets logged, but does not throw.
        /// </summary>
        /// <returns>Task complete when download complete.</returns>
        /// <exception cref="InvalidOperationException">Throws if image is already loaded or not gettable.</exception>
        internal async Task LoadImg()
        {
            if (OnlineDataAndImgIsLoaded)
            {
                throw new InvalidOperationException("Already loaded");
            }
            else if (!ImgIsGettable)
            {
                throw new InvalidOperationException("img not gettable");
            }

            try
            {
                log("Commencing download of image on date: " + this.PublishDate);
                ImgLocalUrl = await FileHelpers.DownloadUrlToImageStorePath(ImgOnlineUrl);
                await CreatePreviewFromFullSize();
            }
            catch (Exception)
            {
                // TODO is this sufficient to promp correct redownload when online again?
                log("Error: could not load online url - no connection?: " + ImgOnlineUrl);
            }
        }

        /// <summary>
        /// Creates postprocessed images.
        /// </summary>
        /// <param name="screens">Screens to create postprocessed images for.</param>
        /// <param name="postProcessPrefsDictionary">Post process prefs.</param>
        /// <returns>Task that completes when postprocess is done.</returns>
        /// <exception cref="InvalidOperationException">Throws on error on postprocessing.</exception>
        internal async Task CreatePostProcessedImages(Dictionary<string, Screen> screens, Dictionary<Preferences.PostProcessPreferenceEnum, Preferences.PostProcessPreference> postProcessPrefsDictionary)
        {
            log("creating postprocessed images on thread: " + Environment.CurrentManagedThreadId);

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
                log($"error loading file ({ImgLocalUrl}): {ex.Message}");
                return;
            }

            // Prep Dictionary vars for postprocess
            Dictionary<Screen, SKBitmap> unProcessedImagesByScreen =
                screens.ToDictionary(
                    screenKV => screenKV.Value, // Set Screen instance as key
                    screenKV => image); // Value as unprocessed full res image loaded above
            Dictionary<Screen, SKBitmap> postProcessedImagesByScreenId;

            // Prep postprocess chain
            Func<Dictionary<Screen, SKBitmap>> postProcessChain =
                Wallpaper.PostProcess.ComposePostProcess(
                    () => unProcessedImagesByScreen,
                    Wallpaper.PostProcess.ScaleAndCrop,
                    Wallpaper.PostProcess.AddTextCurry(
                        (Preferences.AddTextPreference)postProcessPrefsDictionary[Preferences.PostProcessPreferenceEnum.AddText],
                        Title,
                        Description,
                        Credit));

            // Do the postprocessing
            try
            {
                postProcessedImagesByScreenId = postProcessChain();
            }
            catch (Exception ex)
            {
                log($"Error postprocessing image ({ImgLocalUrl}): {ex.Message}, {ex.StackTrace}");
                throw new InvalidOperationException("Error postprocessing.", ex);
            }

            // Remove old postprocessed files
            // (needs to be saved under new name to prevent wrong cache
            log("Commencing deleting old postproccesed images");
            if (ImgLocalPostProcessedUrlsByScreenId != null)
            {
                foreach (var kv in ImgLocalPostProcessedUrlsByScreenId)
                {
                    log($"Trying to delete postprocessed image at {kv.Value}");

                    FileHelpers.DeleteFile(kv.Value);
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
                        bitmapKV.Value.Encode(f, OnlineUrlIsJPG ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, 90);
                        log($"Postprocess for screen {bitmapKV.Key} saved to path {path}");
                        f.Close();
                        return path;
                    });
            }
            catch (Exception ex)
            {
                log($"Error saving postprocess ({ImgLocalUrl}): {ex.Message}");
            }
        }

        /// <summary>
        /// Creates preview from full size. Logs error if full size cannot be loaded, but does not throw. The same on resize fail.
        /// </summary>
        /// <param name="width">Width of preview in px.</param>
        /// <param name="height">Height of preview in px.</param>
        private async Task CreatePreviewFromFullSize(int width = 250, int height = 180)
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
                log($"Error resizing image ({ImgLocalUrl}): {ex.Message}");
            }

            try
            {
                string path = ImgLocalUrl + "_preview." + FileType;
                FileStream f = File.Create(path);
                image.Encode(f, OnlineUrlIsJPG ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png, 90);
                ImgLocalPreviewUrl = path;
                log($"preview saved ({FileType}): {path}");
            }
            catch (Exception ex)
            {
                log($"Error saving preview ({ImgLocalUrl}): {ex.Message}");
            }

            return;
        }
    }
}