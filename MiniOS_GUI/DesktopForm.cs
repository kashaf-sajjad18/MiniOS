using System;
using System.Drawing;
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
        private Label welcome;

        public DesktopForm()
        {
            // Initialize form properties
            this.Text = "MiniOS Desktop";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;

            // Build all UI components
            BuildLeftDock();
            BuildTaskbar();
            BuildDesktopArea();

            // Events
            this.KeyDown += DesktopForm_KeyDown;
            this.Shown += DesktopForm_Shown;
            this.Resize += DesktopForm_Resize;

            // Log
            SystemLogForm.WriteLog("SYSTEM", "Desktop", "MiniOS Started", "Desktop environment loaded", "SUCCESS");
        }

        private void DesktopForm_Shown(object sender, EventArgs e)
        {
            CenterWelcomeMessage();
        }

        private void DesktopForm_Resize(object sender, EventArgs e)
        {
            CenterWelcomeMessage();
            UpdateClockPosition();
            if (scrollPanel != null)
                scrollPanel.Height = leftDock.Height - 80;
        }

        private void CenterWelcomeMessage()
        {
            if (welcome != null && desktopArea != null)
            {
                welcome.Location = new Point(
                    Math.Max(0, (desktopArea.Width - welcome.Width) / 2),
                    Math.Max(0, (desktopArea.Height - welcome.Height) / 2)
                );
            }
        }

        private void BuildLeftDock()
        {
            leftDock = new Panel();
            leftDock.Dock = DockStyle.Left;
            leftDock.Width = 260;
            leftDock.BackColor = Color.FromArgb(24, 30, 38);
            Controls.Add(leftDock);

            // Logo
            Label logo = new Label();
            logo.Text = "MINIOS";
            logo.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            logo.ForeColor = Color.DeepSkyBlue;
            logo.Location = new Point(25, 20);
            logo.AutoSize = true;
            leftDock.Controls.Add(logo);

            // Scrollable panel for buttons
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

            // CREATE BUTTONS ONLY ONCE
            AddNavButton(">_ TERMINAL", "🖥️", BtnTerminal_Click);
            AddNavButton("🌐 BROWSER", "🌐", BtnBrowser_Click);
            AddNavButton("📁 FILES", "📁", BtnFiles_Click);
            AddNavButton("📊 MEMORY", "📊", BtnMemory_Click);
            AddNavButton("📋 TASKS", "📋", BtnTasks_Click);
            AddNavButton("⚠ PANIC", "⚠️", BtnPanic_Click);
            AddNavButton("🔒 PRIVATE", "🔒", BtnPrivate_Click);
            AddNavButton("📋 SYSTEM LOG", "📜", BtnLogs_Click);
        }

        private void AddNavButton(string text, string icon, EventHandler clickHandler)
        {
            Button btn = new Button();
            btn.Text = $"  {icon}  {text}";
            btn.Size = new Size(220, 50);
            btn.Margin = new Padding(5, 5, 5, 5);
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
            startBtn.Text = " MINIOS";
            startBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            startBtn.Size = new Size(100, 35);
            startBtn.Location = new Point(10, 5);
            startBtn.BackColor = Color.DeepSkyBlue;
            startBtn.ForeColor = Color.White;
            startBtn.FlatStyle = FlatStyle.Flat;
            startBtn.Click += (s, e) => MessageBox.Show("MiniOS System\nVersion 2.0\n\n© 2024", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            taskbar.Controls.Add(startBtn);

            lblClock = new Label();
            lblClock.ForeColor = Color.White;
            lblClock.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblClock.AutoSize = true;
            lblClock.TextAlign = ContentAlignment.MiddleRight;
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

            welcome = new Label();
            welcome.Text = "WELCOME TO MINIOS";
            welcome.ForeColor = Color.DeepSkyBlue;
            welcome.Font = new Font("Segoe UI", 36, FontStyle.Bold);
            welcome.BackColor = Color.Transparent;
            welcome.AutoSize = true;
            desktopArea.Controls.Add(welcome);
        }

        private void DesktopForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (scrollPanel != null)
            {
                if (e.Control && e.KeyCode == Keys.Up)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y - 30);
                else if (e.Control && e.KeyCode == Keys.Down)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y + 30);
                else if (e.KeyCode == Keys.PageUp)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y - 200);
                else if (e.KeyCode == Keys.PageDown)
                    scrollPanel.AutoScrollPosition = new Point(0, -scrollPanel.AutoScrollPosition.Y + 200);
            }
        }

        // ==================== BUTTON CLICK HANDLERS ====================

        private void BtnTerminal_Click(object sender, EventArgs e)
        {
            TerminalForm t = new TerminalForm();
            t.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "Terminal Opened", "User launched terminal", "SUCCESS");
        }

        private void BtnBrowser_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("chrome.exe");
                SystemLogForm.WriteLog("APP", "Desktop", "Browser Opened", "Chrome launched", "SUCCESS");
            }
            catch
            {
                System.Diagnostics.Process.Start("msedge.exe");
                SystemLogForm.WriteLog("APP", "Desktop", "Browser Opened", "Edge launched", "SUCCESS");
            }
        }

        private void BtnFiles_Click(object sender, EventArgs e)
        {
            ExplorerForm exp = new ExplorerForm();
            exp.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "File Explorer Opened", "User accessed files", "SUCCESS");
        }

        private void BtnMemory_Click(object sender, EventArgs e)
        {
            MemoryForm m = new MemoryForm();
            m.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "Memory Manager Opened", "User checked memory", "SUCCESS");
        }

        private void BtnTasks_Click(object sender, EventArgs e)
        {
            TaskManagerForm t = new TaskManagerForm();
            t.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "Task Manager Opened", "User viewed processes", "SUCCESS");
        }

        private void BtnPanic_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Kernel Panic will restart MiniOS Desktop!\n\nContinue?", "⚠ KERNEL PANIC", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                SystemLogForm.WriteLog("SYSTEM", "Desktop", "Kernel Panic", "System restart", "WARNING");
                Application.Restart();
            }
        }

        private void BtnPrivate_Click(object sender, EventArgs e)
        {
            Form passForm = new Form();
            passForm.Text = "Private Access";
            passForm.Size = new Size(350, 160);
            passForm.BackColor = Color.FromArgb(16, 24, 32);
            passForm.StartPosition = FormStartPosition.CenterParent;
            passForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label lbl = new Label();
            lbl.Text = "Enter Private Password:";
            lbl.ForeColor = Color.White;
            lbl.Location = new Point(25, 25);
            lbl.AutoSize = true;

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(25, 55);
            txtPass.Size = new Size(280, 25);
            txtPass.PasswordChar = '*';

            Button btnOk = new Button();
            btnOk.Text = "ACCESS";
            btnOk.Location = new Point(25, 95);
            btnOk.Size = new Size(130, 35);
            btnOk.BackColor = Color.FromArgb(35, 42, 52);
            btnOk.ForeColor = Color.White;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Click += (s, ev) => {
                if (txtPass.Text == "admin123")
                {
                    SystemLogForm.WriteLog("PRIVATE", "admin", "Private Area Accessed", "Access granted", "SUCCESS");
                    MessageBox.Show("Welcome to Private Area!", "Access Granted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    passForm.Close();
                }
                else
                {
                    SystemLogForm.WriteLog("PRIVATE", "Unknown", "Private Access Denied", "Failed attempt", "FAILED");
                    MessageBox.Show("Access Denied! Invalid Password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPass.Clear();
                }
            };

            Button btnCancel = new Button();
            btnCancel.Text = "CANCEL";
            btnCancel.Location = new Point(175, 95);
            btnCancel.Size = new Size(130, 35);
            btnCancel.BackColor = Color.FromArgb(35, 42, 52);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += (s, ev) => passForm.Close();

            passForm.Controls.Add(lbl);
            passForm.Controls.Add(txtPass);
            passForm.Controls.Add(btnOk);
            passForm.Controls.Add(btnCancel);
            passForm.ShowDialog();
        }

        private void BtnLogs_Click(object sender, EventArgs e)
        {
            SystemLogForm logs = new SystemLogForm();
            logs.Show();
            SystemLogForm.WriteLog("APP", "Desktop", "System Log Opened", "User viewed logs", "SUCCESS");
        }
    }
}