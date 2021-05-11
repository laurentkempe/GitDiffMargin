#if !LEGACY_COMMANDS

namespace GitDiffMargin.Commands
{
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Editor.Commanding;

    internal class NextChangeCommandArgs : EditorCommandArgs
    {
        public NextChangeCommandArgs(ITextView textView, ITextBuffer subjectBuffer)
            : base(textView, subjectBuffer)
        {
        }
    }
}

#endif
