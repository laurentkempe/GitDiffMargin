#region using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffMarginViewModel : ViewModelBase
    {
        private readonly IMarginCore _marginCore;
        private readonly Action<EditorDiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private RelayCommand<DiffViewModel> _previousChangeCommand;
        private RelayCommand<DiffViewModel> _nextChangeCommand;

        internal EditorDiffMarginViewModel(IMarginCore marginCore, Action<EditorDiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            if (marginCore == null)
                throw new ArgumentNullException("marginCore");

            _marginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            _marginCore.HunksChanged += HandleHunksChanged;

            DiffViewModels = new ObservableCollection<DiffViewModel>();
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; private set; }

        public RelayCommand<DiffViewModel> PreviousChangeCommand
        {
            get { return _previousChangeCommand ?? (_previousChangeCommand = new RelayCommand<DiffViewModel>(PreviousChange, PreviousChangeCanExecute)); }
        }

        public RelayCommand<DiffViewModel> NextChangeCommand
        {
            get { return _nextChangeCommand ?? (_nextChangeCommand = new RelayCommand<DiffViewModel>(NextChange, NextChangeCanExecute)); }
        }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
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

            //todo uncomment this because it is sued for the editor margin
            //currentDiffViewModel.ShowPopup = false;
        }

        private void HandleHunksChanged(object sender, IEnumerable<HunkRangeInfo> hunkRangeInfos)
        {
            //todo uncomment this because it is sued for the editor margin
            //if (DiffViewModels.Any(dvm => dvm.ShowPopup)) return;

            DiffViewModels.Clear();

            foreach (var diffViewModel in hunkRangeInfos.Select(hunkRangeInfo => new EditorDiffViewModel(hunkRangeInfo, _marginCore, _updateDiffDimensions)))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }
    }
}