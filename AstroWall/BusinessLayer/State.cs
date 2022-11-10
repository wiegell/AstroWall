using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using AppKit;

namespace AstroWall.BusinessLayer
{
    public enum stateEnum
    {
        Initializing,
        Idle,
        ResolvingUrl,
        UpdatingDatabase,
        DownloadingNewest,
        PostProcessing,
        SettingWallpaper,
        BrowsingWallpapers
    }

    public class State
    {
        // Refs
        ApplicationHandler applicationHandler;

        // Overall state
        public stateEnum state { get; private set; }

        // currentVersionString is the long tag from git including commit hash  
        public State(ApplicationHandler applicationHandlerArg, string currentVersionString)
        {
            this.applicationHandler = applicationHandlerArg;
        }

        //public void setPrefs(Preferences prefsArg)
        //{
        //    this.Prefs = prefsArg;
        //}

        public void SetStateInitializing()
        {
            Console.WriteLine("State: Initializing");
            state = stateEnum.Initializing;
            applicationHandler.MenuHandler.EnableStatusIcon();
            applicationHandler.MenuHandler.DisableAllItems();
            applicationHandler.MenuHandler.SetTitleInitialising();
            applicationHandler.MenuHandler.RunDownloadIconAnimation();
        }

        public void SetStateChoosePrefs()
        {
            Console.WriteLine("State: Choose prefs");
            applicationHandler.MenuHandler.disableStatusIcon();
        }


        public void SetStateIdle()
        {
            Console.WriteLine("Setting state to idle:");
            applicationHandler.MenuHandler.EnableStatusIcon();
            applicationHandler.MenuHandler.SetIconToDefault();
            state = stateEnum.Idle;
            applicationHandler.MenuHandler.HideState();

        }

        public void setStateBrowsing()
        {
            Console.WriteLine("Setting state browsing");
            state = BusinessLayer.stateEnum.BrowsingWallpapers;
        }

        public void SetLaunchAgentToReflectPrefs()
        {
            if (applicationHandler
                .Prefs.runAtLogin) GeneralHelpers.SetAsLaunchAgent();
            else GeneralHelpers.RemoveLaunchAgent();
        }

    }
}

