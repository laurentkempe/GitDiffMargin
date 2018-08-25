using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace GitDiffMargin.Commands
{
    internal abstract class GitDiffMarginCommandHandler<T> : ShimCommandHandler<T>
        where T : EditorCommandArgs
    {
        protected GitDiffMarginCommandHandler(GitDiffMarginCommand commandId)
            : base(new Guid(GitDiffMarginCommandHandler.GitDiffMarginCommandSet), (uint) commandId)
        {
        }

        protected override IOleCommandTarget GetCommandTarget(T args)
        {
            return args.TextView.Properties.GetProperty<GitDiffMarginCommandHandler>(
                typeof(GitDiffMarginCommandHandler));
        }
    }
}