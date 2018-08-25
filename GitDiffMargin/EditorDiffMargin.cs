#region using

using GitDiffMargin.Core;
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

        internal EditorDiffMargin(IWpfTextView textView, IMarginCore marginCore)
            : base(textView)
        {
            ViewModel = new EditorDiffMarginViewModel(marginCore, UpdateDiffDimensions);

            UserControl = new EditorDiffMarginControl {DataContext = ViewModel, Width = MarginWidth};
        }

        protected override string MarginName => MarginNameConst;

        private void UpdateDiffDimensions(DiffViewModel diffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (TextView.IsClosed)
                return;

            bool? visible;
            if (diffViewModel.IsDeletion)
                visible = UpdateDeletedDiffDimensions(diffViewModel, hunkRangeInfo);
            else
                visible = UpdateNormalDiffDimensions(diffViewModel, hunkRangeInfo);

            if (visible.HasValue)
                diffViewModel.IsVisible = visible.Value;
        }

        private bool? UpdateNormalDiffDimensions(DiffViewModel diffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (hunkRangeInfo.NewHunkRange.NumberOfLines <= 0) return false;

            var snapshot = TextView.TextBuffer.CurrentSnapshot;

            var startLineNumber = hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = startLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1;
            if (startLineNumber < 0
                || startLineNumber >= snapshot.LineCount
                || endLineNumber < 0
                || endLineNumber >= snapshot.LineCount)
                return false;

            var startLine = snapshot.GetLineFromLineNumber(startLineNumber);
            var endLine = snapshot.GetLineFromLineNumber(endLineNumber);

            if (startLine == null || endLine == null)
                return null;

            var span = new SnapshotSpan(startLine.Start, endLine.End);
            if (!TextView.TextViewLines.FormattedSpan.IntersectsWith(span))
                return false;

            var startLineView = TextView.GetTextViewLineContainingBufferPosition(startLine.Start);
            var endLineView = TextView.GetTextViewLineContainingBufferPosition(endLine.Start);

            if (startLineView == null || endLineView == null)
                return false;

            if (TextView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < startLineView.Start) return false;

            if (TextView.TextViewLines.FirstVisibleLine.Start > endLineView.EndIncludingLineBreak) return false;

            double startTop;
            switch (startLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                case VisibilityState.Hidden:
                case VisibilityState.PartiallyVisible:
                    startTop = startLineView.Top - TextView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was past the end we would have already returned
                    startTop = 0;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    return false;
            }

            double stopBottom;
            switch (endLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                case VisibilityState.Hidden:
                case VisibilityState.PartiallyVisible:
                    stopBottom = endLineView.Bottom - TextView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was before the start we would have already returned
                    stopBottom = TextView.ViewportHeight;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    return false;
            }

            diffViewModel.Top = startTop;
            diffViewModel.Height = stopBottom - startTop;
            return true;
        }

        private bool? UpdateDeletedDiffDimensions(DiffViewModel diffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (hunkRangeInfo.NewHunkRange.NumberOfLines != 0) return false;

            var snapshot = TextView.TextBuffer.CurrentSnapshot;

            var followingLineNumber = hunkRangeInfo.NewHunkRange.StartingLineNumber + 1;
            if (followingLineNumber < 0 || followingLineNumber >= snapshot.LineCount)
                return false;

            var followingLine = snapshot.GetLineFromLineNumber(followingLineNumber);
            if (followingLine == null)
                return null;

            var span = new SnapshotSpan(followingLine.Start, followingLine.End);
            if (!TextView.TextViewLines.FormattedSpan.IntersectsWith(span))
                return false;

            var followingLineView = TextView.GetTextViewLineContainingBufferPosition(followingLine.Start);
            if (followingLineView == null)
                return false;

            if (TextView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < followingLineView.Start) return false;

            if (TextView.TextViewLines.FirstVisibleLine.Start > followingLineView.EndIncludingLineBreak) return false;

            double followingTop;
            switch (followingLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                case VisibilityState.Hidden:
                case VisibilityState.PartiallyVisible:
                    followingTop = followingLineView.Top - TextView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was past the end we would have already returned
                    followingTop = 0;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    return false;
            }

            var center = followingTop;
            var height = TextView.LineHeight;
            diffViewModel.Top = center - height / 2.0;
            diffViewModel.Height = TextView.LineHeight;
            return true;
        }
    }
}