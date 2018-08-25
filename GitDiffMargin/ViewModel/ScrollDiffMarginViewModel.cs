using System;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal class ScrollDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;

        internal ScrollDiffMarginViewModel(IMarginCore marginCore,
            Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions) :
            base(marginCore)
        {
            if (updateDiffDimensions == null)
                throw new ArgumentNullException(nameof(updateDiffDimensions));

            _updateDiffDimensions = updateDiffDimensions;
        }

        protected override DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo)
        {
            return new ScrollDiffViewModel(hunkRangeInfo, MarginCore, _updateDiffDimensions);
        }
    }
}