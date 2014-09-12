using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin
{
    public static class GitDiffMarginTextViewOptions
    {
        public const string DiffMarginName = EditorDiffMargin.MarginNameConst + "/DiffMarginName";

        public static readonly EditorOptionKey<bool> DiffMarginId = new EditorOptionKey<bool>(DiffMarginName);

    }
}
