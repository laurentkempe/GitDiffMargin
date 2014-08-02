#region using

using GitDiffMargin.Git;
using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

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
            ViewModel = new EditorDiffMarginViewModel(marginCore, UpdateDiffDimensions);

            UserControl = new EditorDiffMarginControl {DataContext = ViewModel, Width = MarginWidth};
        }

        private void UpdateDiffDimensions(DiffViewModel diffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (TextView.IsClosed)
                return;

            var snapshot = TextView.TextBuffer.CurrentSnapshot;

            var startLineNumber = hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = startLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1;
            if (startLineNumber < 0
                || startLineNumber >= snapshot.LineCount
                || endLineNumber < 0
                || endLineNumber >= snapshot.LineCount)
            {
                diffViewModel.IsVisible = false;
                return;
            }

            var startLine = snapshot.GetLineFromLineNumber(startLineNumber);
            var endLine = snapshot.GetLineFromLineNumber(endLineNumber);

            if (startLine == null || endLine == null) return;


            if (endLine.LineNumber < startLine.LineNumber)
            {
                var span = new SnapshotSpan(endLine.Start, startLine.End);
                if (!TextView.TextViewLines.FormattedSpan.IntersectsWith(span))
                {
                    diffViewModel.IsVisible = false;
                    return;
                }
            }
            else
            {
                var span = new SnapshotSpan(startLine.Start, endLine.End);
                if (!TextView.TextViewLines.FormattedSpan.IntersectsWith(span))
                {
                    diffViewModel.IsVisible = false;
                    return;
                }
            }

            var startLineView = TextView.GetTextViewLineContainingBufferPosition(startLine.Start);
            var endLineView = TextView.GetTextViewLineContainingBufferPosition(endLine.Start);

            if (startLineView == null || endLineView == null)
            {
                diffViewModel.IsVisible = false;
                return;
            }

            if (TextView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < startLineView.Start
                || TextView.TextViewLines.FirstVisibleLine.Start > endLineView.EndIncludingLineBreak)
            {
                diffViewModel.IsVisible = false;
                return;
            }

            double startTop;
            switch (startLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                    startTop = startLineView.Top - TextView.ViewportTop;
                    break;

                case VisibilityState.Hidden:
                    startTop = startLineView.Top - TextView.ViewportTop;
                    break;

                case VisibilityState.PartiallyVisible:
                    startTop = startLineView.Top - TextView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was past the end we would have already returned
                    startTop = 0;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    diffViewModel.IsVisible = false;
                    return;
            }

            if (startTop >= TextView.ViewportHeight + TextView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                diffViewModel.IsVisible = false;
                return;
            }

            double stopBottom;
            switch (endLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                    stopBottom = endLineView.Bottom - TextView.ViewportTop;
                    break;

                case VisibilityState.Hidden:
                    stopBottom = endLineView.Bottom - TextView.ViewportTop;
                    break;

                case VisibilityState.PartiallyVisible:
                    stopBottom = endLineView.Bottom - TextView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was before the start we would have already returned
                    stopBottom = TextView.ViewportHeight;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    diffViewModel.IsVisible = false;
                    return;
            }

            if (stopBottom <= -TextView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                diffViewModel.IsVisible = false;
                return;
            }

            if (stopBottom <= startTop)
            {
                if (hunkRangeInfo.IsDeletion)
                {
                    double center = (startTop + stopBottom) / 2.0;
                    diffViewModel.Top = (center - (TextView.LineHeight / 2.0)) + TextView.LineHeight;
                    diffViewModel.Height = TextView.LineHeight;
                    diffViewModel.IsVisible = true;
                }
                else
                {
                    // could be reachable if translation changes an addition to empty
                    diffViewModel.IsVisible = false;
                }

                return;
            }

            diffViewModel.Top = startTop;
            diffViewModel.Height = stopBottom - startTop;
            diffViewModel.IsVisible = true;
        }
    }
}