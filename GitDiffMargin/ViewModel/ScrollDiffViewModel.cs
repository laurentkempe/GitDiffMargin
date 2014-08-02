using System;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal class ScrollDiffViewModel : DiffViewModel
    {
        internal ScrollDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore, updateDiffDimensions)
        {
            UpdateDimensions();
        }

        public override bool IsVisible
        {
            get { return true; }
            set { }
        }
    }
}