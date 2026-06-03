using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class TerminalForm : Form
    {
        private RichTextBox terminalBox;
        private Button btnLaunch;

        public TerminalForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "MiniOS Terminal";
            this.Size = new Size(1000, 600);
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.CenterScreen;

            terminalBox = new RichTextBox();

            terminalBox.BackColor = Color.Black;
            terminalBox.ForeColor = Color.Lime;
            terminalBox.Font =
                new Font("Consolas", 12, FontStyle.Bold);

            terminalBox.ReadOnly = true;

            terminalBox.Location =
                new Point(20, 20);

            terminalBox.Size =
                new Size(940, 430);

            terminalBox.Text =
@"MINIOS TERMINAL

Click Launch Terminal.

Backend status:
READY";

            btnLaunch = new Button();

            btnLaunch.Text =
                "Launch MiniOS";

            btnLaunch.Size =
                new Size(220, 60);

            btnLaunch.Location =
                new Point(20, 480);

            btnLaunch.BackColor =
                Color.DarkGreen;

            btnLaunch.ForeColor =
                Color.White;

            btnLaunch.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            btnLaunch.Click += LaunchMiniOS;

            Controls.Add(terminalBox);
            Controls.Add(btnLaunch);
        }

        private void LaunchMiniOS(
            object sender,
            EventArgs e)
        {
            try
            {
                string backendPath =
                    Path.Combine(
                    Application.StartupPath,
                    "MiniOS.exe");

                if (!File.Exists(backendPath))
                {
                    MessageBox.Show(
                    "MiniOS.exe not found.\n\n" +
                    "Build backend first.",
                    "Backend Missing");

                    return;
                }

                Process.Start(backendPath);

                terminalBox.AppendText(
                    "\n\nBackend launched successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Launch Error");
            }
        }
    }
}