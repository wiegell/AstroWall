using System;
using Newtonsoft.Json;
using SkiaSharp;
using System.IO;
using System.Threading.Tasks;

namespace AstroWall.BusinessLayer
{
    [JsonObject(MemberSerialization.OptOut)]
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
        public string FileType { get; private set; }
        [JsonProperty]
        public DateTime PublishDate { get; private set; }
        [JsonProperty]
        public string Title { get; private set; }
        [JsonProperty]
        public string Description { get; private set; }
        [JsonProperty]
        public bool integrity = true;
        [JsonProperty]
        public bool imgIsGettable = true;

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

        public async Task LoadOnlineDataButNotImg()
        {
            if (OnlineDataIsLoaded())
            {
                Console.WriteLine("already loaded");
                return;
            };

            try
            {
                Console.WriteLine("loadonline url: " + PageUrl);
                this.ImgOnlineUrl = await HTMLHelpers.getImgOnlineUrl(PageUrl);
                string[] splitUrl = ImgOnlineUrl.Split(".");
                FileType = splitUrl[splitUrl.Length - 1];
            }
            catch (Exception ex)
            {
                Console.WriteLine("could not get pic url, integrity fail of: " + PageUrl);
                integrity = false;
                imgIsGettable = false;
            }



            string[] tmp = new string[2];
            try
            {
                tmp = await HTMLHelpers.getDescAndTitleFromOnlineUrl(PageUrl);
                Title = tmp[0];
                Description = tmp[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine("could not get descr. or title of:" + PageUrl);
                integrity = false;
            }

            integrity = true;
        }

        public async Task LoadImg()
        {
            if (OnlineDataAndPicIsLoaded())
            {
                throw new Exception("already loaded");
            }
            else if (!imgIsGettable)
            {
                throw new Exception("img not gettable");
            }

            try
            {
                ImgLocalUrl = await FileHelpers.DownloadUrlToImageStorePath(ImgOnlineUrl);
                await createPreviewFromFullSize();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: could not load online url - no connection?: " + ImgOnlineUrl);
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
                Console.WriteLine("error loading file ({0}): {1}", ImgLocalUrl, ex.Message);
                return;
            }

            try
            {
                image = image.Resize(new SKSize(500, 360).ToSizeI(), SKFilterQuality.Medium);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error resizing image ({0}): {1}", ImgLocalUrl, ex.Message);

            }

            try
            {
                string path = ImgLocalUrl + "_preview." + FileType;
                FileStream f = File.Create(path);
                image.Encode(f, (OnlineUrlIsJPG() ? SKEncodedImageFormat.Jpeg : SKEncodedImageFormat.Png), 90);
                ImgLocalPreviewUrl = path;
                Console.WriteLine("preview saved ({0}): {1}", FileType, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving preview ({0}): {1}", ImgLocalUrl, ex.Message);
            }
            return;
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
                ImgLocalUrl == null;
        }

        public bool OnlineDataIsLoaded()
        {
            return
                (ImgOnlineUrl != null &&
                FileType != null &&
                Title != null &&
                Description != null &&
                integrity);
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
            return (PreviewIsLoaded() && FullResIsLoaded()) || !imgIsGettable;
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

