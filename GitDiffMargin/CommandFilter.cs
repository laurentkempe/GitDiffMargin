namespace GitDiffMargin
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;

    using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
    using OLECMD = Microsoft.VisualStudio.OLE.Interop.OLECMD;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
    using OLECMDTEXT = Microsoft.VisualStudio.OLE.Interop.OLECMDTEXT;
    using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
    using VsMenus = Microsoft.VisualStudio.Shell.VsMenus;

    /// <summary>
    /// This is the base class for implementations of <see cref="IOleCommandTarget"/> in managed code.
    /// </summary>
    /// <threadsafety/>
    [ComVisible(true)]
    [CLSCompliant(false)]
    public abstract class CommandFilter : IOleCommandTarget, IDisposable
    {
        /// <summary>
        /// This is the backing field for the <see cref="Enabled"/> property.
        /// </summary>
        private bool _connected;

        /// <summary>
        /// This field stores the next <see cref="IOleCommandTarget"/> in the filter chain.
        /// </summary>
        private IOleCommandTarget _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandFilter"/> class.
        /// </summary>
        protected CommandFilter()
        {
        }

        /// <summary>
        /// Gets or sets whether the command filter is currently enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the current instance has been disposed.</exception>
        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();
                return _connected;
            }

            set
            {
                ThrowIfDisposed();
                if (_connected == value)
                    return;

                if (value)
                {
                    _next = Connect();
                    _connected = value;
                }
                else
                {
                    try
                    {
                        Disconnect();
                    }
                    catch (Exception e)
                    {
                        if (!IsDisposing || ErrorHandler.IsCriticalException(e))
                            throw;
                    }
                    finally
                    {
                        _next = null;
                        _connected = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the current instance is disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the current instance is currently being disposed.
        /// </summary>
        protected bool IsDisposing
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsDisposing)
                throw new InvalidOperationException("Detected a recursive invocation of Dispose");

            try
            {
                IsDisposing = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            finally
            {
                IsDisposing = false;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance.
        /// </summary>
        /// <remarks>
        /// When <paramref name="disposing"/> is <see langword="false"/>, the default implementation
        /// sets <see cref="Enabled"/> to <see langword="false"/>.
        /// </remarks>
        /// <param name="disposing"><see langword="true"/> if this method is being called from <see cref="Dispose()"/>; otherwise, <see langword="false"/> if this method is being called from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Enabled = false;
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Enables the command filter by connecting it to a chain of command targets.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// Do not call this method directly. This method is called as necessary when <see cref="Enabled"/> is set to <see langword="true"/>.
        /// </note>
        /// </remarks>
        /// <returns>The next command target in the chain.</returns>
        protected abstract IOleCommandTarget Connect();

        /// <summary>
        /// Disables the command filter by disconnecting it from a chain of command targets.
        /// </summary>
        /// <remarks>
        /// <note type="caller">
        /// Do not call this method directly. This method is called as necessary when <see cref="Enabled"/> is set to <see langword="false"/>.
        /// </note>
        /// </remarks>
        protected abstract void Disconnect();

        /// <summary>
        /// Throw an <see cref="ObjectDisposedException"/> if the current instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the current instance has been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// This method provides the primary implementation for <see cref="IOleCommandTarget.Exec"/> in all cases where
        /// <see cref="OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP"/> was not requested.
        /// </summary>
        /// <param name="guidCmdGroup">The command group.</param>
        /// <param name="nCmdID">The command ID.</param>
        /// <param name="nCmdexecopt">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <returns>
        /// <para>This method returns <see cref="VSConstants.S_OK"/> on success.</para>
        /// <para>-or-</para>
        /// <para><see cref="OleConstants.OLECMDERR_E_NOTSUPPORTED"/> if the command is not handled by this command
        /// filter, and no other command filters are available to process the request.</para>
        /// </returns>
        private int ExecCommand(ref Guid guidCmdGroup, uint nCmdID, OLECMDEXECOPT nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int rc = VSConstants.S_OK;

            if (!HandlePreExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut))
            {
                // Pass it along the chain.
                rc = this.InnerExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                if (!ErrorHandler.Succeeded(rc))
                    return rc;

                HandlePostExec(ref guidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return rc;
        }

        /// <summary>
        /// This method supports the implementation for commands which are directly implemented by this command filter.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation returns <see langword="false"/> for all commands.</para>
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <returns>
        /// <see langword="true"/> if this command filter handled the command; otherwise, <see langword="false"/>
        /// to call the next <see cref="IOleCommandTarget"/> in the chain.
        /// </returns>
        protected virtual bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            return false;
        }

        /// <summary>
        /// Pass a command on to the next command filter in the chain, if available.
        /// </summary>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <returns>
        /// <para>This method returns the result of calling <see cref="IOleCommandTarget.Exec"/> on the next command
        /// filter in the chain.</para>
        /// <para>-or-</para>
        /// <para><see cref="OleConstants.OLECMDERR_E_NOTSUPPORTED"/> if no other command filters are available to
        /// process the request.</para>
        /// </returns>
        private int InnerExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (_next != null)
                return _next.Exec(ref commandGroup, commandId, (uint)executionOptions, pvaIn, pvaOut);

            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// This method supports specialized handling in response to commands that are successfully handled by another
        /// command target.
        /// </summary>
        /// <remarks>
        /// <para>This method is only called if <see cref="HandlePreExec"/> for the current instance returned
        /// <see langword="false"/> and the next command target in the chain returned a value indicating the command
        /// execution succeeded.</para>
        ///
        /// <para>
        /// The default implementation is empty.
        /// </para>
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        protected virtual void HandlePostExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
        }

        /// <summary>
        /// The expected behavior of this method is not clear.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation returns <see cref="OleConstants.OLECMDERR_E_NOTSUPPORTED"/> for all
        /// commands.</para>
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <returns>The expected behavior of this method is not clear.</returns>
        protected virtual int QueryParameterList(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// The expected behavior of this method is not clear.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation returns <see cref="OleConstants.OLECMDERR_E_NOTSUPPORTED"/> for all
        /// commands.</para>
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="executionOptions">The OLE command execution options.</param>
        /// <param name="pvaIn">An optional pointer to the command argument(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <param name="pvaOut">An optional pointer to the command result(s). The semantics of this parameter are
        /// specific to a particular command.</param>
        /// <returns>The expected behavior of this method is not clear.</returns>
        protected virtual int ShowHelp(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            return (int)OleConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        /// <summary>
        /// Gets the current status of a particular command.
        /// </summary>
        /// <remarks>
        /// The base implementation returns 0 for all commands, indicating the command is not supported by this command
        /// filter.
        /// </remarks>
        /// <param name="commandGroup">The command group.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="oleCommandText">A wrapper around the <see cref="OLECMDTEXT"/> object passed to
        /// <see cref="IOleCommandTarget.QueryStatus"/>, or <see langword="null"/> if this parameter should be
        /// ignored.</param>
        /// <returns>A combination of values from the <see cref="OLECMDF"/> enumeration indicating the current status of
        /// a particular command, or 0 if the command is not recognized by the current command filter.</returns>
        protected virtual OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId, OleCommandText oleCommandText)
        {
            return default(OLECMDF);
        }

        /// <inheritdoc/>
        int IOleCommandTarget.Exec(ref Guid guidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ushort lo = (ushort)(nCmdexecopt & (uint)0xffff);
            ushort hi = (ushort)(nCmdexecopt >> 16);

            switch (lo)
            {
            case (ushort)OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP:
                if (hi == VsMenus.VSCmdOptQueryParameterList)
                {
                    int hr = QueryParameterList(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
                    if (ErrorHandler.Succeeded(hr))
                        return hr;

                    return InnerExec(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
                }
                else
                {
                    int hr = ShowHelp(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
                    if (ErrorHandler.Succeeded(hr))
                        return hr;

                    return InnerExec(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
                }

            default:
                return ExecCommand(ref guidCmdGroup, nCmdID, (OLECMDEXECOPT)nCmdexecopt, pvaIn, pvaOut);
            }
        }

        /// <inheritdoc/>
        int IOleCommandTarget.QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            using (OleCommandText oleCommandText = OleCommandText.FromQueryStatus(pCmdText))
            {
                Guid cmdGroup = guidCmdGroup;
                for (uint i = 0; i < cCmds; i++)
                {
                    OLECMDF status = QueryCommandStatus(ref cmdGroup, prgCmds[i].cmdID, oleCommandText);
                    if (status == default(OLECMDF) && _next != null)
                    {
                        int hr = _next.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                    }
                    else
                    {
                        prgCmds[i].cmdf = (uint)status;
                    }
                }

                return VSConstants.S_OK;
            }
        }
    }
}
