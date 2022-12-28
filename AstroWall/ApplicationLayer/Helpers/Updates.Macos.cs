using System;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    public class Updates
    {

        // TODO is this needed anymore? Only static functions
        //Singleton
        private static volatile Updates instance;
        private static object syncRoot = new object();
        public static Updates Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Updates();
                    }
                }

                return instance;
            }
        }

        private Updates()
        {
        }

        public static void RunPKGUpdate(string pathToPkg)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
            "installer -pkg "+pathToPkg+" -target CurrentUserHomeDirectory"
            };
            //+" "
            nstask.Launch();
            nstask.WaitUntilExit();
        }

        public static void AlertNoUpdates(string curversion)
        {
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = $"You are up to date at version v{curversion}",
                MessageText = "Up to date"
            };
            alert.RunModal();
        }
    }
}

