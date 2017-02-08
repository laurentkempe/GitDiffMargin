#region using

using System;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private RelayCommand<DiffViewModel> _previousChangeCommand;
        private RelayCommand<DiffViewModel> _nextChangeCommand;
        private RelayCommand _toggleIgnoreWhiteSpacesCommand;

        internal EditorDiffMarginViewModel(IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions) :
            base(marginCore)
        {
            if (updateDiffDimensions == null)
                throw new ArgumentNullException("updateDiffDimensions");

            _updateDiffDimensions = updateDiffDimensions;
        }

        public bool IgnoreWhiteSpaces => MarginCore.IgnoreWhiteSpaces;

        public RelayCommand<DiffViewModel> PreviousChangeCommand
        {
            get { return _previousChangeCommand ?? (_previousChangeCommand = new RelayCommand<DiffViewModel>(PreviousChange, PreviousChangeCanExecute)); }
        }

        public RelayCommand<DiffViewModel> NextChangeCommand
        {
            get { return _nextChangeCommand ?? (_nextChangeCommand = new RelayCommand<DiffViewModel>(NextChange, NextChangeCanExecute)); }
        }

        public RelayCommand ToggleIgnoreWhiteSpacesCommand
        {
            get { return _toggleIgnoreWhiteSpacesCommand ?? (_toggleIgnoreWhiteSpacesCommand = new RelayCommand(ToggleIgnoreWhiteSpaces, CanExecuteToggleIgnoreWhiteSpaces)); }
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

        private void ToggleIgnoreWhiteSpaces()
        {
            MarginCore.ToggleIgnoreWhiteSpace();
        }

        private bool CanExecuteToggleIgnoreWhiteSpaces()
        {
            return true;
        }

        public void MoveToChange(DiffViewModel currentDiffViewModel, int indexModifier)
        {
            var diffViewModelIndex = DiffViewModels.IndexOf(currentDiffViewModel) + indexModifier;
            var diffViewModel  = DiffViewModels[diffViewModelIndex];

            MarginCore.MoveToChange(diffViewModel.LineNumber);

            ((EditorDiffViewModel)currentDiffViewModel).ShowPopup = false;
        }

        protected override void HandleHunksChanged(object sender, HunksChangedEventArgs e)
        {
            if (DiffViewModels.Cast<EditorDiffViewModel>().Any(dvm => dvm.ShowPopup)) return;

            base.HandleHunksChanged(sender, e);
        }

        protected override DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo)
        {
            return new EditorDiffViewModel(hunkRangeInfo, MarginCore, _updateDiffDimensions);
        }
    }
}