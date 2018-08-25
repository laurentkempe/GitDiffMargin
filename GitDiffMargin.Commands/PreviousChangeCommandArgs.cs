using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace GitDiffMargin.Commands
{
    internal class PreviousChangeCommandArgs : EditorCommandArgs
    {
        public PreviousChangeCommandArgs(ITextView textView, ITextBuffer subjectBuffer)
            : base(textView, subjectBuffer)
        {
        }
    }
}