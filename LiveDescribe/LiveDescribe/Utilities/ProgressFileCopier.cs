using System;
using System.Runtime.InteropServices;

namespace LiveDescribe.Utilities
{
    /// <summary>
    /// A wrapper for the Wndows API function CopyFileEx.
    /// </summary>
    public class ProgressFileCopier
    {
        #region CopyFileEx Definitions
        /// <summary>
        /// Copies a file while using a callback function to notify progress.
        /// </summary>
        /// <param name="lpExistingFileName">The file to copy.</param>
        /// <param name="lpNewFileName">The path to copy the file to.</param>
        /// <param name="lpProgressRoutine">A callback function to notify progress changes to.</param>
        /// <param name="lpData">
        /// Optional argument to be passed to the callback function. Can be set to NULL.
        /// </param>
        /// <param name="pbCancel">If set to true during copying, then the operation will be cancelled.</param>
        /// <param name="dwCopyFlags">Flags that determine how the file should be copied.</param>
        /// <returns>True on success and false on failure.</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CopyFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine,
            IntPtr lpData,
            ref Int32 pbCancel,
            CopyFileFlags dwCopyFlags);

        /// <summary>
        /// A delegate that holds a reference to the CopyFileEx callback function to use.
        /// </summary>
        /// <param name="totalFileSize">Total size of the file.</param>
        /// <param name="totalBytesTransferred">
        /// The total amount of bytes copied since the start of the operation.
        /// </param>
        /// <param name="streamSize">The total size of the current file stream in bytes.</param>
        /// <param name="streamBytesTransferred">
        /// Number of bytes in the stream that have been transferred since the beginning of the operation.
        /// </param>
        /// <param name="dwStreamNumber">A handle to the current stream.</param>
        /// <param name="dwCallbackReason">The reason for invoking the callback method.</param>
        /// <param name="hSourceFile">A handle to the source file.</param>
        /// <param name="hDestinationFile">A handle to the destination file.</param>
        /// <param name="lpData">An optional argument passed into CopyFileEx. Can be NULL.</param>
        /// <returns>A ProgressResult value that tells CopyFileEx what to do next.</returns>
        delegate ProgressResult CopyProgressRoutine(
        long totalFileSize,
        long totalBytesTransferred,
        long streamSize,
        long streamBytesTransferred,
        uint dwStreamNumber,
        CallbackReason dwCallbackReason,
        IntPtr hSourceFile,
        IntPtr hDestinationFile,
        IntPtr lpData);

        /// <summary>
        /// Flags that specify how the file is to be copied.
        /// </summary>
        [Flags]
        private enum CopyFileFlags : uint
        {
            /// <summary>
            /// Copy the file even if the destination file can not be encrypted.
            /// </summary>
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
            /// <summary>
            /// Make the destination file a symbolic link if the source file is also a symbolic link.
            /// </summary>
            COPY_FILE_COPY_SYMLINK = 0x00000800,
            /// <summary>
            /// Force the copy operation to fail if the destination file already exists.
            /// </summary>
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            /// <summary>
            /// Use unbuffered I/O, bypassing cache resources. Reccomended for large file transfers.
            /// </summary>
            COPY_FILE_NO_BUFFERING = 0x00001000,
            /// <summary>
            /// The file is copied and the original file is opened for write access.
            /// </summary>
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            /// <summary>
            /// Progress of the copy is tracked in the target file in case the copy fails. The
            /// failed copy can be restarted at a later time by specifying the same values for
            /// lpExistingFileName and lpNewFileName as those used in the call that failed. This can
            /// significantly slow down the copy operation as the new file may be flushed multiple
            /// times during the copy operation.
            /// </summary>
            COPY_FILE_RESTARTABLE = 0x00000002,
        }

        private enum CallbackReason : uint
        {
            /// <summary>
            /// Another chunk of the data file was copied to the destination.
            /// </summary>
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            /// <summary>
            /// A new stream was created for copying.
            /// </summary>
            CALLBACK_STREAM_SWITCH = 0x00000001,
        }

        private enum ProgressResult : uint
        {
            /// <summary>
            /// Cancel the operation and delete the destination file
            /// </summary>
            CANCEL = 1,
            /// <summary>
            /// Continue the operation
            /// </summary>
            CONTINUE = 0,
            /// <summary>
            /// Continue the operation, but stop invoking the callback method for updating progress.
            /// </summary>
            QUIET = 3,
            /// <summary>
            /// Stop the operation. It can be restarted again.
            /// </summary>
            STOP = 2,
        }
        #endregion

        /// <summary>
        /// Event that is invoked when the copying file progress is changed.
        /// </summary>
        public event EventHandler<CopyFileProgressChangedEventArgs> ProgressChanged;

        public ProgressFileCopier()
        { }

        //Refer to the CopyProgressRoutine delegate for paramater explanations.
        private ProgressResult CopyProgressCallback(
            long totalFileSize,
            long totalBytesTransferred,
            long streamSize,
            long streamBytesTransferred,
            uint dwStreamNumber,
            CallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData)
        {
            OnProgressChanged(totalFileSize, totalBytesTransferred);
            return ProgressResult.CONTINUE;
        }

        public void CopyFile(string source, string destination)
        {
            int pbCancel = 0;
            CopyFileEx(source, destination, CopyProgressCallback, IntPtr.Zero, ref pbCancel, 0);
        }

        private void OnProgressChanged(long fileSize, long totalBytesTransferred)
        {
            EventHandler<CopyFileProgressChangedEventArgs> handler = ProgressChanged;
            if (handler != null)
            {
                handler(this, new CopyFileProgressChangedEventArgs(fileSize, totalBytesTransferred));
            }
        }
    }
}
