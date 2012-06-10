#region using

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly HunkRangeInfo _hunkRangeInfo;
        private readonly double _lineCount;
        private readonly IWpfTextView _textView;
        private readonly double _windowHeight;
        private bool _isDiffTextVisible;
        private ICommand _showPopUpCommand;
        private bool _showPopup;

        public DiffViewModel(HunkRangeInfo hunkRangeInfo, IWpfTextView textView)
        {
            _hunkRangeInfo = hunkRangeInfo;
            _textView = textView;

            var lineHeight = _textView.LineHeight;

            _windowHeight = textView.ViewportHeight;
            //_lineCount = _textView.TextSnapshot.LineCount;
            _lineCount = _windowHeight/lineHeight;

            Height = _hunkRangeInfo.NewHunkRange.NumberOfLines*lineHeight;

            var ratio = (double) _hunkRangeInfo.NewHunkRange.StartingLineNumber/(double) _lineCount;
            Top = Math.Ceiling(ratio*_windowHeight);

            var bc = new BrushConverter();

            DiffBrush = _hunkRangeInfo.IsAddition ? (Brush) bc.ConvertFrom("#E0FFE0") : (Brush) bc.ConvertFrom("#E0F0FF");

            ShowPopup = false;

            DiffText = _hunkRangeInfo.IsModification ? _hunkRangeInfo.OriginalText : string.Empty;

            _isDiffTextVisible = !string.IsNullOrWhiteSpace(DiffText);
        }

        public double Height { get; set; }

        public double Top { get; set; }

        public Brush DiffBrush { get; private set; }

        public ICommand ShowPopUpCommand
        {
            get { return _showPopUpCommand ?? (_showPopUpCommand = new RelayCommand(ShowPopUp)); }
        }

        public bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                if (value == _showPopup) return;
                _showPopup = value;
                RaisePropertyChanged(() => ShowPopup);
            }
        }

        public string Coordinates
        {
            get
            {
                return string.Format("Top:{0}, Height:{1}, New number of Lines: {2}, StartingLineNumber: {3}", Top, Height,
                                     _hunkRangeInfo.NewHunkRange.NumberOfLines, _hunkRangeInfo.NewHunkRange.StartingLineNumber);
            }
        }

        public string DiffText { get; private set; }

        public bool IsDiffTextVisible
        {
            get { return _isDiffTextVisible; }
            set
            {
                if (value == _isDiffTextVisible) return;
                _isDiffTextVisible = value;
                RaisePropertyChanged(() => IsDiffTextVisible);
            }
        }

        private ICommand _copyOldTextCommand;

        public ICommand CopyOldTextCommand
        {
            get { return _copyOldTextCommand ?? (_copyOldTextCommand = new RelayCommand(CopyOldText, CopyOldTextCanExecute)); }
        }

        private bool CopyOldTextCanExecute()
        {
            return _hunkRangeInfo.IsModification;
        }

        private void CopyOldText()
        {
            Clipboard.SetText(_hunkRangeInfo.OriginalText);
        }

        private ICommand _rollbackCommand;

        public ICommand RollbackCommand
        {
            get { return _rollbackCommand ?? (_rollbackCommand = new RelayCommand(Rollback, RollbackCanExecute)); }
        }

        private bool RollbackCanExecute()
        {
            return _hunkRangeInfo.IsModification;
        }

        private void Rollback()
        {
            var snapshot = _textView.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                for (int n = 0; n < _hunkRangeInfo.NewHunkRange.NumberOfLines; n++)
                {
                    var line = snapshot.GetLineFromLineNumber((int)_hunkRangeInfo.NewHunkRange.StartingLineNumber + n);
                    edit.Delete(line.Start.Position, line.Length);
                }

                var startLine = snapshot.GetLineFromLineNumber((int) _hunkRangeInfo.NewHunkRange.StartingLineNumber);

                edit.Insert(startLine.Start.Position, _hunkRangeInfo.OriginalText);

                edit.Apply();
            }
        }

        private void ShowPopUp()
        {
            ShowPopup = true;
        }
    }
}