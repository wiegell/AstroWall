using System;
using System.IO;

namespace AstroWall.BusinessLayer
{
    public class Logging
    {

        // Formatting
        private static string dateFormat = "dd/MM-yy--HH:mm:ss";

        //Singleton
        private static volatile Logging instance;
        private static object syncRoot = new object();
        public static Logging Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logging();
                    }
                }

                return instance;
            }
        }

        // Writer
        private StreamWriter sw;

        public Logging()
        {
            pruneLogFile();
            sw = File.AppendText(General.getLogPath());
        }

        public static Action<string> GetLogger(string caller, bool isError = false)
        {
            return (string log) =>
                    {
                        lock (syncRoot)
                        {
                            string logStr = (isError ? "ERROR: " : "") + DateTime.Now.ToString(dateFormat) + ", " + caller + ": " + log;
                            Logging.Instance.sw.WriteLine(logStr);
                            Logging.Instance.sw.Flush();
                            Console.WriteLine(logStr);
                        }
                    };
        }

        private void pruneLogFile()
        {
            string path = General.getLogPath();
            if (File.Exists(path))
            {
            long size = (new FileInfo(path)).Length;
            if (size > 5000000) File.Delete(path);
            }
        }
    }
}

