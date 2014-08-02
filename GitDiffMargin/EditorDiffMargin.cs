#region using

using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin
{
    internal sealed class EditorDiffMargin : DiffMarginBase
    {
        private const double MarginWidth = 10.0;

        public const string MarginNameConst = "EditorDiffMargin";

        protected override string MarginName
        {
            get { return MarginNameConst; }
        }

        internal EditorDiffMargin(IWpfTextView textView, IMarginCore marginCore)
            : base(textView, marginCore)
        {
            UserControl = new EditorDiffMarginControl();
            ViewModel = new DiffMarginViewModel(this, textView, marginCore);
            UserControl.DataContext = ViewModel;
            UserControl.Width = MarginWidth;
        }
    }
}