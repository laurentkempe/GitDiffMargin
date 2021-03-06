﻿#if !LEGACY_COMMANDS

namespace GitDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

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

#endif
