using System;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal sealed class ScrollDiffViewModel : DiffViewModel
    {
        internal ScrollDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore,
            Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore, updateDiffDimensions)
        {
            UpdateDimensions();
        }

        public override bool IsVisible
        {
            get => true;
            set { }
        }

        public override double Width => MarginCore.ScrollChangeWidth;
    }
}