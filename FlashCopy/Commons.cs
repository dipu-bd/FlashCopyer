using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCopy
{
    class Commons
    {
        private static string[] letter = { "B", "KB", "MB", "GB", "TB" };

        public static String formatSize(long size)
        {
            int index = 0;
            double res = size;
            while (res > 1024)
            {
                res /= 1024;
                index++;
            }
            return String.Format("{0:0.00}{1}", res, letter[index]);
        }
    }
}
