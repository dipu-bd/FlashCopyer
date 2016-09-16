using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dolinay;
using FlashCopy.Properties;

namespace FlashCopy
{
    public partial class MainForm : Form
    {
        private DriveDetector driveDetector;
        private Copyer mCopyer;

        public MainForm()
        {
            Settings.Default.Reload();

            InitializeComponent();

            copyPath.Text = Settings.Default.CopyDest;
            sizeLimit.SelectedIndex = Settings.Default.SizeLimit;

            mCopyer = new Copyer(listView1);

            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(device_Arrived);
        }

        // Hide instead of closing
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            HideMe();
        }

        public void HideMe()
        {
            this.Hide();
            notifyIcon1.ShowBalloonTip(1000);
        }

        public void ShowMe()
        {
            this.Show();
            this.TopMost = true;
            this.WindowState = FormWindowState.Normal;
            this.TopMost = false;
        }

        private void device_Arrived(object sender, DriveDetectorEventArgs e)
        {
            try
            {
                if (Settings.Default.Enabled)
                {
                    mCopyer.addDrive(e.Drive);
                }
            }
            catch { }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(copyPath.Text))
            {
                MessageBox.Show("Invalid copy path. " + copyPath.Text + " does not exists");
                copyPath.Text = Settings.Default.CopyDest;
            }
            // save settings
            Settings.Default.CopyDest = copyPath.Text;
            Settings.Default.SizeLimit = sizeLimit.SelectedIndex;
            Settings.Default.Save();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder to copy pendrive files";
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                copyPath.Text = fbd.SelectedPath;
            }
        }
         
        private void hideButton_Click(object sender, EventArgs e)
        {
            HideMe();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowMe();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var item = (ListViewItem)listView1.FocusedItem;
            if (item == null) e.Cancel = true;
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ListViewItem)listView1.FocusedItem;
            System.Diagnostics.Process.Start(item.SubItems[4].Text);
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ListViewItem)listView1.FocusedItem;
            var status = (CopyStatus)item.Tag;
            var ok = (status == CopyStatus.InQueue
                || status == CopyStatus.Ongoing);
            if (ok) item.Tag = CopyStatus.Paused;

        }

        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ListViewItem)listView1.FocusedItem;
            var status = (CopyStatus)item.Tag;
            var ok = (status == CopyStatus.Paused);
            if (ok) item.Tag = CopyStatus.Ongoing;
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var item = (ListViewItem)listView1.FocusedItem;
            var status = (CopyStatus)item.Tag;
            var ok = (status == CopyStatus.Paused
                || status == CopyStatus.InQueue
                || status == CopyStatus.Ongoing);
            if (ok) item.Tag = CopyStatus.Cancelled;
        }

    }
}
