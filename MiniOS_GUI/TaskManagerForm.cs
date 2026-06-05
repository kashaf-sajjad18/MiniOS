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
        private Button btnRefresh, btnTerminate, btnStart;

        private bool calculatorRunning = false;
        private bool timeServiceRunning = false;

        public TaskManagerForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(620, 500);
            this.MinimumSize = new Size(620, 500);
            this.MaximumSize = new Size(620, 500);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            BuildUI();
            RefreshProcessList();
            StartAutoRefresh();
        }

        private void BuildUI()
        {
            this.Text = "MiniOS Task Manager";

            Label title = new Label();
            title.Text = "TASK MANAGER";
            title.ForeColor = Color.DeepSkyBlue;
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.Location = new Point(15, 10);
            title.AutoSize = true;

            lblStatus = new Label();
            lblStatus.Text = "Status: Ready";
            lblStatus.ForeColor = Color.Lime;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(15, 45);
            lblStatus.Size = new Size(580, 20);

            listView = new ListView();
            listView.Location = new Point(15, 70);
            listView.Size = new Size(580, 310);
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.BackColor = Color.FromArgb(27, 31, 39);
            listView.ForeColor = Color.White;
            listView.Font = new Font("Consolas", 10);
            listView.MultiSelect = false;
            listView.HideSelection = false;
            listView.Activation = ItemActivation.OneClick;

            listView.SelectedIndexChanged += ListView_SelectedIndexChanged;

            listView.Columns.Clear();
            listView.Columns.Add("PID", 60, HorizontalAlignment.Center);
            listView.Columns.Add("PROCESS", 160, HorizontalAlignment.Left);
            listView.Columns.Add("STATUS", 110, HorizontalAlignment.Center);
            listView.Columns.Add("MEMORY", 100, HorizontalAlignment.Right);

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
            legend.Text = "🟢 Running  |  🔴 Terminated  |  Click to select → Then press TERMINATE or START";
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

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                string selected = listView.SelectedItems[0].SubItems[1].Text;
                selected = selected.Replace("🟢 ", "").Replace("🔴 ", "");
                string status = listView.SelectedItems[0].SubItems[2].Text;

                lblStatus.Text = $"Selected: {selected} ({status}) | Press TERMINATE or START";
                lblStatus.ForeColor = Color.Cyan;
            }
            else
            {
                lblStatus.Text = "No process selected | Click on a process to select it";
                lblStatus.ForeColor = Color.Gray;
            }
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += (s, e) => RefreshProcessList();
            refreshTimer.Start();
        }

        private void SyncFromStorage()
        {
            MemoryStorage.LoadMemory();
            calculatorRunning = MemoryStorage.CalculatorRunning;
            timeServiceRunning = MemoryStorage.TimeRunning;
        }

        private void RefreshProcessList()
        {
            SyncFromStorage();

            string selectedPid = null;
            if (listView.SelectedItems.Count > 0)
            {
                selectedPid = listView.SelectedItems[0].Text;
            }

            listView.Items.Clear();

            int usedMem = MemoryStorage.GetUsedRAM();
            int percentage = MemoryStorage.GetPercentage();

            int runningCount = 2;
            if (calculatorRunning) runningCount++;
            if (timeServiceRunning) runningCount++;

            lblStatus.Text = $"🖥️ {runningCount}/4 Running | RAM: {FormatKB(usedMem)} / {FormatKB(MemoryStorage.TotalRAM)} ({percentage}%)";

            AddProcessItem("001", "Kernel", "RUNNING", FormatKB(MemoryStorage.KernelMem), true);
            AddProcessItem("002", "Shell", "RUNNING", FormatKB(MemoryStorage.ShellMem), true);

            string calcStatus = calculatorRunning ? "RUNNING" : "TERMINATED";
            string calcMemStr = calculatorRunning ? FormatKB(MemoryStorage.CalculatorMem) : "0 KB";
            AddProcessItem("003", "Calculator", calcStatus, calcMemStr, calculatorRunning);

            string timeStatus = timeServiceRunning ? "RUNNING" : "TERMINATED";
            string timeMemStr = timeServiceRunning ? FormatKB(MemoryStorage.TimeMem) : "0 KB";
            AddProcessItem("004", "Time Service", timeStatus, timeMemStr, timeServiceRunning);

            if (selectedPid != null)
            {
                foreach (ListViewItem item in listView.Items)
                {
                    if (item.Text == selectedPid)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }

        private string FormatKB(int kb)
        {
            if (kb < 1024) return kb + " KB";
            return (kb / 1024) + " MB";
        }

        private void AddProcessItem(string pid, string name, string status, string memory, bool isRunning)
        {
            ListViewItem item = new ListViewItem(pid);
            string icon = isRunning ? "🟢 " : "🔴 ";
            item.SubItems.Add(icon + name);
            item.SubItems.Add(status);
            item.SubItems.Add(memory);
            item.ForeColor = isRunning ? Color.Lime : Color.Red;
            listView.Items.Add(item);
        }

        private void UpdateMemoryForms()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is MemoryForm memoryForm)
                {
                    memoryForm.SetCalculatorStatus(calculatorRunning);
                    memoryForm.SetTimeStatus(timeServiceRunning);
                    memoryForm.RefreshMemory();
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
                MessageBox.Show("Please SELECT a process first!\n\nClick on any process to select it, then click TERMINATE.",
                    "Task Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = listView.SelectedItems[0].SubItems[1].Text;
            selected = selected.Replace("🟢 ", "").Replace("🔴 ", "");
            string status = listView.SelectedItems[0].SubItems[2].Text;

            if (status == "TERMINATED")
            {
                MessageBox.Show($"{selected} is already terminated!", "Task Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected == "Kernel" || selected == "Shell")
            {
                MessageBox.Show($"Cannot terminate {selected} - Critical system process!",
                    "Task Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show($"Terminate {selected} process?\n\nThis will free up memory.",
                "Confirm Termination", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (selected == "Calculator")
                {
                    MemoryStorage.StopCalculator();
                    SystemLogForm.WriteLog("TASK", "admin", "Process Terminated", "Calculator", "SUCCESS");
                }
                else if (selected == "Time Service")
                {
                    MemoryStorage.StopTimeService();
                    SystemLogForm.WriteLog("TASK", "admin", "Process Terminated", "Time Service", "SUCCESS");
                }

                RefreshProcessList();
                UpdateMemoryForms();
                lblStatus.Text = $"✓ {selected} terminated at {DateTime.Now:HH:mm:ss}";
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please SELECT a process first!\n\nClick on any process to select it, then click START.",
                    "Task Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = listView.SelectedItems[0].SubItems[1].Text;
            selected = selected.Replace("🟢 ", "").Replace("🔴 ", "");
            string status = listView.SelectedItems[0].SubItems[2].Text;

            if (selected == "Kernel" || selected == "Shell")
            {
                MessageBox.Show($"{selected} is always running!", "Task Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (status == "RUNNING")
            {
                MessageBox.Show($"{selected} is already running!", "Task Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selected == "Calculator")
            {
                MemoryStorage.StartCalculator();
                SystemLogForm.WriteLog("TASK", "admin", "Process Started", "Calculator", "SUCCESS");
            }
            else if (selected == "Time Service")
            {
                MemoryStorage.StartTimeService();
                SystemLogForm.WriteLog("TASK", "admin", "Process Started", "Time Service", "SUCCESS");
            }

            RefreshProcessList();
            UpdateMemoryForms();
            lblStatus.Text = $"✓ {selected} started at {DateTime.Now:HH:mm:ss}";
        }

        public void StartCalculator() { MemoryStorage.StartCalculator(); RefreshProcessList(); UpdateMemoryForms(); }
        public void StopCalculator() { MemoryStorage.StopCalculator(); RefreshProcessList(); UpdateMemoryForms(); }
        public void StartTimeService() { MemoryStorage.StartTimeService(); RefreshProcessList(); UpdateMemoryForms(); }
        public void StopTimeService() { MemoryStorage.StopTimeService(); RefreshProcessList(); UpdateMemoryForms(); }
        public bool IsCalculatorRunning() { return MemoryStorage.CalculatorRunning; }
        public bool IsTimeServiceRunning() { return MemoryStorage.TimeRunning; }
    }
}