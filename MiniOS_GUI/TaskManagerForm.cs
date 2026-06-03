using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class TaskManagerForm : Form
    {
        DataGridView grid;

        public TaskManagerForm()
        { 
            BuildUI();
        }

        void BuildUI()
        {
            this.Text = "MiniOS Task Manager";

            this.Size =
                new Size(1000, 600);

            this.BackColor =
                Color.FromArgb(16, 24, 32);

            Label title = new Label();

            title.Text =
                "TASK MANAGER";

            title.ForeColor =
                Color.DeepSkyBlue;

            title.Font =
                new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold);

            title.Location =
                new Point(30, 20);

            title.AutoSize = true;

            grid =
                new DataGridView();

            grid.Location =
                new Point(30, 100);

            grid.Size =
                new Size(900, 400);

            grid.BackgroundColor =
                Color.FromArgb(27, 31, 39);

            grid.ForeColor =
                Color.White;

            grid.ColumnCount = 4;

            grid.Columns[0].Name = "PID";
            grid.Columns[1].Name = "PROCESS";
            grid.Columns[2].Name = "STATUS";
            grid.Columns[3].Name = "MEMORY";

            grid.Rows.Add(
                "001",
                "Kernel",
                "RUNNING",
                "256 KB");

            grid.Rows.Add(
                "002",
                "Shell",
                "RUNNING",
                "128 KB");

            grid.Rows.Add(
                "003",
                "Calculator",
                "WAITING",
                "64 KB");

            grid.Rows.Add(
                "004",
                "Time Service",
                "WAITING",
                "32 KB");

            Controls.Add(title);
            Controls.Add(grid);
        }
    }
}