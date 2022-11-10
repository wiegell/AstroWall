using System;
using AppKit;
using Foundation;

namespace AstroWall.ApplicationLayer
{
    public class UpdateHelpers
    {

        NSObject wakeHandlerObserver;

        public UpdateHelpers()
        {
        }

        public void RegisterWakeHandler(Action<NSNotification> ac)
        {
            wakeHandlerObserver =
            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(NSWorkspace.DidWakeNotification, ac);
        }

        public void UnRegisterWakeHandler()
        {
            NSWorkspace.SharedWorkspace.NotificationCenter.RemoveObserver(wakeHandlerObserver);
        }

        public void RunPKGUpdate(string pathToPkg)
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

        public void AlertNoUpdates(string curversion)
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

