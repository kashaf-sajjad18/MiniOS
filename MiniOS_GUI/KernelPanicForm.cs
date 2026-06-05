using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class KernelPanicForm : Form
    {
        private Timer updateTimer;
        private Label lblStopCode, lblFreeMemory, lblMode, lblUptime;
        private ListView serviceList;
        private ListView eventLog;
        private ProgressBar severityBar;
        private Button btnRecover, btnClose;
        private Label lblSeverityPercent;

        private DateTime startTime;
        private int totalRAM = 4096;
        private bool isPanicActive = false;
        private int lastUsedMem = 0;

        public KernelPanicForm()
        {
            startTime = DateTime.Now;
            BuildUI();
            StartUpdater();
            SystemLogForm.WriteLog("KERNEL", "admin", "Kernel Panic Mode", "Panic kernel activated", "WARNING");
        }

        private void BuildUI()
        {
            this.Text = "MiniOS - Panic Kernel";
            this.Size = new Size(900, 680);
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(8, 8, 12);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            Label lblTitle = new Label();
            lblTitle.Text = "⚠ KERNEL PANIC";
            lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 80, 80);
            lblTitle.Location = new Point(30, 20);
            lblTitle.AutoSize = true;

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Kernel containment, diagnostics, and recovery";
            lblSubtitle.Font = new Font("Segoe UI", 10);
            lblSubtitle.ForeColor = Color.Gray;
            lblSubtitle.Location = new Point(35, 55);
            lblSubtitle.AutoSize = true;

            Panel mainPanel = new Panel();
            mainPanel.Location = new Point(20, 90);
            mainPanel.Size = new Size(850, 540);
            mainPanel.BackColor = Color.FromArgb(12, 12, 18);
            mainPanel.BorderStyle = BorderStyle.FixedSingle;

            Panel leftPanel = new Panel();
            leftPanel.Location = new Point(10, 10);
            leftPanel.Size = new Size(400, 520);
            leftPanel.BackColor = Color.FromArgb(8, 8, 12);

            Panel modePanel = new Panel();
            modePanel.Location = new Point(0, 0);
            modePanel.Size = new Size(400, 80);
            modePanel.BackColor = Color.FromArgb(20, 20, 28);

            Label lblModeTitle = new Label();
            lblModeTitle.Text = "MODE";
            lblModeTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblModeTitle.ForeColor = Color.Gray;
            lblModeTitle.Location = new Point(15, 10);
            lblModeTitle.AutoSize = true;

            lblMode = new Label();
            lblMode.Text = "ARMED";
            lblMode.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblMode.ForeColor = Color.FromArgb(255, 80, 80);
            lblMode.Location = new Point(15, 35);
            lblMode.AutoSize = true;

            modePanel.Controls.Add(lblModeTitle);
            modePanel.Controls.Add(lblMode);

            Panel stopCodePanel = new Panel();
            stopCodePanel.Location = new Point(0, 85);
            stopCodePanel.Size = new Size(400, 70);
            stopCodePanel.BackColor = Color.FromArgb(15, 15, 22);

            Label lblStopTitle = new Label();
            lblStopTitle.Text = "STOP CODE";
            lblStopTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblStopTitle.ForeColor = Color.Gray;
            lblStopTitle.Location = new Point(15, 10);
            lblStopTitle.AutoSize = true;

            lblStopCode = new Label();
            lblStopCode.Text = "0xBAD0F00D";
            lblStopCode.Font = new Font("Consolas", 14, FontStyle.Bold);
            lblStopCode.ForeColor = Color.FromArgb(255, 150, 80);
            lblStopCode.Location = new Point(15, 35);
            lblStopCode.AutoSize = true;

            stopCodePanel.Controls.Add(lblStopTitle);
            stopCodePanel.Controls.Add(lblStopCode);

            Panel memoryPanel = new Panel();
            memoryPanel.Location = new Point(0, 160);
            memoryPanel.Size = new Size(400, 70);
            memoryPanel.BackColor = Color.FromArgb(20, 20, 28);

            Label lblMemTitle = new Label();
            lblMemTitle.Text = "FREE MEMORY";
            lblMemTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblMemTitle.ForeColor = Color.Gray;
            lblMemTitle.Location = new Point(15, 10);
            lblMemTitle.AutoSize = true;

            lblFreeMemory = new Label();
            lblFreeMemory.Text = "3616 KB";
            lblFreeMemory.Font = new Font("Consolas", 18, FontStyle.Bold);
            lblFreeMemory.ForeColor = Color.Lime;
            lblFreeMemory.Location = new Point(15, 35);
            lblFreeMemory.AutoSize = true;

            memoryPanel.Controls.Add(lblMemTitle);
            memoryPanel.Controls.Add(lblFreeMemory);

            Panel severityPanel = new Panel();
            severityPanel.Location = new Point(0, 235);
            severityPanel.Size = new Size(400, 100);
            severityPanel.BackColor = Color.FromArgb(15, 15, 22);

            Label lblSeverityTitle = new Label();
            lblSeverityTitle.Text = "SEVERITY";
            lblSeverityTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblSeverityTitle.ForeColor = Color.Gray;
            lblSeverityTitle.Location = new Point(15, 10);
            lblSeverityTitle.AutoSize = true;

            severityBar = new ProgressBar();
            severityBar.Location = new Point(15, 35);
            severityBar.Size = new Size(370, 20);
            severityBar.Maximum = 100;
            severityBar.Value = 12;

            lblSeverityPercent = new Label();
            lblSeverityPercent.Text = "12%";
            lblSeverityPercent.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblSeverityPercent.ForeColor = Color.FromArgb(255, 80, 80);
            lblSeverityPercent.Location = new Point(15, 60);
            lblSeverityPercent.AutoSize = true;

            severityPanel.Controls.Add(lblSeverityTitle);
            severityPanel.Controls.Add(severityBar);
            severityPanel.Controls.Add(lblSeverityPercent);

            Panel uptimePanel = new Panel();
            uptimePanel.Location = new Point(0, 340);
            uptimePanel.Size = new Size(400, 60);
            uptimePanel.BackColor = Color.FromArgb(20, 20, 28);

            Label lblUptimeTitle = new Label();
            lblUptimeTitle.Text = "UPTIME";
            lblUptimeTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUptimeTitle.ForeColor = Color.Gray;
            lblUptimeTitle.Location = new Point(15, 10);
            lblUptimeTitle.AutoSize = true;

            lblUptime = new Label();
            lblUptime.Text = "00:00:00";
            lblUptime.Font = new Font("Consolas", 14, FontStyle.Bold);
            lblUptime.ForeColor = Color.White;
            lblUptime.Location = new Point(15, 32);
            lblUptime.AutoSize = true;

            uptimePanel.Controls.Add(lblUptimeTitle);
            uptimePanel.Controls.Add(lblUptime);

            leftPanel.Controls.Add(modePanel);
            leftPanel.Controls.Add(stopCodePanel);
            leftPanel.Controls.Add(memoryPanel);
            leftPanel.Controls.Add(severityPanel);
            leftPanel.Controls.Add(uptimePanel);

            Panel rightPanel = new Panel();
            rightPanel.Location = new Point(420, 10);
            rightPanel.Size = new Size(420, 250);
            rightPanel.BackColor = Color.FromArgb(8, 8, 12);

            Label lblServices = new Label();
            lblServices.Text = "SERVICE CONTAINMENT";
            lblServices.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblServices.ForeColor = Color.DeepSkyBlue;
            lblServices.Location = new Point(10, 5);
            lblServices.AutoSize = true;

            serviceList = new ListView();
            serviceList.Location = new Point(5, 30);
            serviceList.Size = new Size(410, 210);
            serviceList.View = View.Details;
            serviceList.BackColor = Color.FromArgb(12, 12, 18);
            serviceList.ForeColor = Color.White;
            serviceList.Font = new Font("Consolas", 9);
            serviceList.GridLines = true;
            serviceList.Columns.Add("SERVICE", 150);
            serviceList.Columns.Add("STATUS", 100);
            serviceList.Columns.Add("ACTION", 120);

            rightPanel.Controls.Add(lblServices);
            rightPanel.Controls.Add(serviceList);

            Panel eventPanel = new Panel();
            eventPanel.Location = new Point(420, 270);
            eventPanel.Size = new Size(420, 260);
            eventPanel.BackColor = Color.FromArgb(8, 8, 12);

            Label lblEventLog = new Label();
            lblEventLog.Text = "EVENT LOG";
            lblEventLog.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblEventLog.ForeColor = Color.DeepSkyBlue;
            lblEventLog.Location = new Point(10, 5);
            lblEventLog.AutoSize = true;

            eventLog = new ListView();
            eventLog.Location = new Point(5, 30);
            eventLog.Size = new Size(410, 195);
            eventLog.View = View.Details;
            eventLog.BackColor = Color.FromArgb(12, 12, 18);
            eventLog.ForeColor = Color.FromArgb(200, 200, 200);
            eventLog.Font = new Font("Consolas", 8);
            eventLog.Columns.Add("TIME", 80);
            eventLog.Columns.Add("EVENT", 310);

            eventPanel.Controls.Add(lblEventLog);
            eventPanel.Controls.Add(eventLog);

            btnRecover = new Button();
            btnRecover.Text = "🔄 RECOVERY SEQUENCE";
            btnRecover.Size = new Size(180, 35);
            btnRecover.Location = new Point(520, 540);
            btnRecover.BackColor = Color.FromArgb(30, 100, 30);
            btnRecover.ForeColor = Color.White;
            btnRecover.FlatStyle = FlatStyle.Flat;
            btnRecover.Click += BtnRecover_Click;

            btnClose = new Button();
            btnClose.Text = "✖ CLOSE";
            btnClose.Size = new Size(120, 35);
            btnClose.Location = new Point(720, 540);
            btnClose.BackColor = Color.FromArgb(100, 30, 30);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(rightPanel);
            mainPanel.Controls.Add(eventPanel);

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(mainPanel);
            Controls.Add(btnRecover);
            Controls.Add(btnClose);

            LoadServices();
            AddEvent("Kernel Panic mode activated", "INFO");
            AddEvent("Panic kernel armed and ready", "INFO");
        }

        private void LoadServices()
        {
            serviceList.Items.Clear();
            AddService("Kernel", "RUNNING", "GUARD");
            AddService("Shell", "RUNNING", "MONITOR");
            AddService("Memory", "RUNNING", "MONITOR");
            AddService("Task Manager", "RUNNING", "MONITOR");
            AddService("Panic Kernel", "ARMED", "ACTIVE");
        }

        private void AddService(string name, string status, string action)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(status);
            item.SubItems.Add(action);

            if (name == "Panic Kernel")
            {
                if (isPanicActive)
                    item.ForeColor = Color.Red;
                else
                    item.ForeColor = Color.FromArgb(255, 80, 80);
            }
            else
                item.ForeColor = Color.Lime;

            serviceList.Items.Add(item);
        }

        private void UpdateServicePanicKernel()
        {
            foreach (ListViewItem item in serviceList.Items)
            {
                if (item.Text == "Panic Kernel")
                {
                    if (isPanicActive)
                    {
                        item.SubItems[1].Text = "TRIGGERED";
                        item.ForeColor = Color.Red;
                    }
                    else
                    {
                        item.SubItems[1].Text = "ARMED";
                        item.ForeColor = Color.FromArgb(255, 80, 80);
                    }
                    break;
                }
            }
        }

        private void AddEvent(string message, string type)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            ListViewItem item = new ListViewItem(time);
            item.SubItems.Add(message);

            if (type == "WARNING")
                item.ForeColor = Color.Yellow;
            else if (type == "ERROR")
                item.ForeColor = Color.Red;
            else if (type == "SUCCESS")
                item.ForeColor = Color.Lime;
            else
                item.ForeColor = Color.Gray;

            eventLog.Items.Insert(0, item);

            while (eventLog.Items.Count > 15)
                eventLog.Items.RemoveAt(eventLog.Items.Count - 1);
        }

        private void StartUpdater()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 1000;
            updateTimer.Tick += UpdateUI;
            updateTimer.Start();
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Only update uptime - no memory auto-check to prevent crashes
            TimeSpan uptime = DateTime.Now - startTime;
            lblUptime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", uptime.Hours, uptime.Minutes, uptime.Seconds);
        }

        private void BtnRecover_Click(object sender, EventArgs e)
        {
            try
            {
                string docsPath = Path.Combine(Application.StartupPath, "MiniOS_Documents");
                string panicFile = Path.Combine(docsPath, "PANIC_TEMP_FILE.tmp");

                if (File.Exists(panicFile))
                {
                    long fileSize = new FileInfo(panicFile).Length;
                    File.Delete(panicFile);
                    AddEvent($"Recovery: Deleted {fileSize / 1024}KB panic file", "SUCCESS");
                }

                AddEvent("Recovery sequence started", "INFO");
                AddEvent("Shell command queue flushed", "INFO");
                AddEvent("Memory cleanup initiated", "INFO");
                AddEvent("Panic kernel reset and re-armed", "SUCCESS");

                foreach (Form form in Application.OpenForms)
                {
                    if (form is MemoryForm memoryForm)
                    {
                        var method = memoryForm.GetType().GetMethod("RefreshMemory");
                        if (method != null)
                            method.Invoke(memoryForm, null);
                    }
                }

                // Reset panic mode
                isPanicActive = false;
                lblMode.Text = "ARMED";
                lblMode.ForeColor = Color.FromArgb(255, 80, 80);
                UpdateServicePanicKernel();
                severityBar.Value = 12;
                lblSeverityPercent.Text = "12%";
                lblFreeMemory.Text = "3616 KB";
                severityBar.ForeColor = Color.FromArgb(255, 80, 80);
                lblSeverityPercent.ForeColor = Color.FromArgb(255, 80, 80);
                lblFreeMemory.ForeColor = Color.Lime;

                SystemLogForm.WriteLog("KERNEL", "admin", "Kernel Recovery", "Recovery sequence completed", "SUCCESS");

                MessageBox.Show("✅ Recovery sequence completed!\n\nSystem memory restored.\nPanic kernel reset and re-armed.",
                    "Recovery Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Recovery failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetPanicMode(bool isPanic)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(SetPanicMode), isPanic);
                return;
            }

            isPanicActive = isPanic;
            if (isPanic)
            {
                lblMode.Text = "TRIGGERED";
                lblMode.ForeColor = Color.Red;
                UpdateServicePanicKernel();
                AddEvent("⚠️ KERNEL PANIC TRIGGERED!", "ERROR");
                AddEvent("Memory threshold exceeded - System in panic mode", "ERROR");
                severityBar.Value = 95;
                lblSeverityPercent.Text = "95%";
                lblFreeMemory.Text = "200 KB";
                severityBar.ForeColor = Color.Red;
                lblSeverityPercent.ForeColor = Color.Red;
                lblFreeMemory.ForeColor = Color.Red;
            }
            else
            {
                lblMode.Text = "ARMED";
                lblMode.ForeColor = Color.FromArgb(255, 80, 80);
                UpdateServicePanicKernel();
                severityBar.Value = 12;
                lblSeverityPercent.Text = "12%";
                lblFreeMemory.Text = "3616 KB";
                severityBar.ForeColor = Color.FromArgb(255, 80, 80);
                lblSeverityPercent.ForeColor = Color.FromArgb(255, 80, 80);
                lblFreeMemory.ForeColor = Color.Lime;
            }
        }
    }
}