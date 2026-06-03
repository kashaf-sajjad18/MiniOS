using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class MemoryForm : Form
    {
        private ProgressBar ramBar;
        private Label lblTotal, lblUsed, lblFree, lblPercentage;
        private Label lblKernel, lblShell, lblCalculator, lblTime, lblTask;
        private Label lblDocuments, lblDownloads, lblPrivate, lblTotalStorage;
        private Timer refreshTimer;
        private Panel ramPanel;  // FIX: Added ramPanel as class field
        private Panel storagePanel;  // FIX: Added storagePanel as class field

        private int totalRAM = 4096;
        private int kernelMem = 256;
        private int shellMem = 128;
        private int taskMem = 64;
        private int calculatorMem = 64;
        private int timeMem = 32;

        private bool calculatorRunning = false;
        private bool timeRunning = false;

        private string documentsPath;
        private string downloadsPath;
        private string privatePath;

        private long documentsSize = 0;
        private long downloadsSize = 0;
        private long privateSize = 0;

        public MemoryForm()
        {
            documentsPath = Path.Combine(Application.StartupPath, "MiniOS_Documents");
            downloadsPath = Path.Combine(Application.StartupPath, "MiniOS_Downloads");
            privatePath = Path.Combine(Application.StartupPath, "MiniOS_Private");

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);
            if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);
            if (!Directory.Exists(privatePath)) Directory.CreateDirectory(privatePath);

            BuildUI();
            StartRefreshTimer();
            UpdateMemoryDisplay();
        }

        private void BuildUI()
        {
            this.Text = "MiniOS Memory Manager";
            this.Size = new Size(750, 580);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label title = new Label();
            title.Text = "MEMORY MANAGER";
            title.ForeColor = Color.DeepSkyBlue;
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.Location = new Point(20, 15);
            title.AutoSize = true;

            Label ramLabel = new Label();
            ramLabel.Text = "RAM USAGE";
            ramLabel.ForeColor = Color.White;
            ramLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            ramLabel.Location = new Point(30, 60);
            ramLabel.AutoSize = true;

            ramBar = new ProgressBar();
            ramBar.Location = new Point(30, 90);
            ramBar.Size = new Size(680, 35);
            ramBar.Maximum = totalRAM;

            lblPercentage = new Label();
            lblPercentage.ForeColor = Color.Lime;
            lblPercentage.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblPercentage.Location = new Point(30, 130);
            lblPercentage.AutoSize = true;

            // RAM Panel
            ramPanel = new Panel();
            ramPanel.Location = new Point(30, 160);
            ramPanel.Size = new Size(680, 180);
            ramPanel.BackColor = Color.FromArgb(27, 31, 39);
            ramPanel.BorderStyle = BorderStyle.FixedSingle;

            lblTotal = CreateDetailLabel("TOTAL RAM:", $"{totalRAM} KB", 15, 15, ramPanel);
            lblUsed = CreateDetailLabel("USED RAM:", "", 15, 45, ramPanel);
            lblFree = CreateDetailLabel("FREE RAM:", "", 15, 75, ramPanel);

            Label divider1 = new Label();
            divider1.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
            divider1.ForeColor = Color.Gray;
            divider1.Location = new Point(15, 100);
            divider1.AutoSize = true;
            ramPanel.Controls.Add(divider1);

            lblKernel = CreateDetailLabel("Kernel:", $"{kernelMem} KB", 15, 125, ramPanel);
            lblShell = CreateDetailLabel("Shell:", $"{shellMem} KB", 15, 150, ramPanel);
            lblCalculator = CreateDetailLabel("Calculator:", "", 320, 15, ramPanel);
            lblTime = CreateDetailLabel("Time Service:", "", 320, 45, ramPanel);
            lblTask = CreateDetailLabel("Task Manager:", $"{taskMem} KB", 320, 75, ramPanel);

            // Storage Panel
            storagePanel = new Panel();
            storagePanel.Location = new Point(30, 355);
            storagePanel.Size = new Size(680, 150);
            storagePanel.BackColor = Color.FromArgb(27, 31, 39);
            storagePanel.BorderStyle = BorderStyle.FixedSingle;

            Label storageTitle = new Label();
            storageTitle.Text = "📁 FILE STORAGE";
            storageTitle.ForeColor = Color.DeepSkyBlue;
            storageTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            storageTitle.Location = new Point(15, 10);
            storageTitle.AutoSize = true;
            storagePanel.Controls.Add(storageTitle);

            lblTotalStorage = CreateDetailLabel("Total Storage:", "", 15, 35, storagePanel);
            lblDocuments = CreateDetailLabel("📄 Documents:", "", 15, 60, storagePanel);
            lblDownloads = CreateDetailLabel("⬇ Downloads:", "", 15, 85, storagePanel);
            lblPrivate = CreateDetailLabel("🔒 Private:", "", 15, 110, storagePanel);

            Button btnRefresh = new Button();
            btnRefresh.Text = "REFRESH";
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Location = new Point(30, 520);
            btnRefresh.BackColor = Color.FromArgb(35, 42, 52);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += (s, e) => UpdateMemoryDisplay();

            Label info = new Label();
            info.Text = "💾 Auto-refresh every 2 seconds | File sizes update in real-time";
            info.ForeColor = Color.Gray;
            info.Font = new Font("Segoe UI", 8);
            info.Location = new Point(30, 555);
            info.AutoSize = true;

            Controls.Add(title);
            Controls.Add(ramLabel);
            Controls.Add(ramBar);
            Controls.Add(lblPercentage);
            Controls.Add(ramPanel);
            Controls.Add(storagePanel);
            Controls.Add(btnRefresh);
            Controls.Add(info);
        }

        private Label CreateDetailLabel(string name, string value, int x, int y, Panel parent)
        {
            Label label = new Label();
            label.Text = $"{name,-15} {value}";
            label.ForeColor = Color.White;
            label.Font = new Font("Consolas", 10);
            label.Location = new Point(x, y);
            label.AutoSize = true;
            parent.Controls.Add(label);
            return label;
        }

        private void StartRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += (s, e) => UpdateMemoryDisplay();
            refreshTimer.Start();
        }

        private void UpdateFolderSizes()
        {
            documentsSize = GetFolderSize(documentsPath);
            downloadsSize = GetFolderSize(downloadsPath);
            privateSize = GetFolderSize(privatePath);
        }

        private long GetFolderSize(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    size += new FileInfo(file).Length;
                }
            }
            catch { }
            return size;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)) + " MB";
            return (bytes / (1024 * 1024 * 1024)) + " GB";
        }

        private int GetUsedRAM()
        {
            int used = kernelMem + shellMem + taskMem;
            if (calculatorRunning) used += calculatorMem;
            if (timeRunning) used += timeMem;

            long totalStorage = documentsSize + downloadsSize + privateSize;
            int storageKB = (int)(totalStorage / 1024);
            used += Math.Min(storageKB, 512);

            return Math.Min(used, totalRAM);
        }

        private void UpdateMemoryDisplay()
        {
            UpdateFolderSizes();

            int usedMem = GetUsedRAM();
            int freeMem = totalRAM - usedMem;
            int percentage = (usedMem * 100) / totalRAM;
            long totalStorage = documentsSize + downloadsSize + privateSize;

            ramBar.Value = usedMem;
            if (percentage < 50)
                ramBar.ForeColor = Color.Lime;
            else if (percentage < 80)
                ramBar.ForeColor = Color.Yellow;
            else
                ramBar.ForeColor = Color.Red;

            lblPercentage.Text = $"{percentage}% USED ({usedMem} KB / {totalRAM} KB)";
            lblUsed.Text = $"USED RAM:         {usedMem} KB";
            lblFree.Text = $"FREE RAM:         {freeMem} KB";

            string calcMem = calculatorRunning ? $"{calculatorMem} KB 🟢" : "0 KB 🔴";
            string timeMemStr = timeRunning ? $"{timeMem} KB 🟢" : "0 KB 🔴";
            lblCalculator.Text = $"Calculator:       {calcMem}";
            lblTime.Text = $"Time Service:     {timeMemStr}";

            lblTotalStorage.Text = $"Total Storage:    {FormatBytes(totalStorage)}";
            lblDocuments.Text = $"📄 Documents:     {FormatBytes(documentsSize)}";
            lblDownloads.Text = $"⬇ Downloads:     {FormatBytes(downloadsSize)}";
            lblPrivate.Text = $"🔒 Private:       {FormatBytes(privateSize)}";
        }

        public void SetCalculatorStatus(bool running)
        {
            calculatorRunning = running;
            UpdateMemoryDisplay();
        }

        public void SetTimeStatus(bool running)
        {
            timeRunning = running;
            UpdateMemoryDisplay();
        }
    }
}