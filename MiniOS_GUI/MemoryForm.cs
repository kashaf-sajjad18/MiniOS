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
        private Label lblWarning;
        private Timer refreshTimer;
        private Panel ramPanel, storagePanel;

        private int totalRAM = 4096;
        private int kernelMem = 256;
        private int shellMem = 128;
        private int taskMem = 64;
        private int calculatorMem = 64;
        private int timeMem = 32;

        private bool calculatorRunning = false;
        private bool timeRunning = false;

        private string documentsPath, downloadsPath, privatePath;
        private long documentsSize = 0, downloadsSize = 0, privateSize = 0;
        private long totalFileSize = 0;

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
            UpdateFromTaskManager();
        }

        private void BuildUI()
        {
            this.Text = "NOVA-OS Memory Manager";
            this.Size = new Size(800, 700);
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
            ramBar.Size = new Size(720, 35);
            ramBar.Maximum = totalRAM;

            lblPercentage = new Label();
            lblPercentage.ForeColor = Color.Lime;
            lblPercentage.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblPercentage.Location = new Point(30, 130);
            lblPercentage.AutoSize = true;

            lblWarning = new Label();
            lblWarning.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblWarning.Location = new Point(30, 155);
            lblWarning.AutoSize = true;

            ramPanel = new Panel();
            ramPanel.Location = new Point(30, 180);
            ramPanel.Size = new Size(720, 180);
            ramPanel.BackColor = Color.FromArgb(27, 31, 39);
            ramPanel.BorderStyle = BorderStyle.FixedSingle;

            lblTotal = CreateDetailLabel("TOTAL RAM:", "", 15, 15, ramPanel);
            lblUsed = CreateDetailLabel("USED RAM:", "", 15, 45, ramPanel);
            lblFree = CreateDetailLabel("FREE RAM:", "", 15, 75, ramPanel);

            Label divider1 = new Label();
            divider1.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
            divider1.ForeColor = Color.Gray;
            divider1.Location = new Point(15, 100);
            divider1.AutoSize = true;
            ramPanel.Controls.Add(divider1);

            lblKernel = CreateDetailLabel("Kernel:", "", 15, 125, ramPanel);
            lblShell = CreateDetailLabel("Shell:", "", 15, 150, ramPanel);
            lblCalculator = CreateDetailLabel("Calculator:", "", 320, 15, ramPanel);
            lblTime = CreateDetailLabel("Time Service:", "", 320, 45, ramPanel);
            lblTask = CreateDetailLabel("Task Manager:", "", 320, 75, ramPanel);

            storagePanel = new Panel();
            storagePanel.Location = new Point(30, 375);
            storagePanel.Size = new Size(720, 180);
            storagePanel.BackColor = Color.FromArgb(27, 31, 39);
            storagePanel.BorderStyle = BorderStyle.FixedSingle;

            Label storageTitle = new Label();
            storageTitle.Text = "📁 FILE STORAGE (COUNTED IN RAM USAGE)";
            storageTitle.ForeColor = Color.DeepSkyBlue;
            storageTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            storageTitle.Location = new Point(15, 10);
            storageTitle.AutoSize = true;
            storagePanel.Controls.Add(storageTitle);

            lblTotalStorage = CreateDetailLabel("Total File Size:", "", 15, 35, storagePanel);
            lblDocuments = CreateDetailLabel("📄 Documents:", "", 15, 60, storagePanel);
            lblDownloads = CreateDetailLabel("⬇ Downloads:", "", 15, 85, storagePanel);
            lblPrivate = CreateDetailLabel("🔒 Private:", "", 15, 110, storagePanel);

            Label divider2 = new Label();
            divider2.Text = "─────────────────────────────────────────────";
            divider2.ForeColor = Color.Gray;
            divider2.Location = new Point(15, 135);
            divider2.AutoSize = true;
            storagePanel.Controls.Add(divider2);

            Label noteLabel = new Label();
            noteLabel.Text = "⚠️ File storage is counted as disk cache in RAM usage";
            noteLabel.ForeColor = Color.Yellow;
            noteLabel.Font = new Font("Segoe UI", 8);
            noteLabel.Location = new Point(15, 150);
            noteLabel.AutoSize = true;
            storagePanel.Controls.Add(noteLabel);

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 REFRESH";
            btnRefresh.Size = new Size(100, 35);
            btnRefresh.Location = new Point(30, 570);
            btnRefresh.BackColor = Color.FromArgb(35, 42, 52);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += (s, e) => UpdateMemoryDisplay();

            Button btnPanic = new Button();
            btnPanic.Text = "⚠ KERNEL PANIC (Fill Memory)";
            btnPanic.Size = new Size(180, 35);
            btnPanic.Location = new Point(150, 570);
            btnPanic.BackColor = Color.FromArgb(100, 50, 30);
            btnPanic.ForeColor = Color.White;
            btnPanic.FlatStyle = FlatStyle.Flat;
            btnPanic.Click += BtnPanic_Click;

            Label info = new Label();
            info.Text = "💾 Auto-refresh every 2 seconds | File sizes are added to RAM usage dynamically";
            info.ForeColor = Color.Gray;
            info.Font = new Font("Segoe UI", 8);
            info.Location = new Point(30, 620);
            info.AutoSize = true;

            Controls.Add(title);
            Controls.Add(ramLabel);
            Controls.Add(ramBar);
            Controls.Add(lblPercentage);
            Controls.Add(lblWarning);
            Controls.Add(ramPanel);
            Controls.Add(storagePanel);
            Controls.Add(btnRefresh);
            Controls.Add(btnPanic);
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

        private string FormatSizeKB(int kb)
        {
            if (kb < 1024) return kb + " KB";
            return (kb / 1024) + " MB";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)) + " MB";
            return (bytes / (1024 * 1024 * 1024)) + " GB";
        }

        private void StartRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += (s, e) => UpdateMemoryDisplay();
            refreshTimer.Start();
        }

        private void UpdateFromTaskManager()
        {
            calculatorRunning = MemoryStorage.CalculatorRunning;
            timeRunning = MemoryStorage.TimeRunning;
        }

        private void UpdateFolderSizes()
        {
            documentsSize = GetFolderSize(documentsPath);
            downloadsSize = GetFolderSize(downloadsPath);
            privateSize = GetFolderSize(privatePath);
            totalFileSize = documentsSize + downloadsSize + privateSize;
        }

        private long GetFolderSize(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;
            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                    size += new FileInfo(file).Length;
            }
            catch { }
            return size;
        }

        public int GetUsedRAM()
        {
            MemoryStorage.RefreshFileStorage();
            return MemoryStorage.GetUsedRAM();
        }

        private void UpdateMemoryDisplay()
        {
            UpdateFromTaskManager();
            MemoryStorage.RefreshFileStorage();
            UpdateFolderSizes();

            int usedMem = MemoryStorage.GetUsedRAM();
            int freeMem = MemoryStorage.GetFreeRAM();
            int percentage = MemoryStorage.GetPercentage();
            int fileSizeKB = (int)(totalFileSize / 1024);

            ramBar.Value = usedMem;

            // FORCE RED BAR IF PANIC FILE EXISTS
            string panicFile = Path.Combine(documentsPath, "PANIC_TEMP_FILE.tmp");
            bool panicFileExists = File.Exists(panicFile);

            if (panicFileExists || percentage >= 80)
            {
                ramBar.ForeColor = Color.Red;
                lblWarning.Text = "🔴 CRITICAL: KERNEL PANIC ACTIVE! Memory is full!";
                lblWarning.ForeColor = Color.Red;
                lblWarning.BackColor = Color.DarkRed;

                // Update Kernel Panic window
                UpdateKernelPanicWindow(true);
            }
            else if (percentage >= 50)
            {
                ramBar.ForeColor = Color.Yellow;
                lblWarning.Text = "⚠️ WARNING: Memory usage is above 50%";
                lblWarning.ForeColor = Color.Yellow;
                lblWarning.BackColor = Color.Transparent;

                UpdateKernelPanicWindow(false);
            }
            else
            {
                ramBar.ForeColor = Color.Lime;
                lblWarning.Text = "";
                lblWarning.BackColor = Color.Transparent;

                UpdateKernelPanicWindow(false);
            }

            lblPercentage.Text = $"{percentage}% USED ({FormatSizeKB(usedMem)} / {FormatSizeKB(MemoryStorage.TotalRAM)})";
            lblUsed.Text = $"USED RAM:         {FormatSizeKB(usedMem)}";
            lblFree.Text = $"FREE RAM:         {FormatSizeKB(freeMem)}";
            lblTotal.Text = $"TOTAL RAM:        {FormatSizeKB(MemoryStorage.TotalRAM)}";

            string calcMem = calculatorRunning ? FormatSizeKB(MemoryStorage.CalculatorMem) + " 🟢" : "0 KB 🔴";
            string timeMemStr = timeRunning ? FormatSizeKB(MemoryStorage.TimeMem) + " 🟢" : "0 KB 🔴";
            lblCalculator.Text = $"Calculator:       {calcMem}";
            lblTime.Text = $"Time Service:     {timeMemStr}";
            lblKernel.Text = $"Kernel:           {FormatSizeKB(MemoryStorage.KernelMem)} KB";
            lblShell.Text = $"Shell:            {FormatSizeKB(MemoryStorage.ShellMem)} KB";
            lblTask.Text = $"Task Manager:     {FormatSizeKB(MemoryStorage.TaskMem)} KB";

            lblTotalStorage.Text = $"Total File Size:  {FormatBytes(totalFileSize)} (≈ {fileSizeKB} KB in RAM)";
            lblDocuments.Text = $"📄 Documents:     {FormatBytes(documentsSize)}";
            lblDownloads.Text = $"⬇ Downloads:     {FormatBytes(downloadsSize)}";
            lblPrivate.Text = $"🔒 Private:       {FormatBytes(privateSize)}";
        }

        private void UpdateKernelPanicWindow(bool isPanic)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is KernelPanicForm panicForm)
                {
                    panicForm.SetPanicMode(isPanic);
                }
            }
        }

        private void BtnPanic_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "⚠ KERNEL PANIC: Create dummy file to fill memory?\n\n" +
                "This will create a temporary file in Documents folder.\n" +
                "Memory usage will increase to CRITICAL level.\n\n" +
                "Continue?",
                "Kernel Panic",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string panicFile = Path.Combine(documentsPath, "PANIC_TEMP_FILE.tmp");

                    // If panic file already exists, delete it first
                    if (File.Exists(panicFile))
                    {
                        File.Delete(panicFile);
                        UpdateFolderSizes();
                        MemoryStorage.RefreshFileStorage();
                    }

                    // Get current memory usage
                    int currentUsed = GetUsedRAM();

                    // Create a file that will push memory to 90%+
                    int targetUsed = (int)(totalRAM * 0.90);
                    int neededKB = targetUsed - currentUsed;

                    // Ensure minimum 500KB file
                    if (neededKB < 500) neededKB = 500;
                    if (neededKB > 3500) neededKB = 3500;

                    byte[] data = new byte[1024];
                    Random rnd = new Random();

                    using (FileStream fs = new FileStream(panicFile, FileMode.Create))
                    {
                        for (int i = 0; i < neededKB; i++)
                        {
                            rnd.NextBytes(data);
                            fs.Write(data, 0, data.Length);
                        }
                    }

                    // Force refresh
                    UpdateFolderSizes();
                    MemoryStorage.RefreshFileStorage();

                    int newUsed = GetUsedRAM();
                    int newPercentage = (newUsed * 100) / totalRAM;

                    SystemLogForm.WriteLog("KERNEL", "admin", "Kernel Panic",
                        $"Created {neededKB}KB panic file - Memory: {currentUsed}KB → {newUsed}KB / {totalRAM}KB ({newPercentage}%)", "CRITICAL");

                    UpdateMemoryDisplay();

                    // Force update Kernel Panic window
                    UpdateKernelPanicWindow(true);

                    MessageBox.Show(
                        $"⚠ KERNEL PANIC TRIGGERED! ⚠\n\n" +
                        $"Created {neededKB} KB file in Documents folder.\n" +
                        $"Memory: {currentUsed}KB → {newUsed}KB / {totalRAM}KB ({newPercentage}%)\n\n" +
                        $"🔴 SYSTEM IN PANIC MODE!\n\n" +
                        $"To recover:\n" +
                        $"1. Open File Explorer → Documents\n" +
                        $"2. Delete PANIC_TEMP_FILE.tmp\n" +
                        $"3. Click REFRESH in Memory Manager\n" +
                        $"4. Or click RECOVERY in Kernel Panic window",
                        "KERNEL PANIC",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Panic failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        public void RefreshMemory()
        {
            UpdateMemoryDisplay();
        }

        public void TriggerKernelPanic()
        {
            BtnPanic_Click(null, null);
        }
    }
}