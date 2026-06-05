using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class Notepad : Form
    {
        private RichTextBox textBox;
        private string filePath;
        private bool canEdit;
        private bool isModified = false;
        private Label lblStatus;
        private Timer autoSaveTimer;

        public Notepad(string path, bool editable)
        {
            filePath = path;
            canEdit = editable;

            this.Text = canEdit ? "NOVA-OS Notepad - Editor" : "NOVA-OS Notepad - Reader";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);

            BuildUI();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                LoadFile();
            else if (!string.IsNullOrEmpty(filePath))
                CreateNewFile();
            else
                NewFile();
        }

        private void BuildUI()
        {
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

            ToolStripSeparator sep = new ToolStripSeparator();

            ToolStripMenuItem closeItem = new ToolStripMenuItem("Close");
            closeItem.ShortcutKeys = Keys.Control | Keys.W;
            closeItem.Click += (s, e) => this.Close();

            fileMenu.DropDownItems.Add(newItem);
            fileMenu.DropDownItems.Add(openItem);
            fileMenu.DropDownItems.Add(saveItem);
            fileMenu.DropDownItems.Add(saveAsItem);
            fileMenu.DropDownItems.Add(sep);
            fileMenu.DropDownItems.Add(closeItem);

            // Edit Menu (only if editable)
            if (canEdit)
            {
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

                menuStrip.Items.Add(editMenu);
            }

            // Format Menu
            ToolStripMenuItem formatMenu = new ToolStripMenuItem("Format");
            ToolStripMenuItem wordWrapItem = new ToolStripMenuItem("Word Wrap");
            wordWrapItem.CheckOnClick = true;
            wordWrapItem.Checked = true;
            wordWrapItem.Click += (s, e) => textBox.WordWrap = wordWrapItem.Checked;
            formatMenu.DropDownItems.Add(wordWrapItem);

            ToolStripMenuItem fontSizeItem = new ToolStripMenuItem("Font Size");
            ToolStripMenuItem size10Item = new ToolStripMenuItem("10");
            ToolStripMenuItem size12Item = new ToolStripMenuItem("12");
            ToolStripMenuItem size14Item = new ToolStripMenuItem("14");
            ToolStripMenuItem size16Item = new ToolStripMenuItem("16");
            size10Item.Click += (s, e) => textBox.Font = new Font("Consolas", 10);
            size12Item.Click += (s, e) => textBox.Font = new Font("Consolas", 12);
            size14Item.Click += (s, e) => textBox.Font = new Font("Consolas", 14);
            size16Item.Click += (s, e) => textBox.Font = new Font("Consolas", 16);
            fontSizeItem.DropDownItems.Add(size10Item);
            fontSizeItem.DropDownItems.Add(size12Item);
            fontSizeItem.DropDownItems.Add(size14Item);
            fontSizeItem.DropDownItems.Add(size16Item);
            formatMenu.DropDownItems.Add(fontSizeItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(formatMenu);

            // Toolbar
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 35;
            toolbar.BackColor = Color.FromArgb(24, 30, 38);

            Button btnSave = new Button();
            btnSave.Text = "💾 Save";
            btnSave.Size = new Size(80, 28);
            btnSave.Location = new Point(10, 3);
            btnSave.BackColor = Color.FromArgb(35, 42, 52);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += (s, e) => SaveFile();
            btnSave.Enabled = canEdit;
            toolbar.Controls.Add(btnSave);

            // Text Editor
            textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.BackColor = Color.Black;
            textBox.ForeColor = Color.Lime;
            textBox.Font = new Font("Consolas", 11);
            textBox.WordWrap = true;

            if (!canEdit)
            {
                textBox.ReadOnly = true;
                textBox.BackColor = Color.FromArgb(20, 25, 35);
            }

            textBox.TextChanged += (s, e) => {
                if (canEdit && !isModified)
                {
                    isModified = true;
                    this.Text = (canEdit ? "✏ " : "📖 ") + (string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileName(filePath)) + " *";
                }
            };

            // Status Bar
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 25;
            lblStatus.BackColor = Color.FromArgb(24, 30, 38);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Text = "Ready";
            lblStatus.Padding = new Padding(5, 0, 0, 0);

            Controls.Add(textBox);
            Controls.Add(toolbar);
            Controls.Add(menuStrip);
            Controls.Add(lblStatus);

            // Auto-save timer
            if (canEdit)
            {
                autoSaveTimer = new Timer();
                autoSaveTimer.Interval = 30000;
                autoSaveTimer.Tick += (s, e) => { if (isModified && !string.IsNullOrEmpty(filePath)) AutoSave(); };
                autoSaveTimer.Start();
            }

            UpdateStatus();
        }

        private void LoadFile()
        {
            try
            {
                textBox.Text = File.ReadAllText(filePath);
                isModified = false;
                this.Text = (canEdit ? "✏ " : "📖 ") + Path.GetFileName(filePath);
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot load file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateNewFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, "");
                isModified = false;
                this.Text = (canEdit ? "✏ " : "📖 ") + Path.GetFileName(filePath);
            }
            catch { }
        }

        private void NewFile()
        {
            if (isModified && canEdit)
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
            this.Text = canEdit ? "✏ NOVA-OS Notepad - New Document" : "📖 NOVA-OS Notepad - Reader";
            UpdateStatus();
        }

        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "MiniOS_Documents");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
                LoadFile();
            }
        }

        private void SaveFile()
        {
            if (!canEdit) return;

            if (string.IsNullOrEmpty(filePath))
            {
                SaveFileAs();
                return;
            }

            try
            {
                File.WriteAllText(filePath, textBox.Text);
                isModified = false;
                this.Text = "✏ " + Path.GetFileName(filePath);
                UpdateStatus();
                SystemLogForm.WriteLog("FILE", "admin", "File Saved", Path.GetFileName(filePath), "SUCCESS");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot save: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFileAs()
        {
            if (!canEdit) return;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "MiniOS_Documents");
            dialog.FileName = "Untitled.txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
                SaveFile();
            }
        }

        private void AutoSave()
        {
            if (!string.IsNullOrEmpty(filePath) && canEdit)
            {
                try
                {
                    File.WriteAllText(filePath, textBox.Text);
                    isModified = false;
                    UpdateStatus("Auto-saved");
                }
                catch { }
            }
        }

        private void UpdateStatus(string extra = "")
        {
            int lines = textBox.Lines.Length;
            int chars = textBox.TextLength;
            string mode = canEdit ? "EDIT MODE" : "READ ONLY";

            lblStatus.Text = $"{mode} | Lines: {lines} | Characters: {chars} | {(string.IsNullOrEmpty(filePath) ? "Unsaved" : Path.GetFileName(filePath))}";
            if (!string.IsNullOrEmpty(extra))
                lblStatus.Text += $" | {extra}";
        }

        private void UpdateStatus()
        {
            UpdateStatus("");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isModified && canEdit && !string.IsNullOrEmpty(filePath))
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
    }
}