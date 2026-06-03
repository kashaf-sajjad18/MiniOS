using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class ExplorerForm : Form
    {
        private ListView fileList;
        private TextBox pathBar;
        private Button btnBack, btnRefresh, btnNewFolder, btnDelete, btnRename, btnNewText, btnImageView;
        private Label lblStatus;
        private Timer refreshTimer;
        private string currentPath = "";
        private string documentsPath, downloadsPath, privatePath;
        private bool isPrivateUnlocked = false;
        private string privatePassword = "admin123";

        public ExplorerForm()
        {
            InitializeFolders();
            BuildUI();
            LoadPath(currentPath);
            StartRefresh();
            SystemLogForm.WriteLog("FILE", "admin", "File Explorer Opened", "User accessed files", "SUCCESS");
        }

        private void InitializeFolders()
        {
            documentsPath = Path.Combine(Application.StartupPath, "MiniOS_Documents");
            downloadsPath = Path.Combine(Application.StartupPath, "MiniOS_Downloads");
            privatePath = Path.Combine(Application.StartupPath, "MiniOS_Private");

            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);
            if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);
            if (!Directory.Exists(privatePath)) Directory.CreateDirectory(privatePath);

            string welcomeFile = Path.Combine(documentsPath, "Welcome.txt");
            if (!File.Exists(welcomeFile))
                File.WriteAllText(welcomeFile, "Welcome to MiniOS File System!\n\nYou can create, edit, and manage files here.");

            currentPath = documentsPath;
        }

        private void BuildUI()
        {
            this.Text = "MiniOS File Explorer";
            this.Size = new Size(950, 600);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Top Toolbar
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 50;
            toolbar.BackColor = Color.FromArgb(24, 30, 38);

            Label titleLabel = new Label();
            titleLabel.Text = "📁 MINIOS FILE SYSTEM";
            titleLabel.ForeColor = Color.DeepSkyBlue;
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.Location = new Point(15, 12);
            titleLabel.AutoSize = true;

            pathBar = new TextBox();
            pathBar.Location = new Point(250, 12);
            pathBar.Size = new Size(450, 30);
            pathBar.BackColor = Color.FromArgb(30, 36, 44);
            pathBar.ForeColor = Color.Lime;
            pathBar.Font = new Font("Consolas", 10);
            pathBar.ReadOnly = true;

            btnBack = CreateToolButton("◀", 720, 10);
            btnBack.Click += BtnBack_Click;
            btnRefresh = CreateToolButton("🔄", 760, 10);
            btnRefresh.Click += BtnRefresh_Click;

            toolbar.Controls.Add(titleLabel);
            toolbar.Controls.Add(pathBar);
            toolbar.Controls.Add(btnBack);
            toolbar.Controls.Add(btnRefresh);

            // Left Panel
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 220;
            leftPanel.BackColor = Color.FromArgb(20, 26, 34);

            Label quickLabel = new Label();
            quickLabel.Text = "MINIOS STORAGE";
            quickLabel.ForeColor = Color.DeepSkyBlue;
            quickLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            quickLabel.Location = new Point(15, 15);
            quickLabel.AutoSize = true;

            Button btnDocuments = CreateQuickButton("📄 Documents", 15, 50);
            btnDocuments.Click += (s, e) => LoadPath(documentsPath);
            Button btnDownloads = CreateQuickButton("⬇ Downloads", 15, 95);
            btnDownloads.Click += (s, e) => LoadPath(downloadsPath);
            Button btnPrivate = CreateQuickButton("🔒 Private Vault", 15, 140);
            btnPrivate.Click += (s, e) => AccessPrivateFolder(btnPrivate);
            Button btnNewFolderQuick = CreateQuickButton("📁 + New Folder", 15, 195);
            btnNewFolderQuick.Click += BtnNewFolder_Click;

            Label infoLabel = new Label();
            infoLabel.Text = "\n💡 INFO:\n• Private password:\n  admin123\n• Double-click .txt files\n• Images open in viewer";
            infoLabel.ForeColor = Color.Gray;
            infoLabel.Font = new Font("Segoe UI", 8);
            infoLabel.Location = new Point(15, 260);
            infoLabel.AutoSize = true;

            leftPanel.Controls.Add(quickLabel);
            leftPanel.Controls.Add(btnDocuments);
            leftPanel.Controls.Add(btnDownloads);
            leftPanel.Controls.Add(btnPrivate);
            leftPanel.Controls.Add(btnNewFolderQuick);
            leftPanel.Controls.Add(infoLabel);

            // Main Area
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);

            fileList = new ListView();
            fileList.Dock = DockStyle.Fill;
            fileList.View = View.Details;
            fileList.FullRowSelect = true;
            fileList.GridLines = true;
            fileList.BackColor = Color.FromArgb(27, 31, 39);
            fileList.ForeColor = Color.White;
            fileList.Font = new Font("Segoe UI", 10);
            fileList.MultiSelect = false;
            fileList.DoubleClick += FileList_DoubleClick;
            fileList.KeyDown += FileList_KeyDown;
            fileList.Columns.Add("Name", 350);
            fileList.Columns.Add("Size", 100, HorizontalAlignment.Right);
            fileList.Columns.Add("Type", 120);
            fileList.Columns.Add("Modified", 140);

            // Bottom Action Bar
            Panel actionBar = new Panel();
            actionBar.Dock = DockStyle.Bottom;
            actionBar.Height = 55;
            actionBar.BackColor = Color.FromArgb(24, 30, 38);

            btnNewFolder = CreateActionButton("📁 New Folder", 10, 10);
            btnNewFolder.Click += BtnNewFolder_Click;
            btnNewText = CreateActionButton("📝 New Text", 120, 10);
            btnNewText.Click += BtnNewText_Click;
            btnImageView = CreateActionButton("🖼 Image Viewer", 230, 10);
            btnImageView.Click += BtnImageView_Click;
            btnDelete = CreateActionButton("🗑 Delete", 350, 10);
            btnDelete.Click += BtnDelete_Click;
            btnRename = CreateActionButton("✏ Rename", 460, 10);
            btnRename.Click += BtnRename_Click;

            lblStatus = new Label();
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(580, 20);
            lblStatus.AutoSize = true;

            actionBar.Controls.Add(btnNewFolder);
            actionBar.Controls.Add(btnNewText);
            actionBar.Controls.Add(btnImageView);
            actionBar.Controls.Add(btnDelete);
            actionBar.Controls.Add(btnRename);
            actionBar.Controls.Add(lblStatus);

            mainPanel.Controls.Add(fileList);
            Controls.Add(mainPanel);
            Controls.Add(leftPanel);
            Controls.Add(toolbar);
            Controls.Add(actionBar);
        }

        private Button CreateToolButton(string text, int x, int y)
        {
            Button btn = new Button() { Text = text, Size = new Size(35, 32), Location = new Point(x, y), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 60, 70);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(35, 42, 52);
            return btn;
        }

        private Button CreateQuickButton(string text, int x, int y)
        {
            Button btn = new Button() { Text = text, Size = new Size(190, 40), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 36, 44), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(45, 55, 65);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(30, 36, 44);
            return btn;
        }

        private Button CreateActionButton(string text, int x, int y)
        {
            Button btn = new Button() { Text = text, Size = new Size(105, 38), Location = new Point(x, y), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 60, 70);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(35, 42, 52);
            return btn;
        }

        private void LoadPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            currentPath = path;
            pathBar.Text = GetDisplayPath(currentPath);
            fileList.Items.Clear();

            try
            {
                foreach (string dir in Directory.GetDirectories(currentPath))
                {
                    DirectoryInfo info = new DirectoryInfo(dir);
                    ListViewItem item = new ListViewItem("📁 " + info.Name);
                    item.SubItems.Add(""); item.SubItems.Add("Folder"); item.SubItems.Add(info.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = dir; fileList.Items.Add(item);
                }

                foreach (string file in Directory.GetFiles(currentPath))
                {
                    FileInfo info = new FileInfo(file);
                    string ext = info.Extension.ToLower();
                    string icon = GetFileIcon(ext);
                    ListViewItem item = new ListViewItem(icon + " " + info.Name);
                    item.SubItems.Add(FormatSize(info.Length));
                    item.SubItems.Add(GetFileType(ext));
                    item.SubItems.Add(info.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = file; fileList.Items.Add(item);
                }

                lblStatus.Text = $"{fileList.Items.Count} items | Total: {FormatSize(GetFolderSize(currentPath))}";
            }
            catch (Exception ex) { MessageBox.Show($"Cannot access: {ex.Message}", "Error"); }
        }

        private string GetDisplayPath(string path)
        {
            if (path == documentsPath) return "📄 My Documents";
            if (path == downloadsPath) return "⬇ Downloads";
            if (path == privatePath && isPrivateUnlocked) return "🔓 Private Vault (Unlocked)";
            if (path == privatePath) return "🔒 Private Vault (Locked)";
            return Path.GetFileName(path);
        }

        private string GetFileIcon(string ext)
        {
            if (ext == ".txt") return "📄";
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp") return "🖼";
            return "📄";
        }

        private string GetFileType(string ext)
        {
            if (ext == ".txt") return "Text";
            if (ext == ".jpg" || ext == ".jpeg") return "JPEG";
            if (ext == ".png") return "PNG";
            if (ext == ".gif") return "GIF";
            if (ext == ".bmp") return "BMP";
            return "File";
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)) + " MB";
            return (bytes / (1024 * 1024 * 1024)) + " GB";
        }

        private long GetFolderSize(string folderPath)
        {
            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folderPath)) size += new FileInfo(file).Length;
                foreach (string dir in Directory.GetDirectories(folderPath)) size += GetFolderSize(dir);
            }
            catch { }
            return size;
        }

        private void AccessPrivateFolder(Button btnPrivate)
        {
            if (!isPrivateUnlocked)
            {
                Form passForm = new Form() { Text = "Private Vault Access", Size = new Size(350, 150), BackColor = Color.FromArgb(16, 24, 32), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
                Label lbl = new Label() { Text = "Enter Private Vault Password:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
                TextBox txtPass = new TextBox() { Location = new Point(20, 50), Size = new Size(290, 25), PasswordChar = '*' };
                Button btnOk = new Button() { Text = "UNLOCK", Location = new Point(20, 85), Size = new Size(100, 30), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White };
                btnOk.Click += (s, ev) => {
                    if (txtPass.Text == privatePassword)
                    {
                        isPrivateUnlocked = true;
                        btnPrivate.Text = "🔓 Private Vault (Unlocked)";
                        btnPrivate.BackColor = Color.FromArgb(30, 100, 30);
                        CreateDummyFilesInPrivate();
                        LoadPath(privatePath);
                        passForm.Close();
                        SystemLogForm.WriteLog("PRIVATE", "admin", "Private Vault Unlocked", "Access granted", "SUCCESS");
                    }
                    else { MessageBox.Show("Invalid password!", "Access Denied"); }
                };
                passForm.Controls.Add(lbl); passForm.Controls.Add(txtPass); passForm.Controls.Add(btnOk);
                passForm.ShowDialog();
            }
            else LoadPath(privatePath);
        }

        private void CreateDummyFilesInPrivate()
        {
            try
            {
                string dummy1 = Path.Combine(privatePath, "SecretNotes.txt");
                if (!File.Exists(dummy1)) File.WriteAllText(dummy1, "🔒 CONFIDENTIAL - PRIVATE VAULT\n\nCreated: " + DateTime.Now);
                string dummy2 = Path.Combine(privatePath, "Passwords.txt");
                if (!File.Exists(dummy2)) File.WriteAllText(dummy2, "🔐 PASSWORD VAULT\n\nStore your sensitive information here.");
                string dummyFolder = Path.Combine(privatePath, "Backup");
                if (!Directory.Exists(dummyFolder)) Directory.CreateDirectory(dummyFolder);
            }
            catch { }
        }

        private void BtnNewFolder_Click(object sender, EventArgs e)
        {
            Form folderForm = new Form() { Text = "New Folder", Size = new Size(350, 120), BackColor = Color.FromArgb(16, 24, 32), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            Label lbl = new Label() { Text = "Folder name:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            TextBox txtName = new TextBox() { Location = new Point(20, 45), Size = new Size(290, 25), Text = "NewFolder" };
            Button btnOk = new Button() { Text = "CREATE", Location = new Point(20, 80), Size = new Size(100, 30), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White };
            btnOk.Click += (s, ev) => {
                string folderName = txtName.Text.Trim();
                if (!string.IsNullOrEmpty(folderName))
                {
                    string newPath = Path.Combine(currentPath, folderName);
                    if (!Directory.Exists(newPath)) { Directory.CreateDirectory(newPath); LoadPath(currentPath); folderForm.Close(); }
                    else MessageBox.Show("Folder already exists!", "Error");
                }
            };
            folderForm.Controls.Add(lbl); folderForm.Controls.Add(txtName); folderForm.Controls.Add(btnOk);
            folderForm.ShowDialog();
        }

        private void BtnNewText_Click(object sender, EventArgs e)
        {
            string fileName = $"NewFile_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(currentPath, fileName);
            File.WriteAllText(filePath, $"Created: {DateTime.Now}\n\nWrite your content here...");
            LoadPath(currentPath);
            SystemLogForm.WriteLog("FILE", "admin", "Text File Created", fileName, "SUCCESS");
            TextEditorForm editor = new TextEditorForm(filePath);
            editor.Show();
        }

        private void BtnImageView_Click(object sender, EventArgs e)
        {
            ImageViewerForm viewer = new ImageViewerForm(currentPath);
            viewer.Show();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) { MessageBox.Show("Select an item to delete!", "Info"); return; }
            string path = fileList.SelectedItems[0].Tag.ToString();
            bool isFolder = Directory.Exists(path);
            string name = fileList.SelectedItems[0].Text.Substring(2);
            if (MessageBox.Show($"Delete '{name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try { if (isFolder) Directory.Delete(path, true); else File.Delete(path); LoadPath(currentPath); }
                catch (Exception ex) { MessageBox.Show($"Cannot delete: {ex.Message}", "Error"); }
            }
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) { MessageBox.Show("Select an item to rename!", "Info"); return; }
            string oldPath = fileList.SelectedItems[0].Tag.ToString();
            string oldName = fileList.SelectedItems[0].Text.Substring(2);
            Form renameForm = new Form() { Text = "Rename", Size = new Size(350, 120), BackColor = Color.FromArgb(16, 24, 32), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            Label lbl = new Label() { Text = "New name:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            TextBox txtName = new TextBox() { Location = new Point(20, 45), Size = new Size(290, 25), Text = oldName };
            Button btnOk = new Button() { Text = "RENAME", Location = new Point(20, 80), Size = new Size(100, 30), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White };
            btnOk.Click += (s, ev) => {
                string newName = txtName.Text.Trim();
                if (!string.IsNullOrEmpty(newName) && newName != oldName)
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                    try { if (Directory.Exists(oldPath)) Directory.Move(oldPath, newPath); else File.Move(oldPath, newPath); LoadPath(currentPath); renameForm.Close(); }
                    catch (Exception ex) { MessageBox.Show($"Cannot rename: {ex.Message}", "Error"); }
                }
            };
            renameForm.Controls.Add(lbl); renameForm.Controls.Add(txtName); renameForm.Controls.Add(btnOk);
            renameForm.ShowDialog();
        }

        private void FileList_DoubleClick(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) return;
            string path = fileList.SelectedItems[0].Tag.ToString();
            if (Directory.Exists(path)) LoadPath(path);
            else if (File.Exists(path))
            {
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".txt") new TextEditorForm(path).Show();
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp") new ImageViewerForm(Path.GetDirectoryName(path)).Show();
                else System.Diagnostics.Process.Start(path);
            }
        }

        private void FileList_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Delete) BtnDelete_Click(null, null); else if (e.KeyCode == Keys.F2) BtnRename_Click(null, null); }
        private void BtnBack_Click(object sender, EventArgs e) { string parent = Directory.GetParent(currentPath)?.FullName; if (parent != null) LoadPath(parent); }
        private void BtnRefresh_Click(object sender, EventArgs e) { LoadPath(currentPath); }
        private void StartRefresh() { refreshTimer = new Timer(); refreshTimer.Interval = 3000; refreshTimer.Tick += (s, e) => { if (this.Visible) LoadPath(currentPath); }; refreshTimer.Start(); }
    }
}