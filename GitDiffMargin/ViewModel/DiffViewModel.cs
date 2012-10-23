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

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly HunkRangeInfo _hunkRangeInfo;
        private readonly double _lineCount;
        private readonly IWpfTextView _textView;
        private readonly double _windowHeight;
        private ICommand _copyOldTextCommand;
        private bool _isDiffTextVisible;
        private ICommand _rollbackCommand;
        private ICommand _showPopUpCommand;
        private bool _showPopup;

        public DiffViewModel(HunkRangeInfo hunkRangeInfo, IWpfTextView textView)
        {
            _hunkRangeInfo = hunkRangeInfo;
            _textView = textView;

            var lineHeight = _textView.LineHeight;

            _windowHeight = textView.ViewportHeight;
            _lineCount = _windowHeight/lineHeight;

            if (_hunkRangeInfo.IsDeletion)
            {
                Height = _textView.LineHeight;
            }
            else
            {
                Height = _hunkRangeInfo.NewHunkRange.NumberOfLines * lineHeight;
            }

            var ratio = (double) _hunkRangeInfo.NewHunkRange.StartingLineNumber/(double) _lineCount;
            Top = Math.Ceiling(ratio*_windowHeight);

            var bc = new BrushConverter();
            DiffBrush = _hunkRangeInfo.IsModification ? (Brush)bc.ConvertFrom("#0DC0FF") : (Brush)bc.ConvertFrom("#0DCE0E");

            ShowPopup = false;

            if (_hunkRangeInfo.OriginalText != null && _hunkRangeInfo.OriginalText.Any())
            {
                DiffText = _hunkRangeInfo.IsModification || _hunkRangeInfo.IsDeletion ? String.Join(Environment.NewLine, _hunkRangeInfo.OriginalText) : string.Empty;
            }

            var startLineNumber = (int)_hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = (int)(_hunkRangeInfo.NewHunkRange.StartingLineNumber + _hunkRangeInfo.NewHunkRange.NumberOfLines);

            _isVisible = _textView.TextViewLines.SelectMany(line => line.Snapshot.Lines).All(line => line.LineNumber <= endLineNumber && line.LineNumber >= startLineNumber);

            if (_hunkRangeInfo.IsDeletion || _hunkRangeInfo.IsModification)
            {
                _isDiffTextVisible = true;
            }

            //if ((int)_hunkRangeInfo.NewHunkRange.StartingLineNumber < _textView.TextViewLines.Count)
            //{
            //    var wpfTextViewLine = _textView.TextViewLines[(int)_hunkRangeInfo.NewHunkRange.StartingLineNumber];

            //    _isDiffTextVisible = !string.IsNullOrWhiteSpace(DiffText) && wpfTextViewLine.VisibilityState == VisibilityState.FullyVisible;                
            //}
            //else
            //{
            //    _isDiffTextVisible = false;
            //}

            //_isVisible = lines.All(line =>  line.VisibilityState == VisibilityState.FullyVisible);
        }

        public double Height { get; set; }

        public double Top { get; set; }

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

        public string Coordinates
        {
            get
            {
                return string.Format("Top:{0}, Height:{1}, New number of Lines: {2}, StartingLineNumber: {3}", Top, Height,
                                     _hunkRangeInfo.NewHunkRange.NumberOfLines, _hunkRangeInfo.NewHunkRange.StartingLineNumber);
            }
        }

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