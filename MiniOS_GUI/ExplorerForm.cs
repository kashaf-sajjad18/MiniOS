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
        private Button btnBack, btnRefresh, btnDelete, btnRename, btnNewText;
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
        }

        private void InitializeFolders()
        {
            documentsPath = Path.Combine(Application.StartupPath, "MiniOS_Documents");
            downloadsPath = Path.Combine(Application.StartupPath, "MiniOS_Downloads");
            privatePath = Path.Combine(Application.StartupPath, "MiniOS_Private");

            // Create folders directly - no blocking backend call at startup
            if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);
            if (!Directory.Exists(downloadsPath)) Directory.CreateDirectory(downloadsPath);
            if (!Directory.Exists(privatePath)) Directory.CreateDirectory(privatePath);

            currentPath = documentsPath;
        }

        private void BuildUI()
        {
            this.Text = "NOVA-OS File Explorer";
            this.Size = new Size(950, 600);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 45;
            toolbar.BackColor = Color.FromArgb(24, 30, 38);

            Label titleLabel = new Label();
            titleLabel.Text = "📁 NOVA-OS FILE SYSTEM";
            titleLabel.ForeColor = Color.DeepSkyBlue;
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.Location = new Point(10, 12);
            titleLabel.AutoSize = true;

            pathBar = new TextBox();
            pathBar.Location = new Point(200, 10);
            pathBar.Size = new Size(450, 25);
            pathBar.BackColor = Color.FromArgb(30, 36, 44);
            pathBar.ForeColor = Color.Lime;
            pathBar.Font = new Font("Consolas", 10);
            pathBar.ReadOnly = true;

            btnBack = CreateToolButton("◀", 660, 8);
            btnBack.Click += BtnBack_Click;
            btnRefresh = CreateToolButton("🔄", 695, 8);
            btnRefresh.Click += BtnRefresh_Click;

            toolbar.Controls.Add(titleLabel);
            toolbar.Controls.Add(pathBar);
            toolbar.Controls.Add(btnBack);
            toolbar.Controls.Add(btnRefresh);

            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 200;
            leftPanel.BackColor = Color.FromArgb(20, 26, 34);

            Label quickLabel = new Label();
            quickLabel.Text = "NOVA-OS STORAGE";
            quickLabel.ForeColor = Color.DeepSkyBlue;
            quickLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            quickLabel.Location = new Point(10, 15);
            quickLabel.AutoSize = true;

            Button btnDocuments = CreateQuickButton("📄 Documents", 10, 45);
            btnDocuments.Click += (s, e) => LoadPath(documentsPath);

            Button btnDownloads = CreateQuickButton("⬇ Downloads", 10, 85);
            btnDownloads.Click += (s, e) => LoadPath(downloadsPath);

            Button btnPrivate = CreateQuickButton("🔒 Private Vault", 10, 125);
            btnPrivate.Click += (s, e) => AccessPrivateFolder(btnPrivate);

            Button btnNewFolder = CreateQuickButton("📁 + New Folder", 10, 170);
            btnNewFolder.Click += BtnNewFolder_Click;

            Label infoLabel = new Label();
            infoLabel.Text = "\n💡 Info:\n• Private password: admin123";
            infoLabel.ForeColor = Color.Gray;
            infoLabel.Font = new Font("Segoe UI", 8);
            infoLabel.Location = new Point(10, 220);
            infoLabel.AutoSize = true;

            leftPanel.Controls.Add(quickLabel);
            leftPanel.Controls.Add(btnDocuments);
            leftPanel.Controls.Add(btnDownloads);
            leftPanel.Controls.Add(btnPrivate);
            leftPanel.Controls.Add(btnNewFolder);
            leftPanel.Controls.Add(infoLabel);

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
            fileList.HideSelection = false;
            fileList.Activation = ItemActivation.OneClick;
            fileList.DoubleClick += FileList_DoubleClick;
            fileList.KeyDown += FileList_KeyDown;

            fileList.Columns.Add("Name", 350);
            fileList.Columns.Add("Size", 100, HorizontalAlignment.Right);
            fileList.Columns.Add("Type", 100);
            fileList.Columns.Add("Modified", 130);

            Panel actionBar = new Panel();
            actionBar.Dock = DockStyle.Bottom;
            actionBar.Height = 50;
            actionBar.BackColor = Color.FromArgb(24, 30, 38);

            btnNewText = CreateActionButton("📝 New Text", 10, 8);
            btnNewText.Click += BtnNewText_Click;

            btnDelete = CreateActionButton("🗑 Delete", 120, 8);
            btnDelete.Click += BtnDelete_Click;

            btnRename = CreateActionButton("✏ Rename", 230, 8);
            btnRename.Click += BtnRename_Click;

            lblStatus = new Label();
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(350, 18);
            lblStatus.AutoSize = true;

            actionBar.Controls.Add(btnNewText);
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
            return new Button { Text = text, Size = new Size(30, 30), Location = new Point(x, y), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        private Button CreateQuickButton(string text, int x, int y)
        {
            return new Button { Text = text, Size = new Size(180, 35), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 36, 44), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        private Button CreateActionButton(string text, int x, int y)
        {
            return new Button { Text = text, Size = new Size(100, 35), Location = new Point(x, y), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        // ── Get short folder name for passing to backend (relative) ──
        private string FolderToken(string fullPath)
        {
            if (fullPath.StartsWith(documentsPath)) return "MiniOS_Documents";
            if (fullPath.StartsWith(downloadsPath)) return "MiniOS_Downloads";
            if (fullPath.StartsWith(privatePath)) return "MiniOS_Private";
            // subfolder: manually compute relative path (compatible with .NET Framework 4.7.2)
            string basePath = Application.StartupPath.TrimEnd('\\') + "\\";
            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(basePath.Length);
            return fullPath; // fallback: return as-is
        }

        public void LoadPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                if (path != null && path.Contains("Private") && !isPrivateUnlocked)
                {
                    lblStatus.Text = "🔒 Private folder locked";
                    return;
                }
                return;
            }

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

                long totalSize = GetFolderSize(currentPath);
                lblStatus.Text = $"{fileList.Items.Count} items | Total: {FormatSize(totalSize)}";
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
        }

        private string GetFileIcon(string ext)
        {
            if (ext == ".txt") return "📄";
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp") return "🖼️";
            return "📄";
        }

        private string GetFileType(string ext)
        {
            if (ext == ".txt") return "Text";
            if (ext == ".png") return "PNG";
            if (ext == ".jpg" || ext == ".jpeg") return "JPEG";
            if (ext == ".gif") return "GIF";
            if (ext == ".bmp") return "BMP";
            return "File";
        }

        private string GetDisplayPath(string path)
        {
            if (path == documentsPath) return "📄 My Documents";
            if (path == downloadsPath) return "⬇ Downloads";
            if (path == privatePath && isPrivateUnlocked) return "🔓 Private Vault (Unlocked)";
            if (path == privatePath) return "🔒 Private Vault (Locked)";
            return Path.GetFileName(path);
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

        // ── Private vault: password verified via Login.asm / FileExplorer.asm ──
        private void AccessPrivateFolder(Button btnPrivate)
        {
            if (!isPrivateUnlocked)
            {
                Form passForm = new Form();
                passForm.Text = "Private Vault";
                passForm.Size = new Size(400, 180);
                passForm.BackColor = Color.FromArgb(16, 24, 32);
                passForm.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label { Text = "Enter Password:", ForeColor = Color.White, Location = new Point(25, 25), Size = new Size(100, 25) };

                TextBox txtPass = new TextBox { Location = new Point(25, 55), Size = new Size(340, 25), PasswordChar = '*' };

                Button btnOk = new Button { Text = "UNLOCK", Location = new Point(100, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnCancel = new Button { Text = "CANCEL", Location = new Point(220, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

                btnOk.Click += (s, ev) =>
                {
                    // BACKEND: Assembly checks the password.
                    string result = MiniOSBackend.RunToString($"explorer checkpass {txtPass.Text}");

                    if (result.Contains("[ACCESS GRANTED]"))
                    {
                        isPrivateUnlocked = true;
                        btnPrivate.Text = "🔓 Private Vault (Unlocked)";
                        btnPrivate.BackColor = Color.FromArgb(30, 100, 30);
                        LoadPath(privatePath);
                        passForm.Close();
                    }
                    else
                    {
                        MessageBox.Show("Wrong password!");
                    }
                };
                btnCancel.Click += (s, ev) => passForm.Close();

                passForm.Controls.Add(lbl);
                passForm.Controls.Add(txtPass);
                passForm.Controls.Add(btnOk);
                passForm.Controls.Add(btnCancel);
                passForm.ShowDialog();
            }
            else LoadPath(privatePath);
        }

        private void BtnNewFolder_Click(object sender, EventArgs e)
        {
            Form folderForm = new Form();
            folderForm.Text = "Create New Folder";
            folderForm.Size = new Size(420, 170);
            folderForm.BackColor = Color.FromArgb(16, 24, 32);
            folderForm.StartPosition = FormStartPosition.CenterParent;
            folderForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            folderForm.MaximizeBox = false;
            folderForm.MinimizeBox = false;

            Label lbl = new Label { Text = "Folder name:", ForeColor = Color.White, Font = new Font("Segoe UI", 10), Location = new Point(20, 25), Size = new Size(100, 25) };
            TextBox txtName = new TextBox { Location = new Point(20, 55), Size = new Size(365, 25), Font = new Font("Segoe UI", 10), Text = "NewFolder", BackColor = Color.FromArgb(30, 36, 44), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Button btnCreate = new Button { Text = "CREATE", Location = new Point(100, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            Button btnCancel = new Button { Text = "CANCEL", Location = new Point(220, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            btnCreate.Click += (s, ev) =>
            {
                string folderName = txtName.Text.Trim();
                if (string.IsNullOrEmpty(folderName)) { MessageBox.Show("Please enter a folder name!", "Warning"); return; }

                string newPath = Path.Combine(currentPath, folderName);
                if (Directory.Exists(newPath)) { MessageBox.Show("Folder already exists!", "Error"); return; }

                string folderToken = FolderToken(currentPath);
                // BACKEND: Assembly creates the directory.
                string result = MiniOSBackend.RunToString($"explorer mkdir {folderToken} {folderName}");

                if (result.Contains("[OK]") || Directory.Exists(newPath))
                {
                    LoadPath(currentPath);
                    folderForm.Close();
                }
                else
                {
                    // Fallback: create via C# if backend output is unavailable.
                    try { Directory.CreateDirectory(newPath); LoadPath(currentPath); folderForm.Close(); }
                    catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
                }
            };

            btnCancel.Click += (s, ev) => folderForm.Close();
            txtName.KeyPress += (s, ev) => { if (ev.KeyChar == (char)13) btnCreate.PerformClick(); };

            folderForm.Controls.Add(lbl);
            folderForm.Controls.Add(txtName);
            folderForm.Controls.Add(btnCreate);
            folderForm.Controls.Add(btnCancel);
            folderForm.ShowDialog();
        }

        // ── Create new text file via Assembly backend ──
        private void BtnNewText_Click(object sender, EventArgs e)
        {
            string fileName = $"NewFile_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string content = $"Created: {DateTime.Now}\r\n\r\nWrite your content here...";
            string folderToken = FolderToken(currentPath);

            // BACKEND: Assembly creates the file.
            string result = MiniOSBackend.RunToString($"explorer create {folderToken} {fileName} {content}");

            // Reload regardless (file should exist on disk now)
            LoadPath(currentPath);
        }

        // ── Delete file or folder via Assembly backend ──
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) { MessageBox.Show("Select an item to delete!"); return; }

            string path = fileList.SelectedItems[0].Tag.ToString();
            bool isFolder = Directory.Exists(path);
            string name = fileList.SelectedItems[0].Text.Substring(2);

            if (MessageBox.Show($"Delete '{name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (isFolder)
                {
                    // Folders: use C# recursive delete (Assembly DeleteFileA only handles files)
                    try { Directory.Delete(path, true); LoadPath(currentPath); }
                    catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
                }
                else
                {
                    string folderToken = FolderToken(Path.GetDirectoryName(path));
                    // BACKEND: Assembly deletes the file.
                    string result = MiniOSBackend.RunToString($"explorer delete {folderToken} {Path.GetFileName(path)}");

                    if (result.Contains("[OK]") || !File.Exists(path))
                        LoadPath(currentPath);
                    else
                    {
                        // Fallback
                        try { File.Delete(path); LoadPath(currentPath); }
                        catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
                    }
                }
            }
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) { MessageBox.Show("Select an item to rename!"); return; }

            string oldPath = fileList.SelectedItems[0].Tag.ToString();
            string oldName = fileList.SelectedItems[0].Text.Substring(2);

            Form renameForm = new Form { Text = "Rename", Size = new Size(400, 150), BackColor = Color.FromArgb(16, 24, 32), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };

            Label lbl = new Label { Text = "New name:", ForeColor = Color.White, Location = new Point(25, 25), Size = new Size(100, 25) };
            TextBox txt = new TextBox { Text = oldName, Location = new Point(25, 55), Size = new Size(340, 25) };
            Button btnOK = new Button { Text = "RENAME", Location = new Point(100, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button btnCancel = new Button { Text = "CANCEL", Location = new Point(220, 100), Size = new Size(100, 35), BackColor = Color.FromArgb(35, 42, 52), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnOK.Click += (s, ev) =>
            {
                string newName = txt.Text.Trim();
                if (string.IsNullOrEmpty(newName)) { MessageBox.Show("Name cannot be empty!"); return; }
                if (newName == oldName) { renameForm.Close(); return; }

                string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                if (Directory.Exists(newPath) || File.Exists(newPath)) { MessageBox.Show("Already exists!"); return; }

                    // BACKEND: no rename command exists in FileExplorer.asm, so the GUI keeps a C# move fallback.
                try
                {
                    if (Directory.Exists(oldPath)) Directory.Move(oldPath, newPath);
                    else File.Move(oldPath, newPath);
                    LoadPath(currentPath);
                    renameForm.Close();
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
            };
            btnCancel.Click += (s, ev) => renameForm.Close();

            renameForm.Controls.Add(lbl); renameForm.Controls.Add(txt);
            renameForm.Controls.Add(btnOK); renameForm.Controls.Add(btnCancel);
            renameForm.ShowDialog();
        }

        private void FileList_DoubleClick(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 0) return;
            string path = fileList.SelectedItems[0].Tag.ToString();

            if (Directory.Exists(path))
            {
                LoadPath(path);
                return;
            }

            if (!File.Exists(path)) return;

            string extension = Path.GetExtension(path).ToLower();

            // Images -> ImageViewer
            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg"
                || extension == ".gif" || extension == ".bmp")
            {
                ImageViewerForm viewer = new ImageViewerForm(Path.GetDirectoryName(path));
                viewer.Show();
                viewer.LoadImage(path);
            }
            // Text files AND files with no/unknown extension -> BuiltInNotepad
            else if (extension == ".txt" || extension == ".log" || extension == ".csv"
                     || extension == ".ini" || extension == ".cfg" || extension == "")
            {
                new BuiltInNotepad(path, true).Show();
            }
            else
            {
                // Ask: open in NOVA-OS Notepad or system default?
                var result = MessageBox.Show(
                    "Open '" + Path.GetFileName(path) + "' in NOVA-OS Notepad?\n\nClick No to use system default.",
                    "Open File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    new BuiltInNotepad(path, true).Show();
                else
                    System.Diagnostics.Process.Start(path);
            }

            SystemLogForm.WriteLog("FILE", "admin", "File Opened", Path.GetFileName(path), "SUCCESS");
        }

        private void FileList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) BtnDelete_Click(null, null);
            else if (e.KeyCode == Keys.F2) BtnRename_Click(null, null);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            string parent = Directory.GetParent(currentPath)?.FullName;
            if (parent != null) LoadPath(parent);
        }

        private void BtnRefresh_Click(object sender, EventArgs e) { LoadPath(currentPath); }

        private void StartRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 3000;
            refreshTimer.Tick += (s, e) => { if (this.Visible) LoadPath(currentPath); };
            refreshTimer.Start();
        }
    }
}