using System;
using System.Drawing;

namespace AstroWall.BusinessLayer
{
    public class Wallpaper
    {
        // Refs
        ApplicationHandler applicationHandler;

        public Wallpaper(ApplicationHandler applicationHandler)
        {
            this.applicationHandler = applicationHandler;
        }

        public void SetWallpaperAllScreens(string url)
        {
            General.SetWallpaper(url, true);
        }
        public void SetWallpaperAllScreens(ImgWrap iw)
        {
            if (iw.FullResIsLoaded())
                General.SetWallpaper(iw.ImgLocalUrl, true);
        }

        public void SetWallpaperMainScreen(string url)
        {
            General.SetWallpaper(url, false);
        }

        public void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded())
                General.SetWallpaper(iw.ImgLocalPreviewUrl, false);
        }

        public void ResetWallpaper()
        {
            SetWallpaperAllScreens(
          applicationHandler.Prefs.hasAstroWall() ?
            applicationHandler.Prefs.CurrentAstroWallpaper.ImgLocalUrl :
            applicationHandler.Prefs.CurrentPathToNonAstroWallpaper
            );
        }

        public void launchPostProcessWindow()
        {
            applicationHandler.AppDelegate.launchPostProcessPrompt(applicationHandler.Prefs, callbackWithNewPostProcessSettings);
        }

        public void callbackWithNewPostProcessSettings(Preferences newPrefs)
        {

        }
    }
}

