#if !LEGACY_COMMANDS

namespace GitDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(RollbackChangeCommandHandler))]
    internal class RollbackChangeCommandHandler : GitDiffMarginCommandHandler<RollbackChangeCommandArgs>
    {
        public RollbackChangeCommandHandler()
            : base(GitDiffMarginCommand.RollbackChange)
        {
        }

        public override string DisplayName => "Rollback Change";
    }
}

#endif
