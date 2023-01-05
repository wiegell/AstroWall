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

namespace AstroWall.BusinessLayer
{

    internal class Database
    {
        internal string Title { get; private set; }

        internal List<ImgWrap> ImgWrapList { get; private set; }
        private List<Task> DataLoadList;
        private List<Task> ImgLoadList;

        // Log
        private Action<string> log = Logging.GetLogger("Database");

        internal Database()
        {
            this.ImgWrapList = new List<ImgWrap>();
            if (FileHelpers.DBExists())
            {
                this.log("db exists, deserialize");
                this.ImgWrapList = FileHelpers.DeSerializeNow<List<ImgWrap>>(General.GetDBPath());
            }
            else
            {
                this.log("db not found");
            }
        }

        internal ImgWrap Latest
        {
            get
            {
                if (ImgWrapList == null || ImgWrapList.Count == 0) return null;
                else return ImgWrapList[0];
            }
        }

        private string[] datesLoaded()
        {
            return ImgWrapList.Select((iw) => iw.PublishDate.ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        }

        /// <summary>
        /// checks if dates are in db starting at <c>date</c> and going back <c>n</c> days
        /// </summary>
        /// <param name="date">first date to check</param>
        /// <param name="n">n dates to check starting from (and including) date argument and going backwards</param>
        /// <returns></returns>
        private bool hasDates(int n, DateTime date)
        {
            string[] datesLoadedCache = datesLoaded();
            string[] datesToCheck = new string[n];

            // Gen dates to check
            for (int i = 0; i < n; i++) datesToCheck[i] = date.AddDays(-i).ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture);

            // Check dates
            bool datesAreInDB = true;
            foreach (string dateToCheck in datesToCheck)
            {
                if (!datesLoadedCache.Contains(dateToCheck))
                {
                    log($"Date {dateToCheck} not found");
                    datesAreInDB = false;
                }
                else log($"Date {dateToCheck} found");
            }

            return datesAreInDB;
        }

        private bool hasDate(DateTime date)
        {
            return datesLoaded().Contains(date.ToString(HTMLHelpers.NASADateFormat, System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Returns true on successfull onlinecheck, e.g. false if connection offline
        /// </summary>
        /// <param name="n"></param>
        /// <param name="date"></param>
        /// <param name="forceReload"></param>
        /// <returns></returns>
        internal async Task<bool> LoadDataButNoImgFromOnlineStartingAtDate(int n, DateTime date, bool forceReload = false)
        {
            bool allOfDBHasDataLoaded = ImgWrapList.All(iw => iw.OnlineDataIsLoadedOrUngettable());
            bool datesAreInDB = hasDates(n, date);
            bool isOffline = false;
            List<ImgWrap> tmpImgWrapList = new List<ImgWrap>();
            log("DB has all wanted days registered: " + datesAreInDB);
            log("All of DB has data loaded: " + allOfDBHasDataLoaded);

            // Everything is loaded, no need to download, may
            if (datesAreInDB && allOfDBHasDataLoaded && !forceReload)
            {
                log("Everything is loaded, no onlinecheck, unnecessary check?");
                return true;

            };

            // Download n latest days, if not duplicates
            DataLoadList = new List<Task>();
            try
            {

                for (int i = 0; i < n; i++)
                {
                    DateTime potentialDownload = date.AddDays(-i);
                    if (!this.hasDate(potentialDownload))
                    {
                        log("adding day: " + date.AddDays(-i));
                        ImgWrap tmppw = new ImgWrap(date.AddDays(-i));
                        Task t = tmppw.LoadOnlineDataButNotImg();
                        tmpImgWrapList.Add(tmppw);
                        DataLoadList.Add(t);
                    }
                }
                await Task.WhenAll(DataLoadList);
            }
            catch (Exception ex)
            {
                // Catch offline exception
                if (ex.Message.Contains("No such host is known"))
                {
                    isOffline = true;
                }
                else throw;
            }

            if (!isOffline)
            {
                // Filter out not found results, so that they can be fetched again in the future;
                ImgWrapList.AddRange(tmpImgWrapList.Where(iw => !iw.NotFound));
                return true;
            }
            else
            {
                log("No host: probably offline, nothing is added to ImgWrapList");
                return false;
            }

        }

        internal async Task LoadImgs(bool forceReload = false)
        {
            if (ImgWrapList.All(iw => iw.ImgsAreLoadedOrUngettable()) && !forceReload)
            {
                log("All images loaded, no download");
                return;
            }
            else
            {
                ImgLoadList = new List<Task>();
                foreach (ImgWrap pw in ImgWrapList)
                {
                    if (pw.Integrity && pw.ImgIsGettable && !pw.ImgIsLoaded())
                        ImgLoadList.Add(Task.Run(() => pw.LoadImg()));
                }
                await Task.WhenAll(ImgLoadList);
            }
        }

        internal void SaveToDisk()
        {
            FileHelpers.SerializeNow(ImgWrapList, General.GetDBPath());
        }

        internal List<ImgWrap> getPresentableImages()
        {
            return ImgWrapList.Where((iw) => iw.ImgIsGettable).ToList<ImgWrap>();
        }

        private static bool loadDBFromDisk()
        {
            return true;
        }

        internal void Sort()
        {
            ImgWrapList.Sort();
        }
    }


}

