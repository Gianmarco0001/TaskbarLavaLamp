using System;
using System.IO;

namespace TaskbarLavaLamp
{
    internal static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskbarLavaLamp", "log.txt");

        static Logger()
        {
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }
            catch { }
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
                }
            }
            catch { }
        }

        public static string GetLogPath() => LogPath;

        public static void OpenLog()
        {
            try
            {
                if (File.Exists(LogPath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(LogPath) { UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
            }
            catch { }
        }
    }
}
