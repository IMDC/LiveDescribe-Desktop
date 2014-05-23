using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Utilities
{
    public class CopyFileProgressChangedEventArgs
    {
        public long FileSize { private set; get; }
        public long TotalBytesTransferred { private set; get; }
        public int ProgressPercentage { private set; get; }

        public CopyFileProgressChangedEventArgs(long fileSize, long totalBytesTransferred)
        {
            this.FileSize = fileSize;
            this.TotalBytesTransferred = totalBytesTransferred;
            this.ProgressPercentage = (int)(((double)TotalBytesTransferred / FileSize) * 100);
        }
    }
}
