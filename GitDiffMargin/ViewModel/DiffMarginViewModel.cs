#region using

using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffMarginViewModel : ViewModelBase
    {
        private readonly IWpfTextView _textView;
        private readonly IGitCommands _gitCommands;
        private ITextDocument _document;

        public DiffMarginViewModel(IWpfTextView textView, IGitCommands gitCommands)
        {
            _textView = textView;
            _gitCommands = gitCommands;
            DiffViewModels = new ObservableCollection<DiffViewModel>();

            _textView.Closed += TextViewClosed;
            _textView.TextBuffer.Changed += TextBufferChanged;

            _textView.LayoutChanged += OnLayoutChanged;
            _textView.ViewportHeightChanged += OnViewportHeightChanged;

            // Delay the initial check until the view gets focus
            _textView.GotAggregateFocus += GotAggregateFocus;
            
            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
            if (_document != null)
            {
                _document.FileActionOccurred += FileActionOccurred;
            }
        }

        private void OnViewportHeightChanged(object sender, EventArgs e)
        {
            RefreshDiffViewModelPositions();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            RefreshDiffViewModelPositions();
        }

        private void GotAggregateFocus(object sender, EventArgs e)
        {
            _textView.GotAggregateFocus -= GotAggregateFocus;

            CreateDiffViewModels();
        }

        private void TextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // this is correctly called but the file is not saved and then nothing new is shown
            //todo Modify to work on a copy of the file
            RefreshDiffViewModelPositions();
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; set; }

        private void RefreshDiffViewModelPositions()
        {
            ActivityLog.LogInformation("GitDiffMargin", "RefreshDiffViewModelPositions: " + _document.FilePath);

            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        private void CreateDiffViewModels()
        {
            ActivityLog.LogInformation("GitDiffMargin", "CreateDiffViewModels: " + _document.FilePath);

            var rangeInfos = _gitCommands.GetGitDiffFor(_document.FilePath);

            DiffViewModels.Clear();

            foreach (var diffViewModel in rangeInfos.Select(hunkRangeInfo => new DiffViewModel(hunkRangeInfo, _textView)))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }

        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentLoadedFromDisk) != 0 ||
                (e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                CreateDiffViewModels();
            }
        }

        private void TextViewClosed(object sender, EventArgs e)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            if (_document != null)
            {
                _document.FileActionOccurred -= FileActionOccurred;
                _document = null;
            }

            if (_textView != null)
            {
                _textView.Closed -= TextViewClosed;
                _textView.GotAggregateFocus -= GotAggregateFocus;
                if (_textView.TextBuffer != null)
                {
                    _textView.TextBuffer.Changed -= TextBufferChanged;
                }
            }
        }
    }
}