﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    public static partial class WimgApi
    {
        /// <summary>
        /// Copies an existing file to a new file.
        /// </summary>
        /// <param name="sourceFile">The name of an existing .wim file.</param>
        /// <param name="destinationFile">The name of the new file.</param>
        /// <param name="options">Specifies how the file is to be copied.</param>
        /// <exception cref="ArgumentNullException">sourceFile or destinationFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void CopyFile(string sourceFile, string destinationFile, WimCopyFileOptions options)
        {
            // Call an override
            //
            WimgApi.CopyFile(sourceFile, destinationFile, options, null, null);
        }

        /// <summary>
        /// Copies an existing file to a new file. Notifies the application of its progress through a callback function. If the source file has verification data, the contents of the file are verified during the copy operation.
        /// </summary>
        /// <param name="sourceFile">The name of an existing .wim file.</param>
        /// <param name="destinationFile">The name of the new file.</param>
        /// <param name="options">Specifies how the file is to be copied.</param>
        /// <param name="copyFileProgressCallback">A <see cref="CopyFileProgressCallback"/> method to call when progress is made copying the file and allowing the user to cancel the operation.</param>
        /// <param name="userData">An object containing data to be used by the progress callback method.</param>
        /// <exception cref="ArgumentNullException">sourceFile or destinationFile is null.</exception>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static void CopyFile(string sourceFile, string destinationFile, WimCopyFileOptions options, CopyFileProgressCallback copyFileProgressCallback, object userData)
        {
            // See if sourceFile is null
            //
            if (sourceFile == null)
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            // See if destinationFile is null
            //
            if (destinationFile == null)
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            // Create a CopyFileProgress object
            //
            CopyFileProgress fileInfoCopyProgress = new CopyFileProgress(sourceFile, destinationFile, copyFileProgressCallback, userData);

            // Cancel flag is always false
            //
            bool cancel = false;

            // Call the native function
            //
            if (!WimgApi.NativeMethods.WIMCopyFile(sourceFile, destinationFile, fileInfoCopyProgress.CopyProgressHandler, IntPtr.Zero, ref cancel, (DWORD)options))
            {
                // Throw a Win32Exception based on the last error code
                //
                throw new Win32Exception();
            }
        }

        private static partial class NativeMethods
        {
            /// <summary>
            /// Copies an existing file to a new file. Notifies the application of its progress through a callback function. If the
            /// source file has verification data, the contents of the file are verified during the copy operation.
            /// </summary>
            /// <param name="pszExistingFileName">
            /// A pointer to a null-terminated string that specifies the name of an existing .wim
            /// file.
            /// </param>
            /// <param name="pszNewFileName">A pointer to a null-terminated string that specifies the name of the new file.</param>
            /// <param name="pProgressRoutine">
            /// The address of a callback function of type LPPROGRESS_ROUTINE that is called each time
            /// another portion of the file has been copied. This parameter can be NULL.
            /// </param>
            /// <param name="pvData">An argument to be passed to the callback function. This parameter can be NULL.</param>
            /// <param name="pbCancel">
            /// If this flag is set to TRUE during the copy operation, the operation is canceled. Otherwise, the
            /// copy operation continues to completion.
            /// </param>
            /// <param name="dwCopyFlags">A flag that specifies how the file is to be copied.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To obtain extended error information, call the GetLastError function.
            /// If pProgressRoutine returns PROGRESS_CANCEL because the user cancels the operation, the WIMCopyFile function will
            /// return zero and set the LastError to ERROR_REQUEST_ABORTED. In this case, the partially copied destination file is
            /// deleted.
            /// If pProgressRoutine returns PROGRESS_STOP because the user stops the operation, WIMCopyFile will return zero and set
            /// the LastError to ERROR_REQUEST_ABORTED. In this case, the partially copied destination file is left intact.
            /// If the source file contains verification information, an integrity check is performed on each block as it is copied. If
            /// the integrity check fails, the WIMCopyFile function will return zero and set the LastError to ERROR_FILE_CORRUPT.
            /// </returns>
            /// <remarks>
            /// This function does not preserve extended attributes, security attributes, OLE-structured storage, NTFS file system
            /// alternate data streams, or file attributes.
            /// The WIMCopyFile function copies only the default stream of the source file, so the StreamSize and
            /// StreamBytesTransferred parameters to the CopyProgressRoutine function will always match TotalFileSize and
            /// TotalBytesTransferred, respectively. The value of the dwStreamNumber parameter will always be 1 and the value of the
            /// dwCallBackReason parameter will always be CALLBACK_CHUNK_FINISHED.
            /// If the destination file already exists and has the FILE_ATTRIBUTE_HIDDEN or FILE_ATTRIBUTE_READONLY attribute set, this
            /// function fails with ERROR_ACCESS_DENIED.
            /// </remarks>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMCopyFile(string pszExistingFileName, string pszNewFileName, CopyProgressRoutine pProgressRoutine, IntPtr pvData, [MarshalAs(UnmanagedType.Bool)] ref bool pbCancel, DWORD dwCopyFlags);
        }
    }
}