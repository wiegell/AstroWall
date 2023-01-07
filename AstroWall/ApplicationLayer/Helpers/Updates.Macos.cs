using System;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    /// <summary>
    /// Helper class with static OS specific functions related to updating.
    /// </summary>
    public class Updates
    {
        // Has no state, only static functions.
        private Updates()
        {
        }

        /// <summary>
        /// Runs pkg update, overwrites current open application!.
        /// </summary>
        internal static void RunPKGUpdate(string pathToPkg)
        {
            NSTask nstask = new NSTask();
            nstask.LaunchPath = "/bin/bash";
            nstask.Arguments = new string[]
            {
                "-c",
                "installer -pkg " + pathToPkg + " -target CurrentUserHomeDirectory",
            };
            nstask.Launch();
            nstask.WaitUntilExit();
        }

        /// <summary>
        /// Shows modal that tells the user, that no updates are pending.
        /// </summary>
        /// <param name="curversion"></param>
        internal static void AlertNoUpdates(string curversion)
        {
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = $"You are up to date at version v{curversion}",
                MessageText = "Up to date",
            };
            alert.RunModal();
        }
    }
}