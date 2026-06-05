using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public class ImageViewerForm : Form
    {
        private TableLayoutPanel mainLayout;
        private ListView imageList;
        private Panel picturePanel;
        private PictureBox pictureBox;
        private Label lblImageCount, lblStatus;
        private Button btnUpload, btnDelete, btnRefresh;

        private string currentFolder;
        private string currentImage = null;
        private string[] exts = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public ImageViewerForm(string folderPath)
        {
            currentFolder = folderPath;

            // Default window size - PEHLE SET KARO
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 550);
            this.BackColor = Color.FromArgb(16, 24, 32);

            InitializeComponent();
            LoadImages();
        }

        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.BackColor = Color.FromArgb(16, 24, 32);

            // LEFT PANEL
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.BackColor = Color.FromArgb(20, 26, 34);
            leftPanel.Padding = new Padding(8);

            Label lblTitle = new Label();
            lblTitle.Text = "📸 GALLERY";
            lblTitle.ForeColor = Color.DeepSkyBlue;
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.Location = new Point(8, 8);
            lblTitle.AutoSize = true;

            lblImageCount = new Label();
            lblImageCount.Text = "0 images";
            lblImageCount.ForeColor = Color.Gray;
            lblImageCount.Font = new Font("Segoe UI", 9);
            lblImageCount.Location = new Point(8, 34);
            lblImageCount.AutoSize = true;

            imageList = new ListView();
            imageList.Location = new Point(8, 60);
            imageList.Size = new Size(240, 500);
            imageList.View = View.LargeIcon;
            imageList.LargeImageList = new ImageList();
            imageList.LargeImageList.ImageSize = new Size(100, 100);
            imageList.BackColor = Color.FromArgb(27, 31, 39);
            imageList.ForeColor = Color.White;
            imageList.DoubleClick += (s, e) => OpenSelectedImage();

            btnUpload = new Button();
            btnUpload.Text = "📤 UPLOAD";
            btnUpload.Location = new Point(8, 570);
            btnUpload.Size = new Size(240, 35);
            btnUpload.BackColor = Color.FromArgb(35, 42, 52);
            btnUpload.ForeColor = Color.White;
            btnUpload.FlatStyle = FlatStyle.Flat;
            btnUpload.Click += BtnUpload_Click;

            btnDelete = new Button();
            btnDelete.Text = "🗑 DELETE";
            btnDelete.Location = new Point(8, 615);
            btnDelete.Size = new Size(115, 35);
            btnDelete.BackColor = Color.FromArgb(100, 30, 30);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button();
            btnRefresh.Text = "🔄 REFRESH";
            btnRefresh.Location = new Point(133, 615);
            btnRefresh.Size = new Size(115, 35);
            btnRefresh.BackColor = Color.FromArgb(35, 42, 52);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += (s, e) => LoadImages();

            leftPanel.Controls.Add(lblTitle);
            leftPanel.Controls.Add(lblImageCount);
            leftPanel.Controls.Add(imageList);
            leftPanel.Controls.Add(btnUpload);
            leftPanel.Controls.Add(btnDelete);
            leftPanel.Controls.Add(btnRefresh);

            // RIGHT PANEL
            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(16, 24, 32);
            rightPanel.Padding = new Padding(5);

            picturePanel = new Panel();
            picturePanel.Dock = DockStyle.Fill;
            picturePanel.AutoScroll = true;
            picturePanel.BackColor = Color.Black;

            pictureBox = new PictureBox();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.BackColor = Color.Black;

            picturePanel.Controls.Add(pictureBox);

            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Text = "Ready - Double-click an image to view";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.BackColor = Color.FromArgb(24, 30, 38);

            rightPanel.Controls.Add(picturePanel);
            rightPanel.Controls.Add(lblStatus);

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);
            Controls.Add(mainLayout);
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
                if (Array.Exists(exts, e => e == ext))
                {
                    try
                    {
                        using (var img = Image.FromFile(file))
                        {
                            Image thumb = img.GetThumbnailImage(100, 100, null, IntPtr.Zero);
                            imageList.LargeImageList.Images.Add(thumb);
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

            lblImageCount.Text = $"{imageList.Items.Count} images";
            lblStatus.Text = $"{imageList.Items.Count} images in gallery | Double-click to view";
        }

        public void LoadImage(string imagePath)
        {
            try
            {
                // First, clear current image to release file lock
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                // Load new image
                pictureBox.Image = Image.FromFile(imagePath);
                currentImage = imagePath;
                lblStatus.Text = $"Viewing: {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error");
            }
        }

        private void OpenSelectedImage()
        {
            if (imageList.SelectedItems.Count == 0) return;
            string path = imageList.SelectedItems[0].Tag.ToString();
            LoadImage(path);
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Images";
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int count = 0;
                foreach (string file in dlg.FileNames)
                {
                    try
                    {
                        string dest = Path.Combine(currentFolder, Path.GetFileName(file));
                        if (File.Exists(dest))
                        {
                            string name = Path.GetFileNameWithoutExtension(file);
                            string ext = Path.GetExtension(file);
                            int c = 1;
                            while (File.Exists(Path.Combine(currentFolder, $"{name}_{c}{ext}"))) c++;
                            dest = Path.Combine(currentFolder, $"{name}_{c}{ext}");
                        }
                        File.Copy(file, dest);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error uploading: {ex.Message}", "Error");
                    }
                }
                LoadImages();
                MessageBox.Show($"Uploaded {count} image(s)!");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (imageList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select an image to delete!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string path = imageList.SelectedItems[0].Tag.ToString();
            string fileName = Path.GetFileName(path);

            // Confirm deletion
            DialogResult result = MessageBox.Show($"Delete '{fileName}'?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                // IMPORTANT: If this image is currently being viewed, release it first
                if (currentImage == path)
                {
                    if (pictureBox.Image != null)
                    {
                        pictureBox.Image.Dispose();
                        pictureBox.Image = null;
                    }
                    currentImage = null;
                    lblStatus.Text = "Ready - Double-click an image to view";
                }

                // Now delete the file
                File.Delete(path);

                // Refresh the image list
                LoadImages();

                // Log the deletion
                SystemLogForm.WriteLog("IMAGE", "admin", "Image Deleted", fileName, "SUCCESS");

                lblStatus.Text = $"Deleted: {fileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot delete '{fileName}':\n{ex.Message}", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SystemLogForm.WriteLog("ERROR", "admin", "Delete Failed", fileName, "FAILED");
            }
        }
    }
}