using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace FlashCopy
{
    /// <summary>
    /// NOT TESTED. MAY BE ERROR PRONE. USE WITH CAUTION.
    /// </summary>
    class USBSerial
    {
        public static string GetSerialNumber(string DriveLetter)
        { 
            // get the Logical Disk for that drive letter
            DriveLetter = DriveLetter.TrimEnd("\\".ToCharArray());
            var wmi_ld = new ManagementObject("Win32_LogicalDisk.DeviceID='" + DriveLetter + "'");
            // get the associated DiskPartition 
            foreach (ManagementObject wmi_dp in wmi_ld.GetRelated("Win32_DiskPartition"))
            {
                // get the associated DiskDrive
                foreach (ManagementObject wmi_dd in wmi_dp.GetRelated("Win32_DiskDrive"))
                {
                    // There is a bug in WinVista that corrupts some of the fields
                    // of the Win32_DiskDrive class if you instantiate the class via
                    // its primary key (as in the example above) and the device is
                    // a USB disk.  Oh well... so we have go thru this extra step
                    var wmi = new ManagementClass("Win32_DiskDrive");
                    // loop thru all of the instances.  This is silly, we shouldn't
                    // have to loop thru them all, when we know which one we want.
                    foreach (ManagementObject obj in wmi.GetInstances())
                    {
                        // do the DeviceID fields match?
                        if (obj["DeviceID"].ToString() == wmi_dd["DeviceID"].ToString())
                        {
                            // the serial number is embedded in the PnPDeviceID
                            string temp = obj["PnPDeviceID"].ToString();
                            if (!temp.StartsWith("USBSTOR"))
                            {
                                throw new ApplicationException(DriveLetter + " doesn't appear to be USB Device");
                            }
                            string[] parts = temp.Split("\\&".ToCharArray());
                            // The serial number should be the next to the last element
                            return parts[parts.Length - 2];
                        }
                    }
                }
            }
            return DriveLetter;
        }
    }
}
