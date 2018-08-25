using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace GitDiffMargin.Commands
{
    internal abstract class ShimCommandHandler<T> : ICommandHandler<T>
        where T : CommandArgs
    {
        private readonly uint _commandId;
        private readonly Guid _commandSet;

        protected ShimCommandHandler(Guid commandSet, uint commandId)
        {
            _commandSet = commandSet;
            _commandId = commandId;
        }

        public abstract string DisplayName { get; }

        public virtual CommandState GetCommandState(T args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OLECMD[] command = {new OLECMD {cmdID = _commandId}};
            ErrorHandler.ThrowOnFailure(GetCommandTarget(args).QueryStatus(_commandSet, 1, command, IntPtr.Zero));
            if ((command[0].cmdf & (uint) OLECMDF.OLECMDF_SUPPORTED) == 0)
                return CommandState.Unspecified;

            return (command[0].cmdf & (uint) OLECMDF.OLECMDF_ENABLED) == 0
                ? CommandState.Unavailable
                : CommandState.Available;
        }

        public virtual bool ExecuteCommand(T args, CommandExecutionContext executionContext)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return ErrorHandler.Succeeded(GetCommandTarget(args).Exec(_commandSet, _commandId,
                (uint) OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero));
        }

        protected abstract IOleCommandTarget GetCommandTarget(T args);
    }
}