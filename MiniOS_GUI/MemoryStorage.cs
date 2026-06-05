using System;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public static class MemoryStorage
    {
        private static string memoryFile = Application.StartupPath + "\\MiniOS_Memory.dat";

        // Base Memory values
        public static int TotalRAM = 4096;  // 4MB in KB
        public static int KernelMem = 256;
        public static int ShellMem = 128;
        public static int TaskMem = 64;
        public static int CalculatorMem = 64;
        public static int TimeMem = 32;

        // File System paths
        public static string DocumentsPath = Path.Combine(Application.StartupPath, "MiniOS_Documents");
        public static string DownloadsPath = Path.Combine(Application.StartupPath, "MiniOS_Downloads");
        public static string PrivatePath = Path.Combine(Application.StartupPath, "MiniOS_Private");

        // Process status
        public static bool CalculatorRunning = false;
        public static bool TimeRunning = false;
        public static string CurrentUser = "admin";

        // Cached folder sizes
        private static long documentsSize = 0;
        private static long downloadsSize = 0;
        private static long privateSize = 0;

        static MemoryStorage()
        {
            if (!Directory.Exists(DocumentsPath))
                Directory.CreateDirectory(DocumentsPath);
            if (!Directory.Exists(DownloadsPath))
                Directory.CreateDirectory(DownloadsPath);
            if (!Directory.Exists(PrivatePath))
                Directory.CreateDirectory(PrivatePath);

            LoadMemory();
            UpdateFolderSizes();
        }

        public static void UpdateFolderSizes()
        {
            documentsSize = GetFolderSize(DocumentsPath);
            downloadsSize = GetFolderSize(DownloadsPath);
            privateSize = GetFolderSize(PrivatePath);
        }

        private static long GetFolderSize(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    FileInfo info = new FileInfo(file);
                    size += info.Length;
                }
            }
            catch { }
            return size;
        }

        public static long GetTotalFileStorage()
        {
            return documentsSize + downloadsSize + privateSize;
        }

        public static string GetFormattedFileStorage()
        {
            long total = GetTotalFileStorage();
            return FormatBytes(total);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)) + " MB";
            return (bytes / (1024 * 1024 * 1024)) + " GB";
        }

        public static int GetFileStorageInKB()
        {
            return (int)(GetTotalFileStorage() / 1024);
        }

        public static int GetUsedRAM()
        {
            int used = KernelMem + ShellMem + TaskMem;
            if (CalculatorRunning) used += CalculatorMem;
            if (TimeRunning) used += TimeMem;

            // Add file storage to RAM directly
            int fileStorageKB = GetFileStorageInKB();
            used += fileStorageKB;

            return Math.Min(used, TotalRAM);
        }

        public static int GetFreeRAM()
        {
            return TotalRAM - GetUsedRAM();
        }

        public static int GetPercentage()
        {
            return (GetUsedRAM() * 100) / TotalRAM;
        }

        public static void SaveMemory()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(memoryFile))
                {
                    sw.WriteLine($"# NOVA-OS Memory Storage");
                    sw.WriteLine($"# Last Updated: {DateTime.Now}");
                    sw.WriteLine($"TOTAL_RAM={TotalRAM}");
                    sw.WriteLine($"KERNEL={KernelMem}");
                    sw.WriteLine($"SHELL={ShellMem}");
                    sw.WriteLine($"TASK_MANAGER={TaskMem}");
                    sw.WriteLine($"CALCULATOR={CalculatorMem}");
                    sw.WriteLine($"TIME_SERVICE={TimeMem}");
                    sw.WriteLine($"CALCULATOR_RUNNING={CalculatorRunning}");
                    sw.WriteLine($"TIME_RUNNING={TimeRunning}");
                    sw.WriteLine($"CURRENT_USER={CurrentUser}");
                    sw.WriteLine($"DOCUMENTS_SIZE={documentsSize}");
                    sw.WriteLine($"DOWNLOADS_SIZE={downloadsSize}");
                    sw.WriteLine($"PRIVATE_SIZE={privateSize}");
                }
            }
            catch { }
        }

        public static void LoadMemory()
        {
            if (!File.Exists(memoryFile)) return;

            try
            {
                using (StreamReader sr = new StreamReader(memoryFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#")) continue;

                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "TOTAL_RAM": TotalRAM = int.Parse(parts[1]); break;
                                case "KERNEL": KernelMem = int.Parse(parts[1]); break;
                                case "SHELL": ShellMem = int.Parse(parts[1]); break;
                                case "TASK_MANAGER": TaskMem = int.Parse(parts[1]); break;
                                case "CALCULATOR": CalculatorMem = int.Parse(parts[1]); break;
                                case "TIME_SERVICE": TimeMem = int.Parse(parts[1]); break;
                                case "CALCULATOR_RUNNING": CalculatorRunning = bool.Parse(parts[1]); break;
                                case "TIME_RUNNING": TimeRunning = bool.Parse(parts[1]); break;
                                case "CURRENT_USER": CurrentUser = parts[1]; break;
                                case "DOCUMENTS_SIZE": documentsSize = long.Parse(parts[1]); break;
                                case "DOWNLOADS_SIZE": downloadsSize = long.Parse(parts[1]); break;
                                case "PRIVATE_SIZE": privateSize = long.Parse(parts[1]); break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static void StartCalculator()
        {
            CalculatorRunning = true;
            SaveMemory();
            SystemLogForm.WriteLog("PROCESS", CurrentUser, "Calculator Started", $"Memory allocated: {CalculatorMem}KB", "SUCCESS");
        }

        public static void StopCalculator()
        {
            CalculatorRunning = false;
            SaveMemory();
            SystemLogForm.WriteLog("PROCESS", CurrentUser, "Calculator Stopped", $"Memory freed: {CalculatorMem}KB", "SUCCESS");
        }

        public static void StartTimeService()
        {
            TimeRunning = true;
            SaveMemory();
        }

        public static void StopTimeService()
        {
            TimeRunning = false;
            SaveMemory();
        }

        public static void RefreshFileStorage()
        {
            UpdateFolderSizes();
            SaveMemory();
        }

        public static string GetDocumentsSize() { return FormatBytes(documentsSize); }
        public static string GetDownloadsSize() { return FormatBytes(downloadsSize); }
        public static string GetPrivateSize() { return FormatBytes(privateSize); }
    }
}