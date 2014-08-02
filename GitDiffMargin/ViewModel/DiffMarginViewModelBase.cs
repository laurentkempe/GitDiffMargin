using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal abstract class DiffMarginViewModelBase : ViewModelBase
    {
        protected DiffMarginViewModelBase()
        {
            DiffViewModels = new ObservableCollection<DiffViewModel>();
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; private set; }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        protected abstract void HandleHunksChanged(object sender, IEnumerable<HunkRangeInfo> hunkRangeInfos);
    }
}