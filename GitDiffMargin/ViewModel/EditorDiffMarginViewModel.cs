#region using

using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly IMarginCore _marginCore;
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private RelayCommand<DiffViewModel> _previousChangeCommand;
        private RelayCommand<DiffViewModel> _nextChangeCommand;

        internal EditorDiffMarginViewModel(IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            if (marginCore == null)
                throw new ArgumentNullException("marginCore");

            _marginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            _marginCore.HunksChanged += HandleHunksChanged;
        }

        public RelayCommand<DiffViewModel> PreviousChangeCommand
        {
            get { return _previousChangeCommand ?? (_previousChangeCommand = new RelayCommand<DiffViewModel>(PreviousChange, PreviousChangeCanExecute)); }
        }

        public RelayCommand<DiffViewModel> NextChangeCommand
        {
            get { return _nextChangeCommand ?? (_nextChangeCommand = new RelayCommand<DiffViewModel>(NextChange, NextChangeCanExecute)); }
        }

        private bool PreviousChangeCanExecute(DiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) > 0;
        }

        private bool NextChangeCanExecute(DiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) < (DiffViewModels.Count - 1);
        }

        private void PreviousChange(DiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, -1);
        }

        private void NextChange(DiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, +1);
        }

        private void MoveToChange(DiffViewModel currentDiffViewModel, int indexModifier)
        {
            var diffViewModelIndex = DiffViewModels.IndexOf(currentDiffViewModel) + indexModifier;
            var diffViewModel  = DiffViewModels[diffViewModelIndex];

            _marginCore.MoveToChange(diffViewModel.LineNumber);    

            ((EditorDiffViewModel)currentDiffViewModel).ShowPopup = false;
        }

        protected override void HandleHunksChanged(object sender, IEnumerable<HunkRangeInfo> hunkRangeInfos)
        {
            if (DiffViewModels.Cast<EditorDiffViewModel>().Any(dvm => dvm.ShowPopup)) return;

            DiffViewModels.Clear();

            foreach (var diffViewModel in hunkRangeInfos.Select(hunkRangeInfo => new EditorDiffViewModel(hunkRangeInfo, _marginCore, _updateDiffDimensions)))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }
    }
}