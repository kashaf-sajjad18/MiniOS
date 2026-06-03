using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class TaskManagerForm : Form
    {
        private ListView listView;
        private Timer refreshTimer;
        private Label lblStatus;
        private Button btnRefresh;
        private Button btnTerminate;
        private Button btnStart;

        // Process status tracking
        private bool calculatorRunning = false;
        private bool timeServiceRunning = false;
        private bool shellRunning = true;
        private bool kernelRunning = true;

        // Memory values
        private int kernelMem = 256;
        private int shellMem = 128;
        private int calculatorMem = 64;
        private int timeMem = 32;
        private int taskMem = 64;
        private int totalRAM = 4096;

        public TaskManagerForm()
        {
            BuildUI();
            RefreshProcessList();
            StartAutoRefresh();
        }

        private void BuildUI()
        {
            this.Text = "MiniOS Task Manager";
            this.Size = new Size(600, 480);  // Clean size
            this.MinimumSize = new Size(600, 480);
            this.MaximumSize = new Size(600, 480);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;  // Fixed size

            Label title = new Label();
            title.Text = "TASK MANAGER";
            title.ForeColor = Color.DeepSkyBlue;
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(15, 10);
            title.AutoSize = true;

            lblStatus = new Label();
            lblStatus.Text = "Status: Running";
            lblStatus.ForeColor = Color.Lime;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(15, 45);
            lblStatus.Size = new Size(550, 20);

            // ListView - Clean UI
            listView = new ListView();
            listView.Location = new Point(15, 70);
            listView.Size = new Size(560, 310);
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.BackColor = Color.FromArgb(27, 31, 39);
            listView.ForeColor = Color.White;
            listView.Font = new Font("Consolas", 10);
            listView.MultiSelect = false;
            listView.HideSelection = false;
           

            // Columns - 4 columns only
            listView.Columns.Add("PID", 50, HorizontalAlignment.Center);
            listView.Columns.Add("PROCESS", 160, HorizontalAlignment.Left);
            listView.Columns.Add("STATUS", 100, HorizontalAlignment.Center);
            listView.Columns.Add("MEMORY", 100, HorizontalAlignment.Right);

            // Buttons
            btnRefresh = new Button();
            btnRefresh.Text = "REFRESH";
            btnRefresh.Size = new Size(90, 32);
            btnRefresh.Location = new Point(15, 395);
            btnRefresh.BackColor = Color.FromArgb(35, 42, 52);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            btnTerminate = new Button();
            btnTerminate.Text = "TERMINATE";
            btnTerminate.Size = new Size(90, 32);
            btnTerminate.Location = new Point(115, 395);
            btnTerminate.BackColor = Color.FromArgb(100, 30, 30);
            btnTerminate.ForeColor = Color.White;
            btnTerminate.FlatStyle = FlatStyle.Flat;
            btnTerminate.Click += BtnTerminate_Click;

            btnStart = new Button();
            btnStart.Text = "START";
            btnStart.Size = new Size(90, 32);
            btnStart.Location = new Point(215, 395);
            btnStart.BackColor = Color.FromArgb(30, 100, 30);
            btnStart.ForeColor = Color.White;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Click += BtnStart_Click;

            Label legend = new Label();
            legend.Text = "🟢 Running  |  🔴 Terminated";
            legend.ForeColor = Color.Gray;
            legend.Font = new Font("Segoe UI", 8);
            legend.Location = new Point(15, 440);
            legend.AutoSize = true;

            Controls.Add(title);
            Controls.Add(lblStatus);
            Controls.Add(listView);
            Controls.Add(btnRefresh);
            Controls.Add(btnTerminate);
            Controls.Add(btnStart);
            Controls.Add(legend);
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += (s, e) => RefreshProcessList();
            refreshTimer.Start();
        }

        private void RefreshProcessList()
        {
            listView.Items.Clear();

            // Calculate used memory dynamically
            int usedMem = kernelMem + shellMem + taskMem;
            if (calculatorRunning) usedMem += calculatorMem;
            if (timeServiceRunning) usedMem += timeMem;

            int freeMem = totalRAM - usedMem;
            int percentage = (usedMem * 100) / totalRAM;

            // Update status label with real memory info
            lblStatus.Text = $"🖥️ {(calculatorRunning ? 3 : 2)}/4 Running  |  💾 RAM: {usedMem}KB / {totalRAM}KB ({percentage}%)  |  📀 Free: {freeMem}KB";

            // KERNEL - Always RUNNING
            AddProcessItem("001", "Kernel", kernelRunning ? "RUNNING" : "TERMINATED", $"{kernelMem} KB", kernelRunning);

            // SHELL - Always RUNNING
            AddProcessItem("002", "Shell", shellRunning ? "RUNNING" : "TERMINATED", $"{shellMem} KB", shellRunning);

            // CALCULATOR - Dynamic
            string calcStatus = calculatorRunning ? "RUNNING" : "TERMINATED";
            string calcMem = calculatorRunning ? $"{calculatorMem} KB" : "0 KB";
            AddProcessItem("003", "Calculator", calcStatus, calcMem, calculatorRunning);

            // TIME SERVICE - Dynamic
            string timeStatus = timeServiceRunning ? "RUNNING" : "TERMINATED";
            string timeMemStr = timeServiceRunning ? $"{timeMem} KB" : "0 KB";
            AddProcessItem("004", "Time Service", timeStatus, timeMemStr, timeServiceRunning);

            // Update MemoryForm if open
            UpdateMemoryForm();
        }

        private void AddProcessItem(string pid, string name, string status, string memory, bool isRunning)
        {
            ListViewItem item = new ListViewItem(pid);
            string icon = isRunning ? "🟢 " : "🔴 ";
            item.SubItems.Add(icon + name);
            item.SubItems.Add(status);
            item.SubItems.Add(memory);

            if (isRunning)
                item.ForeColor = Color.Lime;
            else
                item.ForeColor = Color.Red;

            listView.Items.Add(item);
        }

        private void UpdateMemoryForm()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is MemoryForm memoryForm)
                {
                    memoryForm.SetCalculatorStatus(calculatorRunning);
                    memoryForm.SetTimeStatus(timeServiceRunning);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        private void BtnTerminate_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a process first!", "Task Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = listView.SelectedItems[0].SubItems[1].Text
                .Replace("🟢 ", "").Replace("🔴 ", "");
            string status = listView.SelectedItems[0].SubItems[2].Text;

            if (status == "TERMINATED")
            {
                MessageBox.Show($"{selected} is already terminated!", "Task Manager");
                return;
            }

            if (selected == "Kernel" || selected == "Shell")
            {
                MessageBox.Show($"Cannot terminate {selected} - Critical system process!", "Task Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show($"Terminate {selected} process?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (selected == "Calculator")
                {
                    calculatorRunning = false;
                    UpdateTerminalForm(false);
                }
                else if (selected == "Time Service")
                    timeServiceRunning = false;

                RefreshProcessList();
                SystemLogForm.WriteLog("TASK", "admin", "Process Terminated", selected, "SUCCESS");
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a process first!", "Task Manager");
                return;
            }

            string selected = listView.SelectedItems[0].SubItems[1].Text
                .Replace("🟢 ", "").Replace("🔴 ", "");
            string status = listView.SelectedItems[0].SubItems[2].Text;

            if (selected == "Kernel" || selected == "Shell")
            {
                MessageBox.Show($"{selected} is always running!", "Task Manager");
                return;
            }

            if (status == "RUNNING")
            {
                MessageBox.Show($"{selected} is already running!", "Task Manager");
                return;
            }

            if (selected == "Calculator")
            {
                calculatorRunning = true;
                UpdateTerminalForm(true);
            }
            else if (selected == "Time Service")
            {
                timeServiceRunning = true;
            }

            RefreshProcessList();
            SystemLogForm.WriteLog("TASK", "admin", "Process Started", selected, "SUCCESS");
        }

        private void UpdateTerminalForm(bool isRunning)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is TerminalForm terminal)
                {
                    if (isRunning)
                        terminal.StartCalculator();
                    else
                        terminal.StopCalculator();
                }
            }
        }

        // Public methods for external control
        public void StartCalculator() { calculatorRunning = true; RefreshProcessList(); }
        public void StopCalculator() { calculatorRunning = false; RefreshProcessList(); }
        public void StartTimeService() { timeServiceRunning = true; RefreshProcessList(); }
        public void StopTimeService() { timeServiceRunning = false; RefreshProcessList(); }
        public bool IsCalculatorRunning() { return calculatorRunning; }
    }
}