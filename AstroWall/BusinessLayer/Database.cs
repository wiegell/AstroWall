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

    public class Database
    {
        public string Title { get; private set; }

        public List<ImgWrap> ImgWrapList { get; private set; }
        private List<Task> DataLoadList;
        private List<Task> ImgLoadList;

        public Database()
        {
            ImgWrapList = new List<ImgWrap>();
            if (FileHelpers.DBExists())
            {
                Console.WriteLine("db exists, deserialize");
                ImgWrapList = FileHelpers.DeSerializeNow<List<ImgWrap>>(General.getDBPath());
            }
            else
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
        private bool hasDates(int n, DateTime date)
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

        private bool hasDate(DateTime date)
        {
            return datesLoaded().Contains(date.ToString(HTMLHelpers.NASADateFormat));
        }


        public async Task LoadDataButNoImgFromOnlineStartingAtDate(int n, DateTime date, bool forceReload = false)
        {
            bool allOfDBHasDataLoaded = ImgWrapList.All(iw => iw.OnlineDataExceptPicIsLoaded());
            bool datesAreInDB = hasDates(n, date);
            Console.WriteLine("DB has all wanted days registered: " + datesAreInDB);
            Console.WriteLine("All of DB has data loaded: " + allOfDBHasDataLoaded);

            // Everything is loaded, no need to download
            if (datesAreInDB && allOfDBHasDataLoaded && !forceReload) return;

            // Download n latest days, if not duplicates
            DataLoadList = new List<Task>();
            for (int i = 0; i < n; i++)
            {
                DateTime potentialDownload = date.AddDays(-i);
                if (!this.hasDate(potentialDownload))
                {
                    Console.WriteLine("adding day: " + date.AddDays(-i));
                    ImgWrap tmppw = new ImgWrap(date.AddDays(-i));
                    Task t = tmppw.LoadOnlineDataButNotImg();
                    DataLoadList.Add(t);
                    ImgWrapList.Add(tmppw);
                }
            }
            await Task.WhenAll(DataLoadList);
        }

        public async Task LoadImgs(bool forceReload = false)
        {
            if (ImgWrapList.All(iw => iw.ImgsAreLoadedOrUngettable()) && !forceReload)
            {
                Console.WriteLine("All images loaded, no download");
                return;
            }
            else
            {
                ImgLoadList = new List<Task>();
                foreach (ImgWrap pw in ImgWrapList)
                {
                    if (pw.OnlineDataExceptPicIsLoaded() && pw.integrity && pw.imgIsGettable)
                        ImgLoadList.Add(Task.Run(() => pw.LoadImg()));
                }
                await Task.WhenAll(ImgLoadList);
            }
        }

        public void SaveToDisk()
        {
            FileHelpers.SerializeNow(ImgWrapList, General.getDBPath());
        }

        public List<ImgWrap> getPresentableImages()
        {
            return ImgWrapList.Where((iw) => iw.imgIsGettable).ToList<ImgWrap>();
        }

        private bool loadDBFromDisk()
        {
            return true;
        }

        public void Sort()
        {
            ImgWrapList.Sort();
        }
    }


}

