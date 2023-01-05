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

    internal class Wallpaper
    {

        // Nested utility classes
        private class TaskCancelWrap : IDisposable
        {
            public Task task;
            public CancellationTokenSource cts = new CancellationTokenSource();
            public CancellationToken token;

            public TaskCancelWrap()
            {
                token = cts.Token;
            }

            public void Dispose()
            {
                cts.Dispose();
                task.Dispose();
            }
        }

        // Refs
        ApplicationHandler applicationHandler;

        // Noon task
        TaskCancelWrap noonTask;
        bool lastScheduledCheckFailedToSetWallpaper;

        // Log
        private static Action<string> log = Logging.GetLogger("Wallpaper");
        private static Action<string> logError = Logging.GetLogger("Wallpaper", true);

        public Wallpaper(ApplicationHandler applicationHandler)
        {
            this.applicationHandler = applicationHandler;
        }

        public async Task<bool> RunPostProcessAndSetWallpaperAllScreens(ImgWrap imgWrap)
        {
            if (imgWrap == null || !imgWrap.ImgIsGettable) return false;
            try
            {
                // Task wrap to run on non-ui thread
                return await Task.Run(async () =>
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
                    applicationHandler.Prefs.CurrentAstroWallpaper = imgWrap;

                    applicationHandler.State.UnsetStatePostProcessing();
                    return retVar;

                });
            }
            catch (Exception ex)
            {
                General.RunOnUIThread(() =>
                {
                    logError("Exception in postProcess task on thread: " + Environment.CurrentManagedThreadId);
                    throw ex;
                });

                throw;
            }
        }

        public void RunPostProcessAndSetWallpaperAllScreensUnobserved(ImgWrap imgWrap)
        {
            Task touter = Task.Run(async () =>
            {
                await RunPostProcessAndSetWallpaperAllScreens(imgWrap);
            });
        }

        public static async Task<bool> SetWallpaperAllScreens(Dictionary<Screen, string> urlsByScreen)
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
        public static void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded())
                General.SetWallpaper(Screen.MainScreen(), iw.ImgLocalPreviewUrl);
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
            applicationHandler.AppDelegate.LaunchPostProcessPrompt(applicationHandler.Prefs, callbackWithNewPostProcessSettings);
        }

        public void callbackWithNewPostProcessSettings(Preferences.AddText newAtFromDialogue)
        {
            log("setting new post processing prefs" + newAtFromDialogue.isEnabled);
            this.applicationHandler.Prefs.AddTextPostProcess = newAtFromDialogue;
            log("rerunning postprocess");
            applicationHandler.Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(applicationHandler.Prefs.CurrentAstroWallpaper);
        }

        public void registerWakeHandler()
        {
            log("Registering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.RegisterWallpaperWakeHandler(this.wakeCallback);
        }

        public static void unregisterWakeHandler()
        {
            log("Unregistering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterWallpaperLoginHandler();
        }


        public async void wakeCallback(NSNotification not)
        // TODO should not have macos data type in business layer
        {
            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                log("Wake, consider checking for new pics");
                log("Next scheduled check: " + applicationHandler.Prefs.NextScheduledCheck.ToString(Logging.dateFormat, System.Globalization.CultureInfo.InvariantCulture));
                if (lastScheduledCheckFailedToSetWallpaper)
                {
                    log("Last online check was probably during sleep, retrying to set wallpapers in 10 sec");
                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    this.RunPostProcessAndSetWallpaperAllScreensUnobserved(applicationHandler.db.Latest);
                }
                else if (applicationHandler.Prefs.NextScheduledCheck < DateTime.Now)
                {
                    log("Scheduled check has passed, performing check now, delaying set wall by 10 sec");
                    await applicationHandler.checkForNewPics();
                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.db.Latest);
                    createNoonCheck();
                }
                else
                {
                    log("Scheduled check has not yet arrived, recalibrating timer postwake");
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
                DateTime now = DateTime.Now;
                DateTime noonToday = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

                bool shouldSetCheckTomorrow = now > noonToday;
                log("Setting next noon check to " + (shouldSetCheckTomorrow ? "tomorrow" : "today"));

                DateTime dayToSetCheck = now.AddDays(shouldSetCheckTomorrow ? 1 : 0);
                DateTime nextNoon = new DateTime(dayToSetCheck.Year, dayToSetCheck.Month, dayToSetCheck.Day, 12, 0, 0);
                // Debugging line
                // DateTime tomorrowNoon = now.AddMilliseconds(120000);
                applicationHandler.Prefs.NextScheduledCheck = nextNoon;
                int diffMSuntilTomorrowNoon = (int)nextNoon.Subtract(now).TotalMilliseconds;
                log("MS diff until next noon: " + diffMSuntilTomorrowNoon);
                await Task.Delay(diffMSuntilTomorrowNoon);
                log("Noon task token status: " + newTCW.token.IsCancellationRequested);
                if (!newTCW.token.IsCancellationRequested) noonCallback();
            });
            noonTask = newTCW;
            try
            {
                await newTCW.task;
            }
            catch (Exception ex)
            {
                log("task exception: " + ex.Message);
            }
        }


        public void cancelNoonCheck()
        {
            log("cancelling task to check new pics at noon");
            noonTask.cts.Cancel();
            noonTask = null;
        }

        public async void noonCallback()
        {
            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                log("Noon callback, checking for new pics");
                applicationHandler.State.SetStateDownloading("Checking for new pics...");
                await applicationHandler.checkForNewPics();
                bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.db.Latest);
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

