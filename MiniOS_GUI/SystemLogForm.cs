using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class SystemLogForm : Form
    {
        private ListView logListView;
        private Button btnRefresh;
        private Button btnClear;
        private Button btnExport;
        private ComboBox filterCombo;
        private Label lblStatus;
        private Timer refreshTimer;

        // Log file path
        private string logFile = Application.StartupPath + "\\MiniOS_Log.txt";

        public SystemLogForm()
        {
            // Default window size - PEHLE SET KARO
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 650);
            this.MinimumSize = new Size(900, 500);

            BuildUI();
            LoadLogs();
            StartAutoRefresh();
        }

        private void BuildUI()
        {
            this.Text = "MiniOS System Log";
            this.Size = new Size(1100, 650);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title
            Label title = new Label();
            title.Text = "📋 SYSTEM LOG MONITOR";
            title.ForeColor = Color.DeepSkyBlue;
            title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            title.Location = new Point(20, 15);
            title.AutoSize = true;

            // Filter section
            Label filterLabel = new Label();
            filterLabel.Text = "Filter:";
            filterLabel.ForeColor = Color.White;
            filterLabel.Font = new Font("Segoe UI", 10);
            filterLabel.Location = new Point(20, 60);
            filterLabel.AutoSize = true;

            filterCombo = new ComboBox();
            filterCombo.Items.AddRange(new string[] {
                "All Logs",
                "Login",
                "Commands",
                "Private Folder",
                "Files",
                "System",
                "Errors"
            });
            filterCombo.SelectedIndex = 0;
            filterCombo.Size = new Size(150, 25);
            filterCombo.Location = new Point(80, 57);
            filterCombo.BackColor = Color.FromArgb(35, 42, 52);
            filterCombo.ForeColor = Color.White;
            filterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            filterCombo.SelectedIndexChanged += FilterCombo_SelectedIndexChanged;

            // Status label
            lblStatus = new Label();
            lblStatus.Text = "Total Logs: 0";
            lblStatus.ForeColor = Color.Lime;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Location = new Point(250, 62);
            lblStatus.AutoSize = true;

            // ListView for logs
            logListView = new ListView();
            logListView.Location = new Point(20, 95);
            logListView.Size = new Size(1050, 450);
            logListView.View = View.Details;
            logListView.FullRowSelect = true;
            logListView.GridLines = true;
            logListView.BackColor = Color.FromArgb(27, 31, 39);
            logListView.ForeColor = Color.White;
            logListView.Font = new Font("Consolas", 9);

            // Columns
            logListView.Columns.Add("Time", 150, HorizontalAlignment.Left);
            logListView.Columns.Add("Type", 100, HorizontalAlignment.Center);
            logListView.Columns.Add("User", 100, HorizontalAlignment.Left);
            logListView.Columns.Add("Action", 200, HorizontalAlignment.Left);
            logListView.Columns.Add("Details", 350, HorizontalAlignment.Left);
            logListView.Columns.Add("Status", 100, HorizontalAlignment.Center);

            // Buttons
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 REFRESH";
            btnRefresh.Size = new Size(120, 35);
            btnRefresh.Location = new Point(20, 560);
            btnRefresh.BackColor = Color.FromArgb(35, 42, 52);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += BtnRefresh_Click;

            btnClear = new Button();
            btnClear.Text = "🗑 CLEAR LOGS";
            btnClear.Size = new Size(120, 35);
            btnClear.Location = new Point(150, 560);
            btnClear.BackColor = Color.FromArgb(100, 50, 30);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Click += BtnClear_Click;

            btnExport = new Button();
            btnExport.Text = "💾 EXPORT LOGS";
            btnExport.Size = new Size(120, 35);
            btnExport.Location = new Point(280, 560);
            btnExport.BackColor = Color.FromArgb(30, 100, 30);
            btnExport.ForeColor = Color.White;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Click += BtnExport_Click;

            // Legend
            Label legend = new Label();
            legend.Text = "🟢 Login  |  🔵 Command  |  🟡 Private  |  🟠 File  |  🔴 Error  |  ⚪ System";
            legend.ForeColor = Color.Gray;
            legend.Font = new Font("Segoe UI", 9);
            legend.Location = new Point(20, 605);
            legend.AutoSize = true;

            Controls.Add(title);
            Controls.Add(filterLabel);
            Controls.Add(filterCombo);
            Controls.Add(lblStatus);
            Controls.Add(logListView);
            Controls.Add(btnRefresh);
            Controls.Add(btnClear);
            Controls.Add(btnExport);
            Controls.Add(legend);
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += (s, e) => LoadLogs();
            refreshTimer.Start();
        }

        private void LoadLogs()
        {
            logListView.Items.Clear();

            if (!File.Exists(logFile))
            {
                lblStatus.Text = "Total Logs: 0";
                return;
            }

            string[] lines = File.ReadAllLines(logFile);
            int count = 0;
            string filter = filterCombo.SelectedItem.ToString();

            for (int i = lines.Length - 1; i >= 0; i--) // Show newest first
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split('|');
                if (parts.Length >= 5)
                {
                    string time = parts[0];
                    string type = parts[1];
                    string user = parts[2];
                    string action = parts[3];
                    string details = parts[4];
                    string status = parts.Length > 5 ? parts[5] : "SUCCESS";

                    // Apply filter
                    if (filter != "All Logs" && !type.Contains(filter))
                        continue;

                    ListViewItem item = new ListViewItem(time);
                    item.SubItems.Add(GetTypeIcon(type) + " " + type);
                    item.SubItems.Add(user);
                    item.SubItems.Add(action);
                    item.SubItems.Add(details);
                    item.SubItems.Add(status);

                    // Color based on type
                    if (type == "LOGIN")
                        item.ForeColor = Color.Lime;
                    else if (type == "COMMAND")
                        item.ForeColor = Color.Cyan;
                    else if (type == "PRIVATE")
                        item.ForeColor = Color.Yellow;
                    else if (type == "FILE")
                        item.ForeColor = Color.Orange;
                    else if (type == "ERROR")
                        item.ForeColor = Color.Red;
                    else
                        item.ForeColor = Color.White;

                    logListView.Items.Add(item);
                    count++;
                }
            }

            lblStatus.Text = $"Total Logs: {count}";
        }

        private string GetTypeIcon(string type)
        {
            switch (type)
            {
                case "LOGIN": return "🟢";
                case "COMMAND": return "🔵";
                case "PRIVATE": return "🟡";
                case "FILE": return "🟠";
                case "ERROR": return "🔴";
                default: return "⚪";
            }
        }

        private void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadLogs();
            lblStatus.Text = $"Refreshed at {DateTime.Now:HH:mm:ss}";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Clear all system logs?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                File.WriteAllText(logFile, "");
                LoadLogs();
                WriteLog("SYSTEM", "admin", "Logs Cleared", "All system logs were cleared", "SUCCESS");
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Text File|*.txt|CSV File|*.csv";
            saveDialog.Title = "Export System Logs";
            saveDialog.FileName = $"MiniOS_Log_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(logFile))
                {
                    File.Copy(logFile, saveDialog.FileName, true);
                    MessageBox.Show($"Logs exported to:\n{saveDialog.FileName}", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Public method to write log from anywhere
        public static void WriteLog(string type, string user, string action, string details, string status = "SUCCESS")
        {
            string logFile = Application.StartupPath + "\\MiniOS_Log.txt";
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{type}|{user}|{action}|{details}|{status}\n";

            try
            {
                File.AppendAllText(logFile, logEntry);
            }
            catch { }
        }
    }
}