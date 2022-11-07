using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using Newtonsoft.Json;

namespace AstroWall
{

    public class Database
    {
        public string Title { get; private set; }

        public List<ImgWrap> ImgWrapList { get; private set; }
        private Task[] DataLoadList;
        private List<Task> ImgLoadList;

        public Database()
        {
            ImgWrapList = new List<ImgWrap>();
            if (FileHelpers.DBExists())
            {
                Console.WriteLine("db exists, deserialize");
                ImgWrapList = FileHelpers.DeSerializeNow<List<ImgWrap>>(MacOShelpers.getDBPath());
            } else
            {
                Console.WriteLine("db not found");
            }
        }

        private string[] datesLoaded()
        {
            return ImgWrapList.Select((iw) => iw.PublishDate.ToString(HTMLHelpers.NASADateFormat)).ToArray();
        }

        /// <summary>
        /// checks if dates are in db starting at <c>date</c> and going back <c>n</c> days
        /// </summary>
        /// <param name="date">first date to check</param>
        /// <param name="n">n dates to check starting from (and including) date argument and going backwards</param>
        /// <returns></returns>
        private bool checkDatesAreInDB(int n, DateTime date)
        {
            string[] datesLoadedCache = datesLoaded();
            string[] datesToCheck = new string[n];

            // Gen dates to check
            for (int i = 0; i < n; i++) datesToCheck[i] = date.AddDays(-i).ToString(HTMLHelpers.NASADateFormat);

            // Check dates
            bool datesAreInDB = true;
            foreach (string dateToCheck in datesToCheck)
            {
                Console.Write("Checking date: {0}", dateToCheck);
                if (!datesLoadedCache.Contains(dateToCheck))
                {
                    Console.Write(", date not found");
                    datesAreInDB = false;
                }
                else Console.Write(", date found");
                Console.WriteLine();
            }

            return datesAreInDB;

        }

        public async Task LoadDataButNoImgFromOnlineStartingAtDate(int n, DateTime date, bool forceReload = false)
        {
            bool allOfDBHasDataLoaded = ImgWrapList.All(iw => iw.OnlineDataExceptPicIsLoaded());
            bool datesAreInDB = checkDatesAreInDB(n, date);
            Console.WriteLine("impwraplistafter deserialize: " + ImgWrapList.Count);
            foreach (ImgWrap iw in ImgWrapList) Console.WriteLine("localUrl: " + iw.ImgLocalUrl);
            Console.WriteLine("datesindb: " + datesAreInDB);
            Console.WriteLine("allOfDBHasDataLoaded: " + allOfDBHasDataLoaded);

            // Everything is loaded, no need to download
            if (datesAreInDB && allOfDBHasDataLoaded && !forceReload) return;



            DataLoadList = new Task[n];
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine("adding day: " + date.AddDays(-i));
                ImgWrap tmppw = new ImgWrap(date.AddDays(-i));
                Task t = tmppw.LoadOnlineDataButNotImg();
                DataLoadList[i] = (t);
                ImgWrapList.Add(tmppw);
            }
            await Task.WhenAll(DataLoadList);
        }

        public async Task LoadImgs(bool forceReload = false)
        {
            if (ImgWrapList.All(iw => iw.ImgsAreLoadedOrUngettable()) && !forceReload)
            {
                Console.WriteLine("All images loaded, no download");
                return;
            };


            ImgLoadList = new List<Task>();
            foreach (ImgWrap pw in ImgWrapList)
            {
                if (pw.OnlineDataExceptPicIsLoaded() && pw.integrity)
                    ImgLoadList.Add(Task.Run(() => pw.LoadImg()));

            }
            await Task.WhenAll(ImgLoadList);
        }


        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(ImgWrapList, MacOShelpers.getDBPath());
        }

        private bool loadDBFromDisk()
        {
            return true;
        }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class ImgWrap
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
            if (OnlineDataExceptPicIsLoaded())
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
            if (!OnlineDataExceptPicIsLoaded())
            {
                throw new Exception("no imgurl to load");
            };

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

        public bool OnlineDataExceptPicIsLoaded()
        {
            return !imgIsGettable || (ImgOnlineUrl != null && FileType != null && Title != null && Description != null && integrity);
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

    }
}

