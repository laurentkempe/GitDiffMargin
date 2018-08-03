namespace GitDiffMargin.Commands
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Shell;
    using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
    using OLECMD = Microsoft.VisualStudio.OLE.Interop.OLECMD;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;

    internal abstract class ShimCommandHandler<T> : ICommandHandler<T>
        where T : CommandArgs
    {
        private readonly Guid _commandSet;
        private readonly uint _commandId;

        protected ShimCommandHandler(Guid commandSet, uint commandId)
        {
            _commandSet = commandSet;
            _commandId = commandId;
        }

        public abstract string DisplayName
        {
            get;
        }

        public virtual CommandState GetCommandState(T args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OLECMD[] command = { new OLECMD { cmdID = _commandId } };
            ErrorHandler.ThrowOnFailure(GetCommandTarget(args).QueryStatus(_commandSet, 1, command, IntPtr.Zero));
            if ((command[0].cmdf & (uint)OLECMDF.OLECMDF_SUPPORTED) == 0)
                return CommandState.Unspecified;
            else if ((command[0].cmdf & (uint)OLECMDF.OLECMDF_ENABLED) == 0)
                return CommandState.Unavailable;
            else
                return CommandState.Available;
        }

        public virtual bool ExecuteCommand(T args, CommandExecutionContext executionContext)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return ErrorHandler.Succeeded(GetCommandTarget(args).Exec(_commandSet, _commandId, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero));
        }

        protected abstract IOleCommandTarget GetCommandTarget(T args);
    }
}
