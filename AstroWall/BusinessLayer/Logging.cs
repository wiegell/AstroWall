using System;
using System.IO;

namespace AstroWall.BusinessLayer
{
    public class Logging
    {

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
            sw = File.AppendText(General.getLogPath());
        }

        public static Action<string> GetLogger(string caller, bool isError = false)
        {
            return (string log) =>
                    {
                        lock (syncRoot)
                        {
                            string logStr = (isError ? "ERROR: " : "") + DateTime.Now.ToString("dd/MM-yy--HH:mm:ss") + ", " + caller + ": " + log;
                            Logging.Instance.sw.WriteLine(logStr);
                            Logging.Instance.sw.Flush();
                            Console.WriteLine(logStr);
                        }
                    };
        }
    }
}

