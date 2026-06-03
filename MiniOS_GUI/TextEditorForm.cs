using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class TextEditorForm : Form
    {
        private RichTextBox textBox;
        private string filePath;
        private bool isModified = false;
        private Label lblStatus;
        private Timer autoSaveTimer;

        public TextEditorForm(string path = null)
        {
            filePath = path;
            BuildUI();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                LoadFile();
                this.Text = $"📄 Text Editor - {Path.GetFileName(filePath)}";
            }
            else
            {
                this.Text = "📄 Text Editor - New Document";
                isModified = true;
            }

            StartAutoSave();
        }

        private void BuildUI()
        {
            this.Text = "Text Editor";
            this.Size = new Size(900, 650);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Menu Strip
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(24, 30, 38);
            menuStrip.ForeColor = Color.White;

            // File Menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");

            ToolStripMenuItem newItem = new ToolStripMenuItem("New");
            newItem.ShortcutKeys = Keys.Control | Keys.N;
            newItem.Click += (s, e) => NewFile();

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open");
            openItem.ShortcutKeys = Keys.Control | Keys.O;
            openItem.Click += (s, e) => OpenFile();

            ToolStripMenuItem saveItem = new ToolStripMenuItem("Save");
            saveItem.ShortcutKeys = Keys.Control | Keys.S;
            saveItem.Click += (s, e) => SaveFile();

            ToolStripMenuItem saveAsItem = new ToolStripMenuItem("Save As");
            saveAsItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsItem.Click += (s, e) => SaveFileAs();

            ToolStripSeparator sep1 = new ToolStripSeparator();

            ToolStripMenuItem closeItem = new ToolStripMenuItem("Close");
            closeItem.ShortcutKeys = Keys.Control | Keys.W;
            closeItem.Click += (s, e) => this.Close();

            fileMenu.DropDownItems.Add(newItem);
            fileMenu.DropDownItems.Add(openItem);
            fileMenu.DropDownItems.Add(saveItem);
            fileMenu.DropDownItems.Add(saveAsItem);
            fileMenu.DropDownItems.Add(sep1);
            fileMenu.DropDownItems.Add(closeItem);

            // Edit Menu
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");

            ToolStripMenuItem cutItem = new ToolStripMenuItem("Cut");
            cutItem.ShortcutKeys = Keys.Control | Keys.X;
            cutItem.Click += (s, e) => textBox.Cut();

            ToolStripMenuItem copyItem = new ToolStripMenuItem("Copy");
            copyItem.ShortcutKeys = Keys.Control | Keys.C;
            copyItem.Click += (s, e) => textBox.Copy();

            ToolStripMenuItem pasteItem = new ToolStripMenuItem("Paste");
            pasteItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteItem.Click += (s, e) => textBox.Paste();

            ToolStripSeparator sep2 = new ToolStripSeparator();

            ToolStripMenuItem selectAllItem = new ToolStripMenuItem("Select All");
            selectAllItem.ShortcutKeys = Keys.Control | Keys.A;
            selectAllItem.Click += (s, e) => textBox.SelectAll();

            editMenu.DropDownItems.Add(cutItem);
            editMenu.DropDownItems.Add(copyItem);
            editMenu.DropDownItems.Add(pasteItem);
            editMenu.DropDownItems.Add(sep2);
            editMenu.DropDownItems.Add(selectAllItem);

            // Format Menu
            ToolStripMenuItem formatMenu = new ToolStripMenuItem("Format");

            ToolStripMenuItem wordWrapItem = new ToolStripMenuItem("Word Wrap");
            wordWrapItem.CheckOnClick = true;
            wordWrapItem.Checked = true;
            wordWrapItem.Click += (s, e) => textBox.WordWrap = wordWrapItem.Checked;

            formatMenu.DropDownItems.Add(wordWrapItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(formatMenu);

            // Toolbar
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 40;
            toolbar.BackColor = Color.FromArgb(24, 30, 38);

            Button btnNew = CreateToolButton("📄 New", 5);
            btnNew.Click += (s, e) => NewFile();

            Button btnOpen = CreateToolButton("📂 Open", 80);
            btnOpen.Click += (s, e) => OpenFile();

            Button btnSave = CreateToolButton("💾 Save", 155);
            btnSave.Click += (s, e) => SaveFile();

            Button btnSaveAs = CreateToolButton("📁 Save As", 230);
            btnSaveAs.Click += (s, e) => SaveFileAs();

            toolbar.Controls.Add(btnNew);
            toolbar.Controls.Add(btnOpen);
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnSaveAs);

            // Text Editor
            textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.BackColor = Color.Black;
            textBox.ForeColor = Color.Lime;
            textBox.Font = new Font("Consolas", 11);
            textBox.TextChanged += (s, e) => {
                if (!isModified)
                {
                    isModified = true;
                    UpdateTitle();
                }
            };

            // Status bar
            Panel statusBar = new Panel();
            statusBar.Dock = DockStyle.Bottom;
            statusBar.Height = 25;
            statusBar.BackColor = Color.FromArgb(24, 30, 38);

            lblStatus = new Label();
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Font = new Font("Segoe UI", 8);
            lblStatus.Location = new Point(5, 5);
            lblStatus.AutoSize = true;

            statusBar.Controls.Add(lblStatus);

            Controls.Add(textBox);
            Controls.Add(toolbar);
            Controls.Add(menuStrip);
            Controls.Add(statusBar);
        }

        private Button CreateToolButton(string text, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(65, 30);
            btn.Location = new Point(x, 5);
            btn.BackColor = Color.FromArgb(35, 42, 52);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            return btn;
        }

        private void LoadFile()
        {
            try
            {
                textBox.Text = File.ReadAllText(filePath);
                isModified = false;
                UpdateTitle();
                UpdateStatus($"Loaded: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot load file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                File.WriteAllText(filePath, textBox.Text);
                isModified = false;
                UpdateTitle();
                UpdateStatus($"Saved: {Path.GetFileName(filePath)}");
                SystemLogForm.WriteLog("FILE", "admin", "File Saved", Path.GetFileName(filePath), "SUCCESS");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot save: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFileAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            dialog.Title = "Save File As";
            dialog.InitialDirectory = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
                SaveFile();
            }
        }

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            dialog.Title = "Open Text File";
            dialog.InitialDirectory = Application.StartupPath;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
                LoadFile();
                this.Text = $"Text Editor - {Path.GetFileName(filePath)}";
            }
        }

        private void NewFile()
        {
            if (isModified)
            {
                DialogResult result = MessageBox.Show("Save changes to current document?", "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    SaveFile();
                else if (result == DialogResult.Cancel)
                    return;
            }

            filePath = null;
            textBox.Clear();
            isModified = false;
            this.Text = "Text Editor - New Document";
            UpdateStatus("New document created");
        }

        private void UpdateTitle()
        {
            string title = "Text Editor";
            if (!string.IsNullOrEmpty(filePath))
                title = $"Text Editor - {Path.GetFileName(filePath)}";
            else
                title = "Text Editor - New Document";

            if (isModified)
                title = "✏ " + title;

            this.Text = title;
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = $"{message} | Line: {textBox.GetLineFromCharIndex(textBox.SelectionStart) + 1} | Char: {textBox.TextLength}";
        }

        private void StartAutoSave()
        {
            autoSaveTimer = new Timer();
            autoSaveTimer.Interval = 30000; // Auto save every 30 seconds
            autoSaveTimer.Tick += (s, e) => {
                if (isModified && !string.IsNullOrEmpty(filePath))
                {
                    SaveFile();
                    UpdateStatus("Auto-saved");
                }
            };
            autoSaveTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isModified)
            {
                DialogResult result = MessageBox.Show("Save changes before closing?", "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    SaveFile();
                else if (result == DialogResult.Cancel)
                    e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private void UpdateStatusFromText()
        {
            UpdateStatus("");
        }
    }
}