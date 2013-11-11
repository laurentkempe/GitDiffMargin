#region using

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffMarginViewModel : ViewModelBase
    {
        private readonly GitDiffMargin _margin;
        private readonly IWpfTextView _textView;
        private readonly DiffUpdateBackgroundParser _parser;
        private RelayCommand<DiffViewModel> _previousChangeCommand;
        private RelayCommand<DiffViewModel> _nextChangeCommand;

        internal DiffMarginViewModel(GitDiffMargin margin, IWpfTextView textView, ITextDocumentFactoryService textDocumentFactoryService, IGitCommands gitCommands)
        {
            if (margin == null)
                throw new ArgumentNullException("margin");
            if (textView == null)
                throw new ArgumentNullException("textView");
            if (textDocumentFactoryService == null)
                throw new ArgumentNullException("textDocumentFactoryService");
            if (gitCommands == null)
                throw new ArgumentNullException("gitCommands");

            _margin = margin;
            _textView = textView;
            DiffViewModels = new ObservableCollection<DiffViewModel>();

            _textView.LayoutChanged += OnLayoutChanged;

            _parser = new DiffUpdateBackgroundParser(textView.TextBuffer, textView.TextDataModel.DocumentBuffer, TaskScheduler.Default, textDocumentFactoryService, gitCommands);
            _parser.ParseComplete += HandleParseComplete;
            _parser.RequestParse(false);
        }
       
        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            RefreshDiffViewModelPositions();
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

        public override void Cleanup()
        {
            _parser.Dispose();
            base.Cleanup();
        }

        private bool PreviousChangeCanExecute(DiffViewModel currentDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentDiffViewModel) > 0;
        }

        private bool NextChangeCanExecute(DiffViewModel currentDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentDiffViewModel) < (DiffViewModels.Count - 1);
        }

        private void PreviousChange(DiffViewModel currentDiffViewModel)
        {
            MoveToChange(currentDiffViewModel, -1);
        }

        private void NextChange(DiffViewModel currentDiffViewModel)
        {
            MoveToChange(currentDiffViewModel, +1);
        }

        private void MoveToChange(DiffViewModel currentDiffViewModel, int indexModifier)
        {
            var diffViewModelIndex = DiffViewModels.IndexOf(currentDiffViewModel) + indexModifier;
            var diffViewModel  = DiffViewModels[diffViewModelIndex];
            var diffLine = _textView.TextSnapshot.GetLineFromLineNumber(diffViewModel.LineNumber);
            currentDiffViewModel.ShowPopup = false;

            _textView.VisualElement.Focus();
            _textView.Caret.MoveTo(diffLine.Start);
            _textView.Caret.EnsureVisible();
        }

        private void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        private void HandleParseComplete(object sender, ParseResultEventArgs e)
        {
            _margin.VisualElement.Dispatcher.BeginInvoke((Action) (() =>
                                                                       {
                                                                           DiffViewModels.Clear();

                                                                           var diffResult = e as DiffParseResultEventArgs;
                                                                           if (diffResult == null) return;

                                                                           foreach (var diffViewModel in diffResult.Diff.Select(hunkRangeInfo => new DiffViewModel(_margin, hunkRangeInfo, _textView)))
                                                                           {
                                                                               DiffViewModels.Add(diffViewModel);
                                                                           }
                                                                       }));
        }
    }
}