using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCopy
{
    enum CopyStatus
    {
        InQueue,
        Ongoing,
        Paused,
        Failed,
        Cancelled,
        Finished
    }
}
