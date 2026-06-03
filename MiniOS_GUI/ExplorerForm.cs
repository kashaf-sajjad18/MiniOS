using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MiniOS_GUI
{
    public partial class ExplorerForm : Form
    {
        ListBox lstFolders;
        ListBox lstFiles;
        RichTextBox viewer;

        public ExplorerForm()
        {
           
            BuildUI();
        }

        void BuildUI()
        {
            this.Text = "MiniOS Explorer";

            this.Size =
                new Size(1200, 700);

            this.BackColor =
                Color.FromArgb(16, 24, 32);

            lstFolders = new ListBox();

            lstFolders.Location =
                new Point(20, 20);

            lstFolders.Size =
                new Size(250, 600);

            lstFiles = new ListBox();

            lstFiles.Location =
                new Point(300, 20);

            lstFiles.Size =
                new Size(300, 600);

            viewer = new RichTextBox();

            viewer.Location =
                new Point(640, 20);

            viewer.Size =
                new Size(500, 600);

            Controls.Add(lstFolders);
            Controls.Add(lstFiles);
            Controls.Add(viewer);

            lstFolders.Items.Add("Documents");
            lstFolders.Items.Add("Projects");
            lstFolders.Items.Add("Private Folder 🔒");

            lstFolders.SelectedIndexChanged
                += FolderChanged;

            lstFiles.SelectedIndexChanged
                += FileChanged;
        }

        void FolderChanged(
            object sender,
            EventArgs e)
        {
            lstFiles.Items.Clear();

            string folder =
                lstFolders.SelectedItem.ToString();

            if (folder == "Documents")
            {
                lstFiles.Items.Add("notes.txt");
                lstFiles.Items.Add("report.txt");
            }

            if (folder == "Projects")
            {
                lstFiles.Items.Add("MiniOS.txt");
                lstFiles.Items.Add("Assembly.txt");
            }

            if (folder == "Private Folder 🔒")
            {
                string pass =
                    PromptPassword();

                if (pass == "admin123")
                {
                    lstFiles.Items.Add(
                        "confidential.txt");

                    lstFiles.Items.Add(
                        "adminlogs.txt");
                }
                else
                {
                    MessageBox.Show(
                        "Access Denied");
                }
            }
        }

        void FileChanged(
            object sender,
            EventArgs e)
        {
            if (lstFiles.SelectedItem == null)
                return;

            string file =
                lstFiles.SelectedItem.ToString();

            viewer.Text =
                "Opened File:\r\n\r\n"
                + file +
                "\r\n\r\nSample Content.";
        }

        string PromptPassword()
        {
            Form f = new Form();

            TextBox txt =
                new TextBox();

            Button ok =
                new Button();

            string result = "";

            f.Size =
                new Size(320, 170);

            txt.Location =
                new Point(20, 30);

            txt.PasswordChar = '*';

            ok.Text = "Unlock";

            ok.Location =
                new Point(20, 70);

            ok.Click += (s, e) =>
            {
                result = txt.Text;
                f.Close();
            };

            f.Controls.Add(txt);
            f.Controls.Add(ok);

            f.ShowDialog();

            return result;
        }
    }
}