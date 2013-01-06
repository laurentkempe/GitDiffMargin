#region using

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Linq;
using Microsoft.VisualStudio.Text.Formatting;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly HunkRangeInfo _hunkRangeInfo;
        private readonly IWpfTextView _textView;
        private double _lineCount;
        private double _windowHeight;
        private ICommand _copyOldTextCommand;
        private bool _isDiffTextVisible;
        private ICommand _rollbackCommand;
        private ICommand _showPopUpCommand;
        private bool _showPopup;

        public DiffViewModel(HunkRangeInfo hunkRangeInfo, IWpfTextView textView)
        {
            _hunkRangeInfo = hunkRangeInfo;
            _textView = textView;

            ShowPopup = false;

            SetDisplayProperties();

            DiffBrush = GetDiffBrush();
            
            DiffText = GetDiffText();

            IsDiffTextVisible = GetIsDiffTextVisible();
        }

        private void SetDisplayProperties()
        {
            Height = GetHeight();
            Top = GetTopCoordinate();

            IsVisible = GetIsVisible();

            Coordinates = string.Format("Top:{0}, Height:{1}, New number of Lines: {2}, StartingLineNumber: {3}", Top, Height, _hunkRangeInfo.NewHunkRange.NumberOfLines, _hunkRangeInfo.NewHunkRange.StartingLineNumber);
        }

        private bool GetIsDiffTextVisible()
        {
            return _hunkRangeInfo.IsDeletion || _hunkRangeInfo.IsModification;
        }

        private string GetDiffText()
        {
            if (_hunkRangeInfo.OriginalText != null && _hunkRangeInfo.OriginalText.Any())
            {
                return _hunkRangeInfo.IsModification || _hunkRangeInfo.IsDeletion ? String.Join(Environment.NewLine, _hunkRangeInfo.OriginalText) : string.Empty;
            }

            return string.Empty;
        }

        private bool GetIsVisible()
        {
            var hunkStartLineNumber = (int) _hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var hunkEndLineNumber = (int) (_hunkRangeInfo.NewHunkRange.StartingLineNumber + _hunkRangeInfo.NewHunkRange.NumberOfLines);

            var hunkStartline = _textView.TextSnapshot.GetLineFromLineNumber(hunkStartLineNumber);
            var hunkEndline = _textView.TextSnapshot.GetLineFromLineNumber(hunkEndLineNumber);
            if (hunkStartline != null && hunkEndline != null)
            {
                var topLine = _textView.GetTextViewLineContainingBufferPosition(hunkStartline.Start);
                var bottomLine = _textView.GetTextViewLineContainingBufferPosition(hunkEndline.End);

                if (topLine.VisibilityState == VisibilityState.FullyVisible && bottomLine.VisibilityState == VisibilityState.FullyVisible)
                {
                    return true;
                }

                if (topLine.VisibilityState != VisibilityState.FullyVisible && bottomLine.VisibilityState == VisibilityState.FullyVisible)
                {
                    Top = 0;

                    var numberofHiddenLines = _textView.ViewportTop/_textView.LineHeight;

                    var hiddenHunkLines = numberofHiddenLines - hunkStartLineNumber;

                    Height = Height - Math.Ceiling(hiddenHunkLines * _textView.LineHeight);

                    return true;
                }

                if (topLine.VisibilityState == VisibilityState.FullyVisible && bottomLine.VisibilityState != VisibilityState.FullyVisible)
                {
                    Height = _textView.ViewportBottom - (Top + 1);

                    return true;
                }

            }

            return false;
        }

        private Brush GetDiffBrush()
        {
            var bc = new BrushConverter();
            var diffBrush = _hunkRangeInfo.IsModification ? (Brush) bc.ConvertFrom("#0DC0FF") : (Brush) bc.ConvertFrom("#0DCE0E");
            return diffBrush;
        }

        private double GetHeight()
        {
            var lineHeight = _textView.LineHeight;

            _windowHeight = _textView.ViewportHeight;
            _lineCount = _windowHeight/lineHeight;

            if (_hunkRangeInfo.IsDeletion)
            {
                return _textView.LineHeight;
            }
            
            return _hunkRangeInfo.NewHunkRange.NumberOfLines*lineHeight;
        }

        private double GetTopCoordinate()
        {
            var ratio = _hunkRangeInfo.NewHunkRange.StartingLineNumber/_lineCount;
            return Math.Ceiling((ratio * _windowHeight) - _textView.ViewportTop);
        }

        public void RefreshPosition()
        {
            SetDisplayProperties();
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        public Brush DiffBrush { get; private set; }

        public bool IsDeletion { get { return _hunkRangeInfo.IsDeletion;} }

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

        public string Coordinates { get; private set; }

        public string DiffText { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value;
            RaisePropertyChanged(() => IsVisible);}
        }

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

        private ICommand _showDifferenceCommand;

        private bool _isVisible;
        private double _height;
        private double _top;

        public ICommand ShowDifferenceCommand
        {
            get { return _showDifferenceCommand ?? (_showDifferenceCommand = new RelayCommand(ShowDifference, ShowDifferenceCanExecute)); }
        }

        private bool ShowDifferenceCanExecute()
        {
            return _hunkRangeInfo.IsModification;
        }

        private void ShowDifference()
        {
            ITextDocument document;
            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);

            if (document == null || string.IsNullOrEmpty(document.FilePath)) return;
            
            var gitCommands = new GitCommands();
            gitCommands.StartExternalDiff(document.FilePath);
        }

        public ICommand CopyOldTextCommand
        {
            get { return _copyOldTextCommand ?? (_copyOldTextCommand = new RelayCommand(CopyOldText, CopyOldTextCanExecute)); }
        }

        public ICommand RollbackCommand
        {
            get { return _rollbackCommand ?? (_rollbackCommand = new RelayCommand(Rollback, RollbackCanExecute)); }
        }

        private bool CopyOldTextCanExecute()
        {
            return _hunkRangeInfo.IsModification || _hunkRangeInfo.IsDeletion;
        }

        private void CopyOldText()
        {
            Clipboard.SetText(DiffText);
        }

        private bool RollbackCanExecute()
        {
            return _hunkRangeInfo.IsModification || _hunkRangeInfo.IsDeletion;
        }

        private void Rollback()
        {
            var snapshot = _textView.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return;

            using (ITextEdit edit = snapshot.TextBuffer.CreateEdit())
            {
                if (_hunkRangeInfo.NewHunkRange.NumberOfLines == 1)
                {
                    var line = snapshot.GetLineFromLineNumber((int) _hunkRangeInfo.NewHunkRange.StartingLineNumber);

                    var text = line.ExtentIncludingLineBreak.GetText();

                    if(text.EndsWith("\r\n"))
                    {
                        edit.Replace(line.ExtentIncludingLineBreak, _hunkRangeInfo.OriginalText[0] + "\r\n");
                    }
                    else
                    {
                        edit.Replace(line.ExtentIncludingLineBreak, _hunkRangeInfo.OriginalText[0]);
                    }
                }
                else
                {
                    for (var n = 0; n <= _hunkRangeInfo.NewHunkRange.NumberOfLines; n++)
                    {
                        var line = snapshot.GetLineFromLineNumber((int)_hunkRangeInfo.NewHunkRange.StartingLineNumber + n);
                        edit.Delete(line.Start.Position, line.Length);
                    }

                    var startLine = snapshot.GetLineFromLineNumber((int) _hunkRangeInfo.NewHunkRange.StartingLineNumber);

                    foreach (var line in _hunkRangeInfo.OriginalText)
                    {
                        edit.Insert(startLine.Start.Position, line + "\r\n");                        
                    }
                }

                edit.Apply();
            }
        }

        private void ShowPopUp()
        {
            ShowPopup = true;
        }
    }
}