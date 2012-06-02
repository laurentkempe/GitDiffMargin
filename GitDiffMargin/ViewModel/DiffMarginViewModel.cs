#region using

using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;
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

            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
            if (_document != null)
            {
                _document.FileActionOccurred += FileActionOccurred;
            }
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; set; }

        private void UpdateDiffViewModels()
        {
            var rangeInfos = _gitCommands.GetGitDiffFor(_document.FilePath);

            DiffViewModels.Clear();

            foreach (var hunkRangeInfo in rangeInfos)
            {
                var diffViewModel = new DiffViewModel(hunkRangeInfo, _textView);
                DiffViewModels.Add(diffViewModel);
            }
        }

        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentLoadedFromDisk) != 0 ||
                (e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                UpdateDiffViewModels();
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
            }
        }
    }
}