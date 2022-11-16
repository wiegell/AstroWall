using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AstroWall.ApplicationLayer.Helpers;
using AstroWall.BusinessLayer.Preferences;
using Foundation;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Wallpaper
{
    public class TaskCancelWrap
    {
        public Task task;
        public CancellationTokenSource cts = new CancellationTokenSource();
        public CancellationToken token;

        public TaskCancelWrap()
        {
            token = cts.Token;
        }

    }

    public class Wallpaper
    {
        // Refs
        ApplicationHandler applicationHandler;

        // Noon task
        TaskCancelWrap noonTask;
        bool lastScheduledCheckFailedToSetWallpaper;

        public Wallpaper(ApplicationHandler applicationHandler)
        {
            this.applicationHandler = applicationHandler;
        }

        public async Task<bool> RunPostProcessAndSetWallpaperAllScreens(ImgWrap imgWrap)
        {
            applicationHandler.State.SetStatePostProcessing();
            // Get current screens
            var currentScreensConnectedById = Screen.FromCurrentConnected();

            // Create postprocessed images
            await imgWrap.createPostProcessedImages(currentScreensConnectedById, applicationHandler.Prefs.PostProcesses);

            // Set wallpapers
            Dictionary<Screen, string> postProcessedImageUrlByScreen = imgWrap.ImgLocalPostProcessedUrlsByScreenId.ToDictionary(
                // Key is screen id
                screenKV => currentScreensConnectedById[screenKV.Key],
                // Value is post processed url
                screenKV => screenKV.Value
                );
            bool retVar = await SetWallpaperAllScreens(postProcessedImageUrlByScreen);
            applicationHandler.State.UnsetStatePostProcessing();
            return retVar;
        }

        public async Task<bool> SetWallpaperAllScreens(Dictionary<Screen, string> urlsByScreen)
        {
            Object retObj = await General.SetWallpaper(urlsByScreen);
            return (bool)retObj;
        }

        /// <summary>
        /// Should only be used for reset to non-astro wall wallpaper
        /// </summary>
        /// <param name="urlsByScreen"></param>
        /// <returns></returns>
        public async Task<bool> SetWallpaperAllScreens(string path)
        {
            // Get current screens
            var currentScreensConnectedById = Screen.FromCurrentConnected();

            // Set wallpapers
            Dictionary<Screen, string> postProcessedImageUrlByScreen = currentScreensConnectedById.ToDictionary(
                // Key is set to Screen (former value)
                screenKV => screenKV.Value,
                // Value is set to path from args
                screenKV => path
                );
            return await SetWallpaperAllScreens(postProcessedImageUrlByScreen);
        }
        //public async Task<bool> SetWallpaperAllScreens(ImgWrap iw)
        //{
        //    if (iw.FullResIsLoaded())
        //    {
        //        Object retObj = await General.SetWallpaper(iw.ImgLocalUrl, true); ;
        //        return (bool)retObj;
        //    }
        //    else return false;
        //}

        //public void SetWallpaperMainScreen(string url)
        //{

        //    General.SetWallpaper();
        //}
        public void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded())
                General.SetWallpaper(Screen.Main(), iw.ImgLocalPreviewUrl);
        }

        public void ResetWallpaper()
        {
            if (applicationHandler.Prefs.hasAstroWall())
            {
                // Postprocessed should already be created, reset to those
                var currentConnectedScreensById = Screen.FromCurrentConnected();

                // Set wallpapers
                Dictionary<Screen, string> postProcessedImageUrlByScreen =
                    applicationHandler.Prefs.CurrentAstroWallpaper
                    .ImgLocalPostProcessedUrlsByScreenId
                    .ToDictionary(
                    // Key is screen id
                    screenKV => currentConnectedScreensById[screenKV.Key],
                    // Value is post processed url
                    screenKV => screenKV.Value
                    );

                SetWallpaperAllScreens(postProcessedImageUrlByScreen);
            }
            else
            {
                SetWallpaperAllScreens(applicationHandler.Prefs.CurrentPathToNonAstroWallpaper);
            }
        }

        public void launchPostProcessWindow()
        {
            applicationHandler.AppDelegate.launchPostProcessPrompt(applicationHandler.Prefs, callbackWithNewPostProcessSettings);
        }

        public void callbackWithNewPostProcessSettings(Preferences.AddText newAtFromDialogue)
        {
            Console.WriteLine("setting new add text to: " + newAtFromDialogue.isEnabled);
            this.applicationHandler.Prefs.AddTextPostProcess = newAtFromDialogue;
        }

        public void registerWakeHandler()
        {
            Console.WriteLine("Registering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.RegisterWallpaperWakeHandler(this.wakeCallback);
        }

        public void unregisterWakeHandler()
        {
            Console.WriteLine("Unregistering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterWallpaperLoginHandler();
        }


        public async void wakeCallback(NSNotification not)
        // TODO should not have macos data type in business layer
        {
            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                Console.WriteLine("Wake, consider checking for new pics, now is: " + DateTime.Now);
                Console.WriteLine("Next scheduled check: " + applicationHandler.Prefs.NextScheduledCheck);
                if (lastScheduledCheckFailedToSetWallpaper)
                {
                    Console.WriteLine("Last online check was probably during sleep, retrying to set wallpapers in 10 sec");
                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    Task.Run(async () =>
                    {
                        await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.db.ImgWrapList[0]);
                    });
                }
                else if (applicationHandler.Prefs.NextScheduledCheck < DateTime.Now)
                {
                    Console.WriteLine("Scheduled check has passed, performing check now, delaying set wall by 10 sec");
                    applicationHandler.State.SetStateDownloading("Checking for new pics...");
                    await applicationHandler.checkForNewPics();
                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.db.ImgWrapList[0]);
                    createNoonCheck();
                    applicationHandler.State.UnsetStateDownloading();
                }
                else
                {
                    Console.WriteLine("Scheduled check has not yet arrived, recalibrating timer postwake");
                    createNoonCheck();
                }
            }
            else cancelNoonCheck();
        }

        public async void createNoonCheck()
        {
            // Dispose old task (when the timer is reached it should cancel
            // bc. of token)
            if (noonTask != null) noonTask.cts.Cancel();

            TaskCancelWrap newTCW = new TaskCancelWrap();
            newTCW.task = Task.Run(async () =>
            {
                Console.WriteLine("Noontask started (before delay)");
                DateTime now = DateTime.Now;
                DateTime nowPlusOneDay = DateTime.Now.AddDays(1);
                DateTime tomorrowNoon = new DateTime(nowPlusOneDay.Year, nowPlusOneDay.Month, nowPlusOneDay.Day, 12, 0, 0);
                // Debugging line
                // DateTime tomorrowNoon = now.AddMilliseconds(120000);
                applicationHandler.Prefs.NextScheduledCheck = tomorrowNoon;
                int diffMSuntilTomorrowNoon = (int)tomorrowNoon.Subtract(now).TotalMilliseconds;
                Console.WriteLine("MS diff until tomorrow noon: " + diffMSuntilTomorrowNoon);
                await Task.Delay(diffMSuntilTomorrowNoon);
                Console.WriteLine("Noon task token status: " + newTCW.token.IsCancellationRequested);
                if (!newTCW.token.IsCancellationRequested) noonCallback();
            });
            noonTask = newTCW;
            try
            {
                await newTCW.task;
            }
            catch (Exception ex)
            {
                Console.WriteLine("task exception: " + ex.Message);
            }
        }


        public void cancelNoonCheck()
        {
            Console.WriteLine("cancelling task to check new pics at noon");
            noonTask.cts.Cancel();
            noonTask = null;
        }

        public async void noonCallback()
        {
            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                Console.WriteLine("Noon callback, checking for new pics");
                applicationHandler.State.SetStateDownloading("Checking for new pics...");
                await applicationHandler.checkForNewPics();
                bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.db.ImgWrapList[0]);
                if (!successChangeWallpaper) lastScheduledCheckFailedToSetWallpaper = true;

                // Recreate nooncheck tomorrow
                createNoonCheck();

                // Back to idle (set to download in earlier subfunction)
                applicationHandler.State.UnsetStateDownloading();
            }
        }

        public void SetDailyCheckToNewest(bool enabled)
        {
            if (enabled)
            // User checks of check for newest daily
            {
                this.applicationHandler.Prefs.DailyCheck = DailyCheckEnum.Newest;
                registerWakeHandler();
            }
            else
            //
            {
                this.applicationHandler.Prefs.DailyCheck = DailyCheckEnum.None;
                unregisterWakeHandler();
            }
        }
    }
}

