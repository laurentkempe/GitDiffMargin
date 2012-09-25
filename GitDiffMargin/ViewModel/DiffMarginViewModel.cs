#region using

using System;
using System.Collections.ObjectModel;
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
            _textView.Caret.PositionChanged += OnPositionChanged;
            
            // Delay the initial check until the view gets focus
            _textView.GotAggregateFocus += GotAggregateFocus;
            
            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
            if (_document != null)
            {
                _document.FileActionOccurred += FileActionOccurred;

                ActivityLog.LogInformation("GitDiffMargin", "Created DiffMarginViewModel for: " + _document.FilePath);
            }
        }

        private void OnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            int i = 0;
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            //called when collapsing code
            if (AnyTextChanges(e.OldViewState.EditSnapshot.Version, e.NewViewState.EditSnapshot.Version))
            {
                //todo Update the diff usig the file which is not saved
                //UpdateDiffViewModels();
            }
        }


        private static bool AnyTextChanges(ITextVersion oldVersion, ITextVersion currentVersion)
        {
            while (oldVersion != currentVersion)
            {
                if (oldVersion.Changes.Count > 0)
                    return true;
                oldVersion = oldVersion.Next;
            }

            return false;
        }
        private void GotAggregateFocus(object sender, EventArgs e)
        {
            _textView.GotAggregateFocus -= GotAggregateFocus;

            UpdateDiffViewModels();
        }

        private void TextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            //todo this is correctly called but the file is not saved and then nothing new is shown
            UpdateDiffViewModels();
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; set; }

        private void UpdateDiffViewModels()
        {
            ActivityLog.LogInformation("GitDiffMargin", "UpdateDiffViewModels: " + _document.FilePath);

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
                _textView.GotAggregateFocus -= GotAggregateFocus;
                if (_textView.TextBuffer != null)
                {
                    _textView.TextBuffer.Changed -= TextBufferChanged;
                }
            }
        }
    }
}