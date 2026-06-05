using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class DesktopForm : Form
    {
        private Panel leftDock, taskbar, desktopArea;
        private Timer clockTimer;
        private Label lblClock;
        private FlowLayoutPanel buttonPanel;
        private Panel scrollPanel;
        private bool buttonsCreated = false;

        public DesktopForm()
        {
            this.Text = "NOVA-OS Desktop";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            BuildLeftDock();
            BuildTaskbar();
            BuildDesktopArea();

            this.KeyDown += DesktopForm_KeyDown;
            this.Resize += DesktopForm_Resize;

            SystemLogForm.WriteLog("SYSTEM", "Desktop", "NOVA-OS Started", "Desktop environment loaded", "SUCCESS");
        }

        private void DesktopForm_Resize(object sender, EventArgs e)
        {
            UpdateClockPosition();
            if (scrollPanel != null)
                scrollPanel.Height = leftDock.Height - 80;
        }

        private void BuildLeftDock()
        {
            leftDock = new Panel();
            leftDock.Dock = DockStyle.Left;
            leftDock.Width = 260;
            leftDock.BackColor = Color.FromArgb(24, 30, 38);
            Controls.Add(leftDock);

            // MAIN MENU Label
            Label logo = new Label();
            logo.Text = "MAIN MENU";
            logo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            logo.ForeColor = Color.DeepSkyBlue;
            logo.Location = new Point(25, 20);
            logo.AutoSize = true;
            leftDock.Controls.Add(logo);

            scrollPanel = new Panel();
            scrollPanel.Location = new Point(0, 80);
            scrollPanel.Size = new Size(260, leftDock.Height - 80);
            scrollPanel.AutoScroll = true;
            scrollPanel.BackColor = Color.FromArgb(24, 30, 38);
            leftDock.Controls.Add(scrollPanel);

            buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.FlowDirection = FlowDirection.TopDown;
            buttonPanel.WrapContents = false;
            buttonPanel.AutoSize = true;
            buttonPanel.Padding = new Padding(10, 10, 10, 20);
            buttonPanel.BackColor = Color.FromArgb(24, 30, 38);
            scrollPanel.Controls.Add(buttonPanel);

            if (!buttonsCreated)
            {
                CreateButtons();
                buttonsCreated = true;
            }
        }
        private void CreateButtons()
        {
            buttonPanel.Controls.Clear();

            AddButton("🖥️", "TERMINAL", BtnTerminal_Click);
            AddButton("🌐", "BROWSER", BtnBrowser_Click);
            AddButton("📁", "FILES", BtnFiles_Click);
            AddButton("🖼️", "IMAGES", BtnImages_Click);
            AddButton("📊", "MEMORY", BtnMemory_Click);
            AddButton("📋", "TASKS", BtnTasks_Click);
            AddButton("⚠️", "KERNEL PANIC", BtnKernelPanic_Click);
            AddButton("📜", "SYSTEM LOG", BtnLogs_Click);
        }

        private void AddButton(string icon, string text, EventHandler clickHandler)
        {
            Button btn = new Button();
            btn.Text = $"  {icon}  {text}";
            btn.Size = new Size(220, 48);
            btn.Margin = new Padding(5, 3, 5, 3);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.BackColor = Color.FromArgb(35, 42, 52);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 60, 70);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(35, 42, 52);
            btn.Click += clickHandler;

            buttonPanel.Controls.Add(btn);
        }

        private void BuildTaskbar()
        {
            taskbar = new Panel();
            taskbar.Dock = DockStyle.Bottom;
            taskbar.Height = 45;
            taskbar.BackColor = Color.FromArgb(22, 28, 35);
            Controls.Add(taskbar);

            Button startBtn = new Button();
            startBtn.Text = " NOVA-OS";
            startBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            startBtn.Size = new Size(100, 35);
            startBtn.Location = new Point(10, 5);
            startBtn.BackColor = Color.DeepSkyBlue;
            startBtn.ForeColor = Color.White;
            startBtn.FlatStyle = FlatStyle.Flat;
            startBtn.Click += (s, e) => MessageBox.Show("NOVA-OS System\nVersion 2.0\n\n© 2024", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            taskbar.Controls.Add(startBtn);

            lblClock = new Label();
            lblClock.ForeColor = Color.White;
            lblClock.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblClock.AutoSize = true;
            taskbar.Controls.Add(lblClock);

            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += UpdateClock;
            clockTimer.Start();

            UpdateClockPosition();
        }

        private void UpdateClock(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void UpdateClockPosition()
        {
            lblClock.Location = new Point(this.Width - 120, 12);
        }

        private void BuildDesktopArea()
        {
            desktopArea = new Panel();
            desktopArea.Dock = DockStyle.Fill;
            desktopArea.BackColor = Color.FromArgb(16, 24, 32);
            Controls.Add(desktopArea);

            // Welcome Panel on TOP - SAME COLOR AS LEFT PANEL
            Panel welcomePanel = new Panel();
            welcomePanel.Dock = DockStyle.Top;
            welcomePanel.Height = 80;
            welcomePanel.BackColor = Color.FromArgb(24, 30, 38);  // ← SAME as leftDock color

            Label welcomeLabel = new Label();
            welcomeLabel.Text = "WELCOME TO NOVA-OS";
            welcomeLabel.ForeColor = Color.DeepSkyBlue;
            welcomeLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            welcomeLabel.TextAlign = ContentAlignment.MiddleCenter;
            welcomeLabel.Dock = DockStyle.Fill;
            welcomePanel.Controls.Add(welcomeLabel);

            desktopArea.Controls.Add(welcomePanel);

            // Kali Image - FULL BACKGROUND (Like Wallpaper)
            try
            {
                string imagePath = Path.Combine(Application.StartupPath, "dragon.png");

                if (!File.Exists(imagePath))
                    imagePath = Path.Combine(Application.StartupPath, "Resources", "dragon.png");
                if (!File.Exists(imagePath))
                    imagePath = @"D:\coal lab\MiniOS\MiniOS_GUI\bin\Debug\kali pic.png";
                if (!File.Exists(imagePath))
                    imagePath = @"D:\coal lab\MiniOS_GUI\bin\Debug\kali pic.png";

                if (File.Exists(imagePath))
                {
                    PictureBox wallpaper = new PictureBox();
                    wallpaper.Image = Image.FromFile(imagePath);
                    wallpaper.SizeMode = PictureBoxSizeMode.StretchImage;
                    wallpaper.Dock = DockStyle.Fill;
                    wallpaper.BackColor = Color.Transparent;
                    desktopArea.Controls.Add(wallpaper);
                    wallpaper.SendToBack();
                }
            }
            catch { }
        }
        private void DesktopForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (scrollPanel != null)
            {
                if (e.Control && e.KeyCode == Keys.Up)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y - 30);
                else if (e.Control && e.KeyCode == Keys.Down)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y + 30);
            }
        }

        // ==================== BUTTON CLICK HANDLERS ====================

        private void BtnTerminal_Click(object sender, EventArgs e)
        {
            new TerminalForm().Show();
        }

        private void BtnBrowser_Click(object sender, EventArgs e)
        {
            try { System.Diagnostics.Process.Start("chrome.exe"); }
            catch { System.Diagnostics.Process.Start("msedge.exe"); }
        }

        private void BtnFiles_Click(object sender, EventArgs e)
        {
            new ExplorerForm().Show();
        }

        private void BtnImages_Click(object sender, EventArgs e)
        {
            string imagesPath = Path.Combine(Application.StartupPath, "MiniOS_Pictures");
            if (!Directory.Exists(imagesPath))
                Directory.CreateDirectory(imagesPath);
            ImageViewerForm viewer = new ImageViewerForm(imagesPath);
            viewer.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "Image Viewer Opened", "User opened gallery", "SUCCESS");
        }

        private void BtnMemory_Click(object sender, EventArgs e)
        {
            new MemoryForm().Show();
        }

        private void BtnTasks_Click(object sender, EventArgs e)
        {
            new TaskManagerForm().Show();
        }

        private void BtnKernelPanic_Click(object sender, EventArgs e)
        {
            KernelPanicForm panicForm = new KernelPanicForm();
            panicForm.Show();
        }

        private void BtnLogs_Click(object sender, EventArgs e)
        {
            SystemLogForm logs = new SystemLogForm();
            logs.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "System Log Opened", "User viewed logs", "SUCCESS");
        }
    }
}