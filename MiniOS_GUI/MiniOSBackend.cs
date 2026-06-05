using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    internal static class MiniOSBackend
    {
        private static string backendPath;

        public static string ResolveBackendPath()
        {
            if (!string.IsNullOrEmpty(backendPath) && File.Exists(backendPath))
                return backendPath;

            string[] candidates =
            {
                Path.Combine(Application.StartupPath, "NOVA-OS.exe"),
                Path.Combine(Application.StartupPath, "MiniOS_Backend.exe"),
                Path.Combine(Application.StartupPath, "MiniOS.exe"),
                @"D:\coal lab\MiniOS\MiniOS\Debug\NOVA-OS.exe",
                @"D:\coal lab\MiniOS\MiniOS\MiniOS_Backend.exe"
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    backendPath = candidate;
                    return backendPath;
                }
            }

            backendPath = string.Empty;
            return backendPath;
        }

        public static bool IsAvailable()
        {
            return !string.IsNullOrEmpty(ResolveBackendPath());
        }

        public static void LaunchModule(string args, bool waitForExit = false)
        {
            string path = ResolveBackendPath();
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();

                if (waitForExit)
                    process.WaitForExit(8000);
            }
            catch
            {
            }
        }

        public static string RunToString(string args, int timeoutMs = 3000)
        {
            string path = ResolveBackendPath();
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = string.Empty;
                System.Threading.Thread reader = new System.Threading.Thread(() =>
                {
                    try
                    {
                        output = process.StandardOutput.ReadToEnd();
                    }
                    catch
                    {
                    }
                });

                reader.IsBackground = true;
                reader.Start();

                bool exited = process.WaitForExit(timeoutMs);
                if (!exited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }
                }

                reader.Join(500);
                return output.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}