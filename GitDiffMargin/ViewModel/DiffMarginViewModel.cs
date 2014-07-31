#region using

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffMarginViewModel : ViewModelBase
    {
        private readonly EditorDiffMargin _margin;
        private readonly IWpfTextView _textView;
        private readonly IGitCommands _gitCommands;
        private readonly DiffUpdateBackgroundParser _parser;
        private RelayCommand<EditorDiffViewModel> _previousChangeCommand;
        private RelayCommand<EditorDiffViewModel> _nextChangeCommand;

        internal DiffMarginViewModel(EditorDiffMargin margin, IWpfTextView textView, EditorMarginFactory factory)
        {
            if (margin == null)
                throw new ArgumentNullException("margin");
            if (textView == null)
                throw new ArgumentNullException("textView");

            _margin = margin;
            _gitCommands = new GitCommands(factory.ServiceProvider);
            DiffViewModels = new ObservableCollection<EditorDiffViewModel>();

            _textView = textView;

            _parser = new DiffUpdateBackgroundParser(textView.TextBuffer, textView.TextDataModel.DocumentBuffer, TaskScheduler.Default, factory.TextDocumentFactoryService, _gitCommands);
            _parser.ParseComplete += HandleParseComplete;
            _parser.RequestParse(false);
        }
       
        public ObservableCollection<EditorDiffViewModel> DiffViewModels { get; private set; }

        public RelayCommand<EditorDiffViewModel> PreviousChangeCommand
        {
            get { return _previousChangeCommand ?? (_previousChangeCommand = new RelayCommand<EditorDiffViewModel>(PreviousChange, PreviousChangeCanExecute)); }
        }

        public RelayCommand<EditorDiffViewModel> NextChangeCommand
        {
            get { return _nextChangeCommand ?? (_nextChangeCommand = new RelayCommand<EditorDiffViewModel>(NextChange, NextChangeCanExecute)); }
        }

        private bool PreviousChangeCanExecute(EditorDiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) > 0;
        }

        private bool NextChangeCanExecute(EditorDiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) < (DiffViewModels.Count - 1);
        }

        private void PreviousChange(EditorDiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, -1);
        }

        private void NextChange(EditorDiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, +1);
        }

        private void MoveToChange(EditorDiffViewModel currentEditorDiffViewModel, int indexModifier)
        {
            var diffViewModelIndex = DiffViewModels.IndexOf(currentEditorDiffViewModel) + indexModifier;
            var diffViewModel  = DiffViewModels[diffViewModelIndex];
            var diffLine = _textView.TextSnapshot.GetLineFromLineNumber(diffViewModel.LineNumber);
            currentEditorDiffViewModel.ShowPopup = false;

            _textView.VisualElement.Focus();
            _textView.Caret.MoveTo(diffLine.Start);
            _textView.Caret.EnsureVisible();
        }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        public override void Cleanup()
        {
            _parser.Dispose();
            base.Cleanup();
        }

        private void HandleParseComplete(object sender, ParseResultEventArgs e)
        {
            _textView.VisualElement.Dispatcher.BeginInvoke((Action) (() =>
                                                                       {
                                                                           //todo do not clear if it the same collection returned

                                                                           if (DiffViewModels.Any(dvm => dvm.ShowPopup)) return;

                                                                           DiffViewModels.Clear();

                                                                           var diffResult = e as DiffParseResultEventArgs;
                                                                           if (diffResult == null) return;
                                                                           
                                                                           foreach (var diffViewModel in diffResult.Diff.Select(hunkRangeInfo => new EditorDiffViewModel(_margin, hunkRangeInfo, _textView, _gitCommands)))
                                                                           {
                                                                               DiffViewModels.Add(diffViewModel);
                                                                           }
                                                                       }));
        }
    }
}