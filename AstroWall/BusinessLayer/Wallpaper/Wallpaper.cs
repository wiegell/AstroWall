using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using AstroWall.ApplicationLayer.Helpers;
using AstroWall.BusinessLayer.Preferences;
using Foundation;
using Newtonsoft.Json;

namespace AstroWall.BusinessLayer.Wallpaper
{
    /// <summary>
    /// Wallpaper related operations.
    /// </summary>
    internal class Wallpaper
    {
        // Log
        private static Action<string> log = Logging.GetLogger("Wallpaper");
        private static Action<string> logError = Logging.GetLogger("Wallpaper", true);

        // Refs
        private ApplicationHandler applicationHandler;

        // Noon task
        private TaskCancelWrap noonTask;
        private bool lastScheduledCheckFailedToSetWallpaper;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wallpaper"/> class.
        /// </summary>
        /// <param name="applicationHandler">Ref back to parent applicationHandler.</param>
        internal Wallpaper(ApplicationHandler applicationHandler)
        {
            this.applicationHandler = applicationHandler;
        }

        /// <summary>
        /// Sets wallpaper on supplied screens.
        /// </summary>
        /// <param name="urlsByScreen"></param>
        /// <returns>Bool true, if completed ok.</returns>
        internal static async Task<bool> SetWallpaperAllScreens(Dictionary<Screen, string> urlsByScreen)
        {
            object retObj = await General.SetWallpaper(urlsByScreen);
            return (bool)retObj;
        }

        /// <summary>
        /// Should only be used for reset to non-astro wall wallpaper.
        /// </summary>
        /// <returns>bool.</returns>
        internal static async Task<bool> SetWallpaperAllScreens(string path)
        {
            // Get current screens
            var currentScreensConnectedById = Screen.FromCurrentConnected();

            // Set wallpapers
            Dictionary<Screen, string> postProcessedImageUrlByScreen = currentScreensConnectedById.ToDictionary(
                screenKV => screenKV.Value, // Key is set to Screen (former value)
                screenKV => path); // Value is set to path from args
            return await SetWallpaperAllScreens(postProcessedImageUrlByScreen);
        }

        /// <summary>
        /// Unregisters wake handler.
        /// </summary>
        internal static void UnregisterWakeHandler()
        {
            log("Unregistering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.UnRegisterWallpaperLoginHandler();
        }

        /// <summary>
        /// Sets preview wallpaper.
        /// </summary>
        /// <param name="iw">The preview image (low res) of iw is used.</param>
        internal static void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded)
            {
                General.SetWallpaper(Screen.MainScreen(), iw.ImgLocalPreviewUrl);
            }
        }

        /// <summary>
        /// Runs postprocessing on all wallpapers for all screens and sets the wallpapers.
        /// </summary>
        /// <param name="imgWrap"></param>
        /// <returns>Task that completes with TRUE when wallpapers are set. Or FALSE if an error occurred.</returns>
        internal async Task<bool> RunPostProcessAndSetWallpaperAllScreens(ImgWrap imgWrap)
        {
            if (imgWrap == null || !imgWrap.ImgIsGettable)
            {
                return false;
            }

            try
            {
                // Task wrap to run post process on non-ui thread
                return await Task.Run(async () =>
                {
                    applicationHandler.State.SetStatePostProcessing();

                    // Get current screens
                    var currentScreensConnectedById = Screen.FromCurrentConnected();

                    // Create postprocessed images
                    await imgWrap.CreatePostProcessedImages(currentScreensConnectedById, applicationHandler.Prefs.PostProcesses);

                    // Set wallpapers
                    Dictionary<Screen, string> postProcessedImageUrlByScreen = imgWrap.ImgLocalPostProcessedUrlsByScreenId.ToDictionary(
                    screenKV => currentScreensConnectedById[screenKV.Key], // Key is screen id
                    screenKV => screenKV.Value); // Value is post processed url
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

        /// <summary>
        /// Run and forget version of RunPostProcessAndSetWallpaperAllScreens.
        /// </summary>
        internal void RunPostProcessAndSetWallpaperAllScreensUnobserved(ImgWrap imgWrap)
        {
            Task touter = Task.Run(async () =>
            {
                await RunPostProcessAndSetWallpaperAllScreens(imgWrap);
            });
        }

        /// <summary>
        /// Resets wallpaper to last used wallpaper. Used when exiting preview browsing.
        /// </summary>
        internal void ResetWallpaper()
        {
            if (applicationHandler.Prefs.HasAstroWall)
            {
                // Postprocessed should already be created, reset to those
                var currentConnectedScreensById = Screen.FromCurrentConnected();

                // Set wallpapers
                Dictionary<Screen, string> postProcessedImageUrlByScreen =
                    applicationHandler.Prefs.CurrentAstroWallpaper
                    .ImgLocalPostProcessedUrlsByScreenId
                    .ToDictionary(
                    screenKV => currentConnectedScreensById[screenKV.Key], // Key is screen id
                    screenKV => screenKV.Value); // Value is post processed url
                SetWallpaperAllScreens(postProcessedImageUrlByScreen);
            }
            else
            {
                SetWallpaperAllScreens(applicationHandler.Prefs.CurrentPathToNonAstroWallpaper);
            }
        }

        /// <summary>
        /// Launches post process window.
        /// </summary>
        internal void LaunchPostProcessWindow()
        {
            applicationHandler.AppDelegate.LaunchPostProcessPrompt(applicationHandler.Prefs, CallbackWithNewPostProcessSettings);
        }

        /// <summary>
        /// Callback that is called when exiting post process settings window.
        /// </summary>
        /// <param name="newAtFromDialogue">New AddText settings from user.</param>
        internal void CallbackWithNewPostProcessSettings(Preferences.AddTextPreference newAtFromDialogue)
        {
            log("setting new post processing prefs" + newAtFromDialogue.IsEnabled);
            this.applicationHandler.Prefs.AddTextPostProcess = newAtFromDialogue;
            log("rerunning postprocess");
            applicationHandler.Wallpaper.RunPostProcessAndSetWallpaperAllScreensUnobserved(applicationHandler.Prefs.CurrentAstroWallpaper);
        }

        /// <summary>
        /// Registers wake handler.
        /// </summary>
        internal void RegisterWakeHandler()
        {
            log("Registering wallpaper login handler");
            ApplicationLayer.SystemEvents.Instance.RegisterWallpaperWakeHandler(this.WakeCallback);
        }

        /// <summary>
        /// Checks if noon has passed and rechecks for new images if so.
        /// In every case the noon check timer is recalibrated / reset.
        /// </summary>
        internal async void WakeCallback(NSNotification not)
        {
            // TODO should not have macos data type in business layer

            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                log("Wake, consider checking for new pics");
                log("Next scheduled check: " + applicationHandler.Prefs.NextScheduledCheck.ToString(Logging.DateFormat, System.Globalization.CultureInfo.InvariantCulture));
                if (lastScheduledCheckFailedToSetWallpaper)
                {
                    log("Last online check was probably during sleep, retrying to set wallpapers in 10 sec");

                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    this.RunPostProcessAndSetWallpaperAllScreensUnobserved(applicationHandler.DB.Latest);
                }
                else if (applicationHandler.Prefs.NextScheduledCheck < DateTime.Now)
                {
                    log("Scheduled check has passed, performing check now, delaying set wall by 10 sec");
                    await applicationHandler.CheckForNewPics();

                    // Delay is to allow the user to sign in after wake
                    await Task.Delay(10000);
                    bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.DB.Latest);
                    CreateNoonCheck();
                }
                else
                {
                    log("Scheduled check has not yet arrived, recalibrating timer postwake");
                    CreateNoonCheck();
                }
            }
            else
            {
                CancelNoonCheck();
            }
        }

        /// <summary>
        /// Creates check for new images at noon.
        /// </summary>
        internal async void CreateNoonCheck()
        {
            // Dispose old task (when the timer is reached it should cancel
            // bc. of token)
            if (noonTask != null)
            {
                noonTask.Cts.Cancel();
            }

            TaskCancelWrap newTCW = new TaskCancelWrap();
            newTCW.Task = Task.Run(async () =>
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
                log("Noon task token status: " + newTCW.Token.IsCancellationRequested);
                if (!newTCW.Token.IsCancellationRequested)
                {
                    NoonCallback();
                }
            });
            noonTask = newTCW;
            try
            {
                await newTCW.Task;
            }
            catch (Exception ex)
            {
                log("task exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Cancel the noon check for new images.
        /// </summary>
        internal void CancelNoonCheck()
        {
            log("cancelling task to check new pics at noon");
            noonTask.Cts.Cancel();
            noonTask = null;
        }

        /// <summary>
        /// Callback that is run on noon  (the actual check for new images).
        /// </summary>
        internal async void NoonCallback()
        {
            // Double check, if prefs have changed since callback set
            if (applicationHandler.Prefs.DailyCheck == DailyCheckEnum.Newest)
            {
                log("Noon callback, checking for new pics");
                applicationHandler.State.SetStateDownloading("Checking for new pics...");
                await applicationHandler.CheckForNewPics();
                bool successChangeWallpaper = await this.RunPostProcessAndSetWallpaperAllScreens(applicationHandler.DB.Latest);
                if (!successChangeWallpaper)
                {
                    lastScheduledCheckFailedToSetWallpaper = true;
                }

                // Recreate nooncheck tomorrow
                CreateNoonCheck();

                // Back to idle (set to download in earlier subfunction)
                applicationHandler.State.UnsetStateDownloading();
            }
        }

        /// <summary>
        /// Set daily check preference to check for newest image.
        /// </summary>
        /// <param name="enabled"></param>
        internal void SetDailyCheckToNewest(bool enabled)
        {
            if (enabled)
            {
                this.applicationHandler.Prefs.DailyCheck = DailyCheckEnum.Newest;
                RegisterWakeHandler();
            }
            else
            {
                this.applicationHandler.Prefs.DailyCheck = DailyCheckEnum.None;
                UnregisterWakeHandler();
            }
        }

        /// <summary>
        /// Nested class to cancel task.
        /// </summary>
        private class TaskCancelWrap : IDisposable
        {
            internal TaskCancelWrap()
            {
                Token = Cts.Token;
            }

            internal Task Task { get; set; }

            internal CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();

            internal CancellationToken Token { get; set; }

            public void Dispose()
            {
                Cts.Dispose();
                Task.Dispose();
            }
        }
    }
}