using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin
{
    public static class GitDiffMarginTextViewOptions
    {
        public static EditorOptionKey<bool> DiffMarginEnabledId = new EditorOptionKey<bool>("GitDiffMargin/DiffMarginEnabled");
    }
}
