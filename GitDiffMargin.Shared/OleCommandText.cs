namespace GitDiffMargin
{
    using System;
    using System.Runtime.InteropServices;
    using OLECMDTEXT = Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT;
    using OLECMDTEXTF = Microsoft.VisualStudio.OLE.Interop.OLECMDTEXTF;
    using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;

    /// <summary>
    /// This class provides a managed wrapper around an <see cref="OLECMDTEXT"/> structure.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [CLSCompliant(false)]
    public sealed class OleCommandText : IDisposable
    {
        /// <summary>
        /// The pointer to the underlying <see cref="OLECMDTEXT"/> structure in unmanaged memory.
        /// </summary>
        private unsafe readonly OLECMDTEXT* _oleCmdText;

        /// <summary>
        /// This is set to <see langword="true"/> when it is no longer safe to access <see cref="_oleCmdText"/>, just in
        /// case code tried to cache an instance of this class.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="true"/>, all methods in this class throw an <see cref="ObjectDisposedException"/>.
        /// </remarks>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OleCommandText"/> class from the specified pointer.
        /// </summary>
        /// <param name="oleCmdText">A pointer to the <see cref="OLECMDTEXT"/> structure in unmanaged memory.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="oleCmdText"/> is <see cref="IntPtr.Zero"/>.
        /// </exception>
        private unsafe OleCommandText(OLECMDTEXT* oleCmdText)
        {
            if (oleCmdText == null)
                throw new ArgumentNullException(nameof(oleCmdText));

            _oleCmdText = oleCmdText;
        }

        /// <summary>
        /// Get an instance of <see cref="OleCommandText"/> representing the <c>pCmdText</c> argument in a call to
        /// <see cref="IOleCommandTarget.QueryStatus"/>.
        /// </summary>
        /// <param name="pCmdText">
        /// The <c>pCmdText</c> argument passed to <see cref="IOleCommandTarget.QueryStatus"/>.
        /// </param>
        /// <returns>
        /// <para>An instance of <see cref="OleCommandText"/> wrapping the specified pointer.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if <paramref name="pCmdText"/> is <see cref="IntPtr.Zero"/>.</para>
        /// </returns>
        internal static OleCommandText FromQueryStatus(IntPtr pCmdText)
        {
            if (pCmdText == IntPtr.Zero)
                return null;

            unsafe
            {
                return new OleCommandText((OLECMDTEXT*)pCmdText);
            }
        }

        /// <summary>
        /// Gets a value indicating the type of information to be provided.
        /// </summary>
        /// <value>
        /// <para><see cref="OLECMDTEXTF.OLECMDTEXTF_NONE"/> if no extra information is requested.</para>
        /// <para>-or-</para>
        /// <para><see cref="OLECMDTEXTF.OLECMDTEXTF_NAME"/> if the object should provide the localized name of the
        /// command.</para>
        /// <para>-or-</para>
        /// <para><see cref="OLECMDTEXTF.OLECMDTEXTF_STATUS"/> if the object should provide a localized status string
        /// for the command.</para>
        /// </value>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        public OLECMDTEXTF CommandInformationKind
        {
            get
            {
                ThrowIfDisposed();

                unsafe
                {
                    return (OLECMDTEXTF)_oleCmdText->cmdtextf;
                }
            }

            internal set
            {
                ThrowIfDisposed();

                unsafe
                {
                    _oleCmdText->cmdtextf = (uint)value;
                }
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="Text"/> property.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="OLECMDTEXT"/> structure uses a caller-allocated array of characters to hold the string
        /// data. When setting the <see cref="Text"/> property, if the value (plus a terminating NUL character) exceeds
        /// this length, the string is automatically truncated to fit within the provided buffer.</para>
        /// </remarks>
        /// <value>
        /// The maximum length of the <see cref="Text"/> property.
        /// </value>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        public int MaxLength
        {
            get
            {
                ThrowIfDisposed();

                unsafe
                {
                    if (_oleCmdText->cwBuf == 0)
                        return 0;

                    return (int)(_oleCmdText->cwBuf - 1);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text associated with the command. The <see cref="CommandInformationKind"/> property
        /// specifies the specific information provided by the <see cref="Text"/> property.
        /// </summary>
        /// <remarks>
        /// <para>When setting this property, if the <paramref name="value"/> is longer than <see cref="MaxLength"/>, it
        /// is automatically truncated.</para>
        /// </remarks>
        /// <value>
        /// The text associated with the command.
        /// </value>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        public string Text
        {
            get
            {
                ThrowIfDisposed();

                unsafe
                {
                    /* The getter ensures that no more than the minimum of cwActual and cwBuf characters are read. For
                     * maximum compatibility with other implementations that may interpret the OLECMDTEXT documentation
                     * differently, the code assumes that the indicated length may or may not include a NUL terminating
                     * character, and if it does then NUL character is not included in the value returned by this
                     * property.
                     */

                    uint length = Math.Min(_oleCmdText->cwActual, _oleCmdText->cwBuf);
                    if (length == 0)
                        return string.Empty;

                    // assume, but do not require, that the buffer data be NUL-terminated
                    char* buffer = (char*)(&_oleCmdText->rgwz);
                    if (buffer[length - 1] == '\0')
                        length--;

                    return Marshal.PtrToStringUni((IntPtr)buffer, (int)length);
                }
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                ThrowIfDisposed();

                unsafe
                {
                    /* The setter for this property always ensures that the buffer is NUL-terminated. In accordance with
                     * other known implementations, the NUL character is counted in the value assigned for cwActual. The
                     * only case where a NUL character is not written is the case where cwBuf==0, and therefore no
                     * characters may be written to the array.
                     */

                    // special handling for a zero-length buffer
                    if (_oleCmdText->cwBuf == 0)
                    {
                        _oleCmdText->cwActual = (uint)value.Length + 1;
                        return;
                    }

                    char* buffer = (char*)(&_oleCmdText->rgwz);
                    char[] data = value.ToCharArray();
                    if (value.Length >= _oleCmdText->cwBuf)
                    {
                        data[_oleCmdText->cwBuf - 1] = '\0';
                        Marshal.Copy(data, 0, (IntPtr)buffer, (int)_oleCmdText->cwBuf);
                    }
                    else
                    {
                        Marshal.Copy(data, 0, (IntPtr)buffer, data.Length);
                        // NUL-terminate the buffer
                        buffer[data.Length] = '\0';
                    }

                    // always count the NUL character in the actual length
                    _oleCmdText->cwActual = (uint)(value.Length + 1);
                }
            }
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the object has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
