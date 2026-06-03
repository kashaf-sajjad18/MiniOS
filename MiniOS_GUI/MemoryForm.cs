using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class MemoryForm : Form
    {
        ProgressBar ramBar;
        Label lblStats;

        public MemoryForm()
        {
          
            BuildUI();
        }

        void BuildUI()
        {
            this.Text = "MiniOS Memory Manager";

            this.Size =
                new Size(800, 500);

            this.BackColor =
                Color.FromArgb(16, 24, 32);

            Label title = new Label();

            title.Text =
                "MEMORY MANAGER";

            title.ForeColor =
                Color.DeepSkyBlue;

            title.Font =
                new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold);

            title.Location =
                new Point(30, 30);

            title.AutoSize = true;

            ramBar =
                new ProgressBar();

            ramBar.Location =
                new Point(40, 120);

            ramBar.Size =
                new Size(650, 40);

            ramBar.Maximum = 4096;

            ramBar.Value = 480;

            lblStats =
                new Label();

            lblStats.ForeColor =
                Color.White;

            lblStats.Font =
                new Font(
                    "Segoe UI",
                    14);

            lblStats.Location =
                new Point(40, 200);

            lblStats.Size =
                new Size(700, 200);

            lblStats.Text =
                "TOTAL RAM : 4096 KB\r\n\r\n" +
                "USED RAM : 480 KB\r\n" +
                "FREE RAM : 3616 KB\r\n\r\n" +
                "Kernel : 256 KB\r\n" +
                "Shell : 128 KB\r\n" +
                "Calculator : 64 KB\r\n" +
                "Time : 32 KB";

            Controls.Add(title);
            Controls.Add(ramBar);
            Controls.Add(lblStats);
        }
    }
}