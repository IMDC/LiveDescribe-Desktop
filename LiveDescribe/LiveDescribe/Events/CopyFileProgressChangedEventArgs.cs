namespace LiveDescribe.Events
{
    public class CopyFileProgressChangedEventArgs
    {
        public long FileSize { private set; get; }
        public long TotalBytesTransferred { private set; get; }
        public int ProgressPercentage { private set; get; }

        public CopyFileProgressChangedEventArgs(long fileSize, long totalBytesTransferred)
        {
            FileSize = fileSize;
            TotalBytesTransferred = totalBytesTransferred;
            ProgressPercentage = (int)(((double)TotalBytesTransferred / FileSize) * 100);
        }
    }
}
