namespace GitDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(NextChangeCommandHandler))]
    internal class NextChangeCommandHandler : GitDiffMarginCommandHandler<NextChangeCommandArgs>
    {
        public NextChangeCommandHandler()
            : base(GitDiffMarginCommand.NextChange)
        {
        }

        public override string DisplayName => "Next Change";
    }
}
