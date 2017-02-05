using System;
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

        public ObservableCollection<DiffViewModel> DiffViewModels { get; }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
                diffViewModel.RefreshPosition();
        }

        protected virtual void HandleHunksChanged(object sender, HunksChangedEventArgs e)
        {
            DiffViewModels.Clear();

            foreach (var diffViewModel in e.Hunks.Select(CreateDiffViewModel))
                DiffViewModels.Add(diffViewModel);
        }

        protected abstract DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo);

        public override void Cleanup()
        {
            MarginCore.HunksChanged -= HandleHunksChanged;

            base.Cleanup();
        }
    }
}