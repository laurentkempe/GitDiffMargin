using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal abstract class DiffMarginViewModelBase : ViewModelBase
    {
        protected readonly IMarginCore MarginCore;

        protected DiffMarginViewModelBase(IMarginCore marginCore)
        {
            if (marginCore == null)
                throw new ArgumentNullException("marginCore");

            MarginCore = marginCore;

            DiffViewModels = new ObservableCollection<DiffViewModel>();

            MarginCore.HunksChanged += HandleHunksChanged;
        }

        public bool HasDiffs { get; private set; }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; private set; }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        protected virtual void HandleHunksChanged(object sender, HunksChangedEventArgs e)
        {
            DiffViewModels.Clear();

            HasDiffs = e.Hunks.Any();

            var hunks = e.Hunks;

            if (MarginCore.IgnoreWhiteSpaces)
            {
                hunks = hunks.Where(hunk => !hunk.IsWhiteSpaceChange);
            }

            foreach (var diffViewModel in hunks.Select(CreateDiffViewModel))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }

        protected abstract DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo);

        public override void Cleanup()
        {
            MarginCore.HunksChanged -= HandleHunksChanged;

            base.Cleanup();
        }
    }
}