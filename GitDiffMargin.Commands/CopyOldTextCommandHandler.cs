namespace GitDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(CopyOldTextCommandHandler))]
    internal class CopyOldTextCommandHandler : GitDiffMarginCommandHandler<CopyOldTextCommandArgs>
    {
        public CopyOldTextCommandHandler()
            : base(GitDiffMarginCommand.CopyOldText)
        {
        }

        public override string DisplayName => "Copy Old Text";
    }
}
