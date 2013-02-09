using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin
{
    public class GitDiffMarginTextViewOptions
    {
        public const string DiffMarginName = GitDiffMargin.MarginName + "/DiffMarginName";

        public static readonly EditorOptionKey<bool> DiffMarginId = new EditorOptionKey<bool>(DiffMarginName);

    }
}