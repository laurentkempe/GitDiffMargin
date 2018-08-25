using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin.Commands
{
    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(PreviousChangeCommandHandler))]
    internal class PreviousChangeCommandHandler : GitDiffMarginCommandHandler<PreviousChangeCommandArgs>
    {
        public PreviousChangeCommandHandler()
            : base(GitDiffMarginCommand.PreviousChange)
        {
        }

        public override string DisplayName => "Previous Change";
    }
}