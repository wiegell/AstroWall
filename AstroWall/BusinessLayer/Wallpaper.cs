using System;
using System.Drawing;

namespace AstroWall.BusinessLayer
{
    public class Wallpaper
    {
        // Refs
        Preferences prefs;

        public Wallpaper(Preferences prefsRef)
        {
            prefs = prefsRef;
        }

        public void SetWallpaperAllScreens(string url)
        {
            GeneralHelpers.SetWallpaper(url, true);
        }
        public void SetWallpaperAllScreens(ImgWrap iw)
        {
            if (iw.FullResIsLoaded())
                GeneralHelpers.SetWallpaper(iw.ImgLocalUrl, true);
        }

        public void SetWallpaperMainScreen(string url)
        {
            GeneralHelpers.SetWallpaper(url, false);
        }

        public void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded())
                GeneralHelpers.SetWallpaper(iw.ImgLocalPreviewUrl, false);
        }

        public void ResetWallpaper()
        {
            SetWallpaperAllScreens(
            prefs.hasAstroWall() ?
            prefs.currentAstroWallpaper.ImgLocalUrl :
            prefs.currentPathToNonAstroWallpaper
            );
        }
    }
}

