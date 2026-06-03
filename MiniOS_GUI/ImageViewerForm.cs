using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class ImageViewerForm : Form
    {
        private PictureBox pictureBox;
        private ListView imageList;
        private string currentFolder;
        private string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private Button btnUpload, btnDelete, btnRefresh, btnZoomIn, btnZoomOut, btnReset;
        private Label lblStatus, lblImageInfo;
        private Panel picturePanel;
        private float currentZoom = 1.0f;
        private string currentImagePath = null;

        public ImageViewerForm(string folderPath)
        {
            currentFolder = folderPath;
            InitializeComponent();
            LoadImages();
            this.WindowState = FormWindowState.Maximized;
            SystemLogForm.WriteLog("IMAGE", "admin", "Image Viewer Opened", currentFolder, "SUCCESS");
        }

        private void InitializeComponent()
        {
            this.Text = "MiniOS Image Viewer";
            this.BackColor = Color.FromArgb(16, 24, 32);
            this.StartPosition = FormStartPosition.CenterScreen;

            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Vertical;
            splitContainer.SplitterDistance = 280;

            // ========== LEFT PANEL ==========
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.BackColor = Color.FromArgb(20, 26, 34);
            leftPanel.Padding = new Padding(5);

            Label titleLabel = new Label();
            titleLabel.Text = "📸 IMAGE GALLERY";
            titleLabel.ForeColor = Color.DeepSkyBlue;
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.Location = new Point(10, 10);
            titleLabel.AutoSize = true;

            imageList = new ListView();
            imageList.Location = new Point(10, 50);
            imageList.Size = new Size(260, 500);
            imageList.View = View.LargeIcon;
            imageList.LargeImageList = new ImageList();
            imageList.LargeImageList.ImageSize = new Size(80, 80);
            imageList.DoubleClick += ImageList_DoubleClick;
            imageList.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) BtnDelete_Click(null, null); };

            btnUpload = CreateButton("📤 UPLOAD", 10, 560, 260, 40);
            btnUpload.Click += BtnUpload_Click;

            btnDelete = CreateButton("🗑 DELETE", 10, 610, 125, 40);
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = CreateButton("🔄 REFRESH", 145, 610, 125, 40);
            btnRefresh.Click += (s, e) => LoadImages();

            leftPanel.Controls.Add(titleLabel);
            leftPanel.Controls.Add(imageList);
            leftPanel.Controls.Add(btnUpload);
            leftPanel.Controls.Add(btnDelete);
            leftPanel.Controls.Add(btnRefresh);

            // ========== RIGHT PANEL ==========
            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(16, 24, 32);
            rightPanel.Padding = new Padding(10);

            // Top Toolbar
            Panel toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 50;
            toolbar.BackColor = Color.FromArgb(24, 30, 38);

            btnZoomIn = CreateToolButton("🔍 ZOOM IN", 10);
            btnZoomIn.Click += BtnZoomIn_Click;

            btnZoomOut = CreateToolButton("🔍 ZOOM OUT", 120);
            btnZoomOut.Click += BtnZoomOut_Click;

            btnReset = CreateToolButton("📐 RESET", 230);
            btnReset.Click += BtnReset_Click;

            lblImageInfo = new Label();
            lblImageInfo.ForeColor = Color.Cyan;
            lblImageInfo.Font = new Font("Segoe UI", 9);
            lblImageInfo.Location = new Point(340, 15);
            lblImageInfo.AutoSize = true;
            lblImageInfo.Text = "No image selected";

            toolbar.Controls.Add(btnZoomIn);
            toolbar.Controls.Add(btnZoomOut);
            toolbar.Controls.Add(btnReset);
            toolbar.Controls.Add(lblImageInfo);

            // Picture Panel with AutoScroll
            picturePanel = new Panel();
            picturePanel.Dock = DockStyle.Fill;
            picturePanel.AutoScroll = true;
            picturePanel.BackColor = Color.Black;
            picturePanel.AutoScrollMinSize = new Size(100, 100);

            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.BackColor = Color.Black;
            pictureBox.Location = new Point(0, 0);

            picturePanel.Controls.Add(pictureBox);

            // Center image when panel resizes
            picturePanel.Resize += (s, e) => CenterImage();

            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Text = "Double-click any image to view | Use mouse wheel to zoom | Drag to pan";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.BackColor = Color.FromArgb(24, 30, 38);

            rightPanel.Controls.Add(toolbar);
            rightPanel.Controls.Add(picturePanel);
            rightPanel.Controls.Add(lblStatus);

            splitContainer.Panel1.Controls.Add(leftPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);
            Controls.Add(splitContainer);

            // Mouse wheel zoom
            pictureBox.MouseWheel += PictureBox_MouseWheel;
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(width, height);
            btn.Location = new Point(x, y);
            btn.BackColor = Color.FromArgb(35, 42, 52);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            return btn;
        }

        private Button CreateToolButton(string text, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(100, 35);
            btn.Location = new Point(x, 8);
            btn.BackColor = Color.FromArgb(35, 42, 52);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return btn;
        }

        private void CenterImage()
        {
            if (pictureBox.Image == null) return;

            // Calculate center position
            int x = (picturePanel.Width - pictureBox.Width) / 2;
            int y = (picturePanel.Height - pictureBox.Height) / 2;

            // Make sure it doesn't go negative
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            pictureBox.Location = new Point(x, y);
        }

        private void UpdateZoom()
        {
            if (string.IsNullOrEmpty(currentImagePath) || !File.Exists(currentImagePath)) return;

            try
            {
                using (var tempImage = Image.FromFile(currentImagePath))
                {
                    int newWidth = (int)(tempImage.Width * currentZoom);
                    int newHeight = (int)(tempImage.Height * currentZoom);

                    pictureBox.Size = new Size(newWidth, newHeight);
                    pictureBox.Image = new Bitmap(tempImage, newWidth, newHeight);
                }

                CenterImage();
                lblImageInfo.Text = $"Zoom: {currentZoom * 100:F0}% | {pictureBox.Width} x {pictureBox.Height}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (pictureBox.Image == null) return;

            float oldZoom = currentZoom;

            if (e.Delta > 0)
                currentZoom *= 1.1f;
            else
                currentZoom /= 1.1f;

            currentZoom = Math.Max(0.1f, Math.Min(5.0f, currentZoom));

            if (Math.Abs(oldZoom - currentZoom) > 0.01f)
                UpdateZoom();
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null) return;
            currentZoom *= 1.2f;
            currentZoom = Math.Min(5.0f, currentZoom);
            UpdateZoom();
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null) return;
            currentZoom /= 1.2f;
            currentZoom = Math.Max(0.1f, currentZoom);
            UpdateZoom();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null) return;
            currentZoom = 1.0f;
            UpdateZoom();
        }

        private void LoadImages()
        {
            imageList.Items.Clear();
            imageList.LargeImageList.Images.Clear();

            if (!Directory.Exists(currentFolder))
                Directory.CreateDirectory(currentFolder);

            int index = 0;
            foreach (string file in Directory.GetFiles(currentFolder))
            {
                string ext = Path.GetExtension(file).ToLower();
                if (Array.Exists(imageExtensions, e => e == ext))
                {
                    try
                    {
                        using (var img = Image.FromFile(file))
                        {
                            Image thumbnail = img.GetThumbnailImage(80, 80, null, IntPtr.Zero);
                            imageList.LargeImageList.Images.Add(thumbnail);
                        }
                        ListViewItem item = new ListViewItem(Path.GetFileName(file));
                        item.ImageIndex = index;
                        item.Tag = file;
                        imageList.Items.Add(item);
                        index++;
                    }
                    catch { }
                }
            }
            lblStatus.Text = $"{imageList.Items.Count} images found | Double-click to view";
        }

        private void ImageList_DoubleClick(object sender, EventArgs e)
        {
            if (imageList.SelectedItems.Count == 0) return;

            currentImagePath = imageList.SelectedItems[0].Tag.ToString();
            currentZoom = 1.0f;

            try
            {
                if (pictureBox.Image != null)
                    pictureBox.Image.Dispose();

                var img = Image.FromFile(currentImagePath);
                pictureBox.Image = img;
                pictureBox.Size = img.Size;

                CenterImage();

                FileInfo info = new FileInfo(currentImagePath);
                lblImageInfo.Text = $"📷 {Path.GetFileName(currentImagePath)} | {info.Length / 1024} KB | {img.Width} x {img.Height} | Zoom: 100%";
                lblStatus.Text = $"Viewing: {Path.GetFileName(currentImagePath)} | Use mouse wheel to zoom";

                SystemLogForm.WriteLog("IMAGE", "admin", "Image Viewed", Path.GetFileName(currentImagePath), "SUCCESS");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot load image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Image to Upload";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int uploaded = 0;
                foreach (string file in dialog.FileNames)
                {
                    try
                    {
                        string destPath = Path.Combine(currentFolder, Path.GetFileName(file));
                        if (File.Exists(destPath))
                        {
                            string name = Path.GetFileNameWithoutExtension(file);
                            string ext = Path.GetExtension(file);
                            int counter = 1;
                            while (File.Exists(Path.Combine(currentFolder, $"{name}_{counter}{ext}"))) counter++;
                            destPath = Path.Combine(currentFolder, $"{name}_{counter}{ext}");
                        }
                        File.Copy(file, destPath);
                        uploaded++;
                    }
                    catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error"); }
                }
                LoadImages();
                MessageBox.Show($"Successfully uploaded {uploaded} image(s)!", "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SystemLogForm.WriteLog("IMAGE", "admin", "Images Uploaded", $"{uploaded} files", "SUCCESS");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (imageList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select an image to delete!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string imagePath = imageList.SelectedItems[0].Tag.ToString();
            if (MessageBox.Show($"Delete '{Path.GetFileName(imagePath)}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    File.Delete(imagePath);
                    if (currentImagePath == imagePath)
                    {
                        pictureBox.Image?.Dispose();
                        pictureBox.Image = null;
                        currentImagePath = null;
                        lblImageInfo.Text = "No image selected";
                    }
                    LoadImages();
                    lblStatus.Text = $"Deleted: {Path.GetFileName(imagePath)}";
                    SystemLogForm.WriteLog("IMAGE", "admin", "Image Deleted", Path.GetFileName(imagePath), "SUCCESS");
                }
                catch (Exception ex) { MessageBox.Show($"Cannot delete: {ex.Message}", "Error"); }
            }
        }
    }
}