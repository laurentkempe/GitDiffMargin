using System;
using GitDiffMargin.Core;
using GitDiffMargin.Git;
using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin
{
    internal sealed class ScrollDiffMargin : DiffMarginBase
    {
        private const double MarginWidth = 4.0;

        public const string MarginNameConst = "ScrollDiffMargin";
        private readonly IVerticalScrollBar _scrollBar;

        internal ScrollDiffMargin(IWpfTextView textView, IMarginCore marginCore, IWpfTextViewMargin containerMargin)
            : base(textView)
        {
            var scrollBarMargin = containerMargin.GetTextViewMargin(PredefinedMarginNames.VerticalScrollBar);
            // ReSharper disable once SuspiciousTypeConversion.Global
            _scrollBar = (IVerticalScrollBar) scrollBarMargin;

            ViewModel = new ScrollDiffMarginViewModel(marginCore, UpdateDiffDimensions);

            UserControl = new ScrollDiffMarginControl
                {DataContext = ViewModel, Width = MarginWidth, MaxWidth = MarginWidth, MinWidth = MarginWidth};
        }

        protected override string MarginName => MarginNameConst;

        private void UpdateDiffDimensions(DiffViewModel diffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (TextView.IsClosed)
                return;

            var startLineNumber = hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = startLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1;

            var snapshot = TextView.TextBuffer.CurrentSnapshot;

            if (startLineNumber < 0
                || startLineNumber >= snapshot.LineCount
                || endLineNumber < 0
                || endLineNumber >= snapshot.LineCount)
                return;

            var startLine = snapshot.GetLineFromLineNumber(startLineNumber);
            var endLine = snapshot.GetLineFromLineNumber(endLineNumber);

            if (startLine == null || endLine == null) return;

            var mapTop = _scrollBar.Map.GetCoordinateAtBufferPosition(startLine.Start) - 0.5;
            var mapBottom = _scrollBar.Map.GetCoordinateAtBufferPosition(endLine.End) + 0.5;

            diffViewModel.Top = Math.Round(_scrollBar.GetYCoordinateOfScrollMapPosition(mapTop)) - 2.0;
            diffViewModel.Height = Math.Round(_scrollBar.GetYCoordinateOfScrollMapPosition(mapBottom)) -
                                   diffViewModel.Top + 2.0;
        }
    }
}