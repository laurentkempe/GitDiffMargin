using System.Runtime.InteropServices;

namespace GitDiffMargin
{
    [Guid(GitDiffMarginCommandHandler.GitDiffMarginCommandSet)]
    public enum GitDiffMarginCommand
    {
        PreviousChange = 0,
        NextChange = 1,
        RollbackChange = 2,
        ShowDiff = 3,
        CopyOldText = 4,
        ShowPopup = 5,

        GitDiffToolbar = 100,

        GitDiffToolbarGroup = 150
    }
}