using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class DesktopForm : Form
    {
        Panel leftDock;
        Panel taskbar;
        Panel desktopArea;

        Timer clockTimer;

        Label lblClock;

        Button btnTerminal;
        Button btnBrowser;
        Button btnFiles;
        Button btnMemory;
        Button btnTasks;
        Button btnPanic;
        Button btnPrivate;

        public DesktopForm()
        {
            BuildDesktop();
        }

        void BuildDesktop()
        {
            this.Text = "MiniOS Desktop";

            this.WindowState =
                FormWindowState.Maximized;

            this.BackColor =
                Color.FromArgb(16, 24, 32);

            this.FormBorderStyle =
                FormBorderStyle.Sizable;

            BuildLeftDock();

            BuildTaskbar();

            BuildDesktopArea();
        }

        void BuildLeftDock()
        {
            leftDock = new Panel();

            leftDock.Dock =
                DockStyle.Left;

            leftDock.Width = 230;

            leftDock.BackColor =
                Color.FromArgb(24, 30, 38);

            Controls.Add(leftDock);

            Label logo =
                new Label();

            logo.Text =
                "MINIOS";

            logo.Font =
                new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold);

            logo.ForeColor =
                Color.DeepSkyBlue;

            logo.Location =
                new Point(40, 30);

            logo.AutoSize = true;

            leftDock.Controls.Add(logo);

            btnTerminal =
                CreateDockButton(
                    ">_ TERMINAL",
                    100);

            btnBrowser =
                CreateDockButton(
                    "🌐 BROWSER",
                    170);

            btnFiles =
                CreateDockButton(
                    "📁 FILES",
                    240);

            btnMemory =
                CreateDockButton(
                    "📊 MEMORY",
                    310);

            btnTasks =
                CreateDockButton(
                    "📋 TASKS",
                    380);

            btnPanic =
                CreateDockButton(
                    "⚠ PANIC",
                    450);

            btnPrivate =
                CreateDockButton(
                    "🔒 PRIVATE",
                    520);

            leftDock.Controls.Add(btnTerminal);
            leftDock.Controls.Add(btnBrowser);
            leftDock.Controls.Add(btnFiles);
            leftDock.Controls.Add(btnMemory);
            leftDock.Controls.Add(btnTasks);
            leftDock.Controls.Add(btnPanic);
            leftDock.Controls.Add(btnPrivate);

            btnTerminal.Click += BtnTerminal_Click;
            btnBrowser.Click += BtnBrowser_Click;
            btnFiles.Click += BtnFiles_Click;
            btnMemory.Click += BtnMemory_Click;
            btnTasks.Click += BtnTasks_Click;
            btnPanic.Click += BtnPanic_Click;
        }

        Button CreateDockButton(
            string text,
            int y)
        {
            Button b =
                new Button();

            b.Text = text;

            b.Size =
                new Size(180, 50);

            b.Location =
                new Point(20, y);

            b.FlatStyle =
                FlatStyle.Flat;

            b.FlatAppearance.BorderSize = 0;

            b.BackColor =
                Color.FromArgb(35, 42, 52);

            b.ForeColor =
                Color.White;

            b.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold);

            return b;
        }

        void BuildTaskbar()
        {
            taskbar = new Panel();

            taskbar.Dock =
                DockStyle.Bottom;

            taskbar.Height = 45;

            taskbar.BackColor =
                Color.FromArgb(22, 28, 35);

            Controls.Add(taskbar);

            lblClock =
                new Label();

            lblClock.ForeColor =
                Color.White;

            lblClock.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            lblClock.AutoSize = true;

            lblClock.Location =
                new Point(1200, 12);

            taskbar.Controls.Add(lblClock);

            clockTimer =
                new Timer();

            clockTimer.Interval = 1000;

            clockTimer.Tick += UpdateClock;

            clockTimer.Start();
        }

        void UpdateClock(
            object sender,
            EventArgs e)
        {
            lblClock.Text =
                DateTime.Now.ToString(
                    "hh:mm:ss tt");
        }

        void BuildDesktopArea()
        {
            desktopArea =
                new Panel();

            desktopArea.Dock =
                DockStyle.Fill;

            desktopArea.BackColor =
                Color.FromArgb(16, 24, 32);

            Controls.Add(desktopArea);

            Label welcome =
                new Label();

            welcome.Text =
                "WELCOME TO MINIOS";

            welcome.ForeColor =
                Color.DeepSkyBlue;

            welcome.Font =
                new Font(
                    "Segoe UI",
                    28,
                    FontStyle.Bold);

            welcome.Location =
                new Point(320, 120);

            welcome.AutoSize = true;

            desktopArea.Controls.Add(welcome);
        }

        void BtnTerminal_Click(
            object sender,
            EventArgs e)
        {
            TerminalForm t =
                new TerminalForm();

            t.Show();
        }

        void BtnBrowser_Click(
            object sender,
            EventArgs e)
        {
            System.Diagnostics.Process.Start(
                "chrome.exe");
        }

        void BtnFiles_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "ExplorerForm coming next.");
        }

        void BtnMemory_Click(
            object sender,
            EventArgs e)
        {
            MemoryForm m =
                new MemoryForm();

            m.Show();
        }

        void BtnTasks_Click(
            object sender,
            EventArgs e)
        {
            TaskManagerForm t =
                new TaskManagerForm();

            t.Show();
        }

        void BtnPanic_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Kernel Panic module coming next.");
        }
    }
}