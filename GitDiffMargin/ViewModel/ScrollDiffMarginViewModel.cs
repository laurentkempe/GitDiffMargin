using System;
using System.Collections.Generic;
using System.Linq;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal class ScrollDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly IMarginCore _marginCore;
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;

        internal ScrollDiffMarginViewModel(IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            if (marginCore == null)
                throw new ArgumentNullException("marginCore");

            _marginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            _marginCore.HunksChanged += HandleHunksChanged;
        }

        protected override void HandleHunksChanged(object sender, IEnumerable<HunkRangeInfo> hunkRangeInfos)
        {
            DiffViewModels.Clear();

            foreach (var diffViewModel in hunkRangeInfos.Select(hunkRangeInfo => new ScrollDiffViewModel(hunkRangeInfo, _marginCore, _updateDiffDimensions)))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }
    }
}