using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin.Commands
{
    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(ShowPopupCommandHandler))]
    internal class ShowPopupCommandHandler : GitDiffMarginCommandHandler<ShowPopupCommandArgs>
    {
        public ShowPopupCommandHandler()
            : base(GitDiffMarginCommand.ShowPopup)
        {
        }

        public override string DisplayName => "Show Popup";
    }
}