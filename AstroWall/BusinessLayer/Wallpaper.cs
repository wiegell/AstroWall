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
            MacOShelpers.SetWallpaper(url, true);
        }
        public void SetWallpaperAllScreens(ImgWrap iw)
        {
            if (iw.FullResIsLoaded())
                MacOShelpers.SetWallpaper(iw.ImgLocalUrl, true);
        }

        public void SetWallpaperMainScreen(string url)
        {
            MacOShelpers.SetWallpaper(url, false);
        }

        public void SetPreviewWallpaper(ImgWrap iw)
        {
            if (iw.PreviewIsLoaded())
                MacOShelpers.SetWallpaper(iw.ImgLocalPreviewUrl, false);
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

