namespace GitDiffMargin
{
    using System.Runtime.InteropServices;

    [Guid("691DB887-6D82-46A9-B0AF-407C7F0E39BE")]
    public enum GitDiffMarginCommand
    {
        PreviousChange = 0,
        NextChange = 1,
        RollbackChange = 2,
        ShowDiff = 3,
        CopyOldText = 4,

        ToggleCompareToIndex = 50,
        CompareToIndex = 51,
        CompareToHead = 52,

        GitDiffToolbar = 100,

        GitDiffToolbarGroup = 150,
    }
}
