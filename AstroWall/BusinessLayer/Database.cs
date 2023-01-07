using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkiaSharp;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// JSON database operations.
    /// </summary>
    internal class Database
    {
        /// <summary>
        /// Private list with all loded images (wrapped).
        /// </summary>
        private readonly List<ImgWrap> imgWrapList;

        // Log
        private readonly Action<string> log = Logging.GetLogger("Database");

        /// <summary>
        /// Private list which keeps track of data loading process from online.
        /// </summary>
        private List<Task> dataLoadList;

        /// <summary>
        /// Private list which keeps track of image loading process from online.
        /// </summary>
        private List<Task> imgLoadList;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// Tries to read db from disk.
        /// </summary>
        internal Database()
        {
            this.imgWrapList = new List<ImgWrap>();
            if (FileHelpers.DBExists())
            {
                this.log("db exists, deserialize");
                this.imgWrapList = FileHelpers.DeSerializeNow<List<ImgWrap>>(General.GetDBPath());
            }
            else
            {
                this.log("db not found");
            }
        }

        /// <summary>
        /// Gets latest image.
        /// </summary>
        internal ImgWrap Latest
        {
            get
            {
                if (imgWrapList == null || imgWrapList.Count == 0)
                {
                    return null;
                }
                else
                {
                    return imgWrapList[0];
                }
            }
        }

        private string[] DatesLoaded => imgWrapList.Select((iw) => iw.PublishDate.ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture)).ToArray();

        /// <summary>
        /// Loads data starting at date and going back "n" days.
        /// </summary>
        /// <param name="n">How many images to load data from.</param>
        /// <param name="date">Starting date.</param>
        /// <param name="forceReload">Forces reload of data.</param>
        /// <returns>Returns true on successfull onlinecheck, e.g. false if connection offline.</returns>
        internal async Task<bool> LoadDataButNoImgFromOnlineStartingAtDate(int n, DateTime date, bool forceReload = false)
        {
            bool allOfDBHasDataLoaded = imgWrapList.All(iw => iw.OnlineDataIsLoadedOrUngettable);
            bool datesAreInDB = HasDates(n, date);
            bool isOffline = false;
            List<ImgWrap> tmpImgWrapList = new List<ImgWrap>();
            log("DB has all wanted days registered: " + datesAreInDB);
            log("All of DB has data loaded: " + allOfDBHasDataLoaded);

            // Everything is loaded, no need to download
            if (datesAreInDB && allOfDBHasDataLoaded && !forceReload)
            {
                log("Everything is loaded, no onlinecheck, unnecessary check?");
                return true;
            }

            // Download n latest days, if not duplicates
            dataLoadList = new List<Task>();
            try
            {
                for (int i = 0; i < n; i++)
                {
                    DateTime potentialDownload = date.AddDays(-i);
                    if (!this.HasDate(potentialDownload))
                    {
                        log("adding day: " + date.AddDays(-i));
                        ImgWrap tmppw = new ImgWrap(date.AddDays(-i));
                        Task t = tmppw.LoadOnlineDataButNotImg();
                        tmpImgWrapList.Add(tmppw);
                        dataLoadList.Add(t);
                    }
                }

                await Task.WhenAll(dataLoadList);
            }
            catch (Exception ex)
            {
                // Catch offline exception
                if (ex.Message.Contains("No such host is known"))
                {
                    isOffline = true;
                }
                else
                {
                    throw;
                }
            }

            if (!isOffline)
            {
                // Filter out not found results, so that they can be fetched again in the future;
                imgWrapList.AddRange(tmpImgWrapList.Where(iw => !iw.NotFound));
                return true;
            }
            else
            {
                log("No host: probably offline, nothing is added to ImgWrapList");
                return false;
            }
        }

        /// <summary>
        /// Loads the actual image data of all the items where data has been loaded.
        /// </summary>
        /// <param name="forceReload"></param>
        /// <returns>Task completes on successfull load.</returns>
        internal async Task LoadImgs(bool forceReload = false)
        {
            if (imgWrapList.All(iw => iw.ImgsAreLoadedOrUngettable) && !forceReload)
            {
                log("All images loaded, no download");
                return;
            }
            else
            {
                imgLoadList = new List<Task>();
                foreach (ImgWrap pw in imgWrapList)
                {
                    if (pw.Integrity && pw.ImgIsGettable && !pw.ImgIsLoaded)
                    {
                        imgLoadList.Add(Task.Run(() => pw.LoadImg()));
                    }
                }

                await Task.WhenAll(imgLoadList);
            }
        }

        /// <summary>
        /// Saves db to disk.
        /// </summary>
        internal void SaveToDisk()
        {
            FileHelpers.SerializeNow(imgWrapList, General.GetDBPath());
        }

        /// <summary>
        /// Gets gettable images.
        /// </summary>
        /// <returns>Returns list with all images that can be downloaded, e.g. youtube videos filtered out.</returns>
        internal List<ImgWrap> GetGettableImages()
        {
            return imgWrapList.Where((iw) => iw.ImgIsGettable).ToList<ImgWrap>();
        }

        /// <summary>
        /// Sorts imgWrapList.
        /// </summary>
        internal void Sort()
        {
            imgWrapList.Sort();
        }

        private static bool LoadDBFromDisk()
        {
            return true;
        }

        /// <summary>
        /// Checks if dates are in db starting at <c>date</c> and going back <c>n</c> days.
        /// </summary>
        /// <param name="n">n dates to check starting from (and including) date argument and going backwards.</param>
        /// <param name="date">first date to check</param>
        /// <returns>bool.</returns>
        private bool HasDates(int n, DateTime date)
        {
            string[] datesLoadedCache = DatesLoaded;
            string[] datesToCheck = new string[n];

            // Gen dates to check
            for (int i = 0; i < n; i++)
            {
                datesToCheck[i] = date.AddDays(-i).ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture);
            }

            // Check dates
            bool datesAreInDB = true;
            foreach (string dateToCheck in datesToCheck)
            {
                if (!datesLoadedCache.Contains(dateToCheck))
                {
                    log($"Date {dateToCheck} not found");
                    datesAreInDB = false;
                }
                else
                {
                    log($"Date {dateToCheck} found");
                }
            }

            return datesAreInDB;
        }

        private bool HasDate(DateTime date)
        {
            return DatesLoaded.Contains(date.ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}