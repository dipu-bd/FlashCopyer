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

namespace FlashCopy
{
    public partial class MainForm : Form
    {
        private DriveDetector driveDetector;

        public MainForm()
        {
            InitializeComponent();

            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(device_Arrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(device_Removed);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }

        private void device_Arrived(object sender, DriveDetectorEventArgs e)
        {
            var drive = new DriveInfo(e.Drive);

        }
        private void device_Removed(object sender, DriveDetectorEventArgs e)
        {

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

        private void copyEnabled_CheckedChanged(object sender, EventArgs e)
        {
            copyEnabled.Text = copyEnabled.Checked ? "Disable" : "Enable";
        }

        private void hideButton_Click(object sender, EventArgs e)
        {

        }
         
    }
}
