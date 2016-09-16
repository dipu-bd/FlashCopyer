using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FlashCopy.Properties;

namespace FlashCopy
{
    class Copyer
    {
        private ListView listView;

        /*
            this.driveLetter,
            this.driveName,
            this.copyStatus,
            this.sizeColumn,
            this.savePath
         */

        public Copyer(ListView viewer)
        {
            listView = viewer;
            listView.Items.Clear();
        }

        public void addDrive(string drive)
        {
            // check drive
            DriveInfo info = new DriveInfo(drive);
            if (info.DriveType != DriveType.Removable)
                throw new Exception("Non removable drive");
            // wait to be ready
            for (int i = 0; i < 300; ++i)
            {
                if (info.IsReady) break;
                Thread.Sleep(100);
            }
            // not ready after 30 seconds
            if (!info.IsReady)
                throw new Exception("Device was not ready");

            // check size
            long maxsize = 1L << (30 + Settings.Default.SizeLimit);
            long usedSpace = info.TotalSize - info.TotalFreeSpace;
            if (usedSpace > maxsize)
                throw new Exception("Size limit exceeded");

            // build destination folder
            double random = DateTime.Now.Ticks % 100000000;
            string name = info.VolumeLabel + "_" + random.ToString();
            string savePath = Settings.Default.CopyDest;
            savePath = Path.Combine(savePath, name);
            Directory.CreateDirectory(savePath);

            // add item
            ListViewItem item = new ListViewItem();
            item.Text = info.RootDirectory.FullName;
            item.SubItems.Add(info.VolumeLabel);
            item.SubItems.Add(CopyStatus.InQueue.ToString());
            item.SubItems.Add(Commons.formatSize(usedSpace));
            item.SubItems.Add(savePath);
            item.Tag = CopyStatus.InQueue;
            listView.Items.Add(item);

            // start background task
            ThreadPool.QueueUserWorkItem(new WaitCallback(copyTask), item);
        }

        public void copyTask(object data)
        {
            // item to work with
            ListViewItem item = (ListViewItem)data;
            var drive = item.SubItems[0].Text;
            var dest = item.SubItems[4].Text;

            // check status
            if ((CopyStatus)item.Tag != CopyStatus.InQueue)
            {
                setStatus(item);
                return;
            }
            item.Tag = CopyStatus.Ongoing;

            // get size
            long finish = 0;
            DriveInfo info = new DriveInfo(drive);
            long total = info.TotalSize - info.TotalFreeSpace;

            int okay = 0;
            string progress = "0.00%";
            List<string> files = FolderMD5.GetFiles(drive);
            for (int i = 0; i < files.Count; ++i)
            {
                // check settings
                if (!Settings.Default.Enabled)
                {
                    item.Tag = CopyStatus.Cancelled;
                }
                // check status
                setStatus(item);
                var status = (CopyStatus)item.Tag;
                if (status == CopyStatus.Cancelled)
                {
                    return;
                }
                if (status == CopyStatus.Paused)
                {
                    i--;
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    setStatus(item, progress);
                    // copy file
                    string file = files[i];
                    FileInfo finfo = new FileInfo(file);
                    string relativePath = file.Substring(drive.Length);
                    copyFile(file, Path.Combine(dest, relativePath));

                    // increase progress
                    ++okay;
                    finish += finfo.Length;
                    progress = String.Format("{0:0.00}%", 100.0 * finish / total);
                    setStatus(item, progress);
                }
                catch { }
            }

            // is finished ?
            if (okay == files.Count || finish == total)
                item.Tag = CopyStatus.Finished;
            else
                item.Tag = CopyStatus.Failed; 
            setStatus(item);
        }

        private void setStatus(ListViewItem item, string text = "")
        {
            if (string.IsNullOrEmpty(text))
                text = item.Tag.ToString();
            listView.Invoke((MethodInvoker)delegate
            {
                item.SubItems[2].Text = text;
            });
        }

        private void copyFile(string source, string dest)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(source, dest, false);
        }
    }
}
