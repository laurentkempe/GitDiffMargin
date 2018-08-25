﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin.Commands
{
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