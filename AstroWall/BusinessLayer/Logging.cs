using System;
using System.IO;

namespace AstroWall.BusinessLayer
{
    /// <summary>
    /// Custom logging class. Logs to txt file in "Astro folder"
    /// (see application layer / general helpers) as well as to console.
    /// </summary>
    internal class Logging
    {
        /// <summary>
        /// Date format in log.
        /// </summary>
        internal static readonly string DateFormat = "dd/MM-yy--HH:mm:ss";

        // Singleton related
        private static volatile Logging instance;
        private static object syncRoot = new object();

        // Writer
        private StreamWriter sw;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logging"/> class.
        /// Should only be available via singleton, therefore private.
        /// </summary>
        private Logging()
        {
            PruneLogFile();
            sw = File.AppendText(General.GetLogPath());
        }

        /// <summary>
        /// Gets singleton instance.
        /// </summary>
        internal static Logging Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Logging();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets logger.
        /// </summary>
        /// <param name="caller">appends the caller string (the calling class).</param>
        /// <param name="isError">flags as error if true.</param>
        /// <returns>Action that can take string arguments to log.</returns>
        internal static Action<string> GetLogger(string caller, bool isError = false)
        {
            return (string log) =>
                    {
                        lock (syncRoot)
                        {
                            string logStr = (isError ? "ERROR: " : string.Empty) + DateTime.Now.ToString(DateFormat, System.Globalization.CultureInfo.InvariantCulture) + ", " + caller + ": " + log;
                            Logging.Instance.sw.WriteLine(logStr);
                            Logging.Instance.sw.Flush();
                            Console.WriteLine(logStr);
                        }
                    };
        }

        /// <summary>
        /// Prunes the log file if too big.
        /// </summary>
        private static void PruneLogFile()
        {
            string path = General.GetLogPath();
            if (File.Exists(path))
            {
                long size = new FileInfo(path).Length;
                if (size > 5000000)
                {
                    File.Delete(path);
                }
            }
        }
    }
}
