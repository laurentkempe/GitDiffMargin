using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;

namespace GitDiffMargin.Commands
{
    internal class CopyOldTextCommandArgs : EditorCommandArgs
    {
        public CopyOldTextCommandArgs(ITextView textView, ITextBuffer subjectBuffer)
            : base(textView, subjectBuffer)
        {
        }
    }
}