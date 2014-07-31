#region using

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Linq;
using Microsoft.VisualStudio.Text.Formatting;

#endregion

namespace GitDiffMargin.ViewModel
{
    public abstract class DiffViewModel : ViewModelBase
    {
        private double _height;
        private double _top;
        protected HunkRangeInfo HunkRangeInfo;
        protected DiffMarginBase EditorDiffMargin;

        protected DiffViewModel(HunkRangeInfo hunkRangeInfo, DiffMarginBase editorDiffMargin)
        {
            HunkRangeInfo = hunkRangeInfo;
            EditorDiffMargin = editorDiffMargin;
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
            protected set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        public double Width
        {
            get
            {
                return EditorDiffMargin.ChangeWidth;
            }
        }

        public Thickness Margin
        {
            get
            {
                return new Thickness(EditorDiffMargin.ChangeLeft, 0, 0, 0);
            }
        }

        public bool IsDeletion { get { return HunkRangeInfo.IsDeletion;} }

        public Brush DiffBrush
        {
            get
            {
                if (HunkRangeInfo.IsAddition)
                {
                    return EditorDiffMargin.AdditionBrush;
                }
                return HunkRangeInfo.IsModification ? EditorDiffMargin.ModificationBrush : EditorDiffMargin.RemovedBrush;
            }
        }

        public void RefreshPosition()
        {
            UpdateDimensions();
        }
        
        protected abstract void UpdateDimensions();
    }

    public class EditorDiffViewModel : DiffViewModel
    {
        private readonly IWpfTextView _textView;
        private readonly IGitCommands _gitCommands;
        private bool _isDiffTextVisible;
        private bool _showPopup;
        private bool _reverted;
        private ICommand _copyOldTextCommand;
        private ICommand _rollbackCommand;
        private ICommand _showPopUpCommand;

        internal EditorDiffViewModel(EditorDiffMargin editorDiffMargin, HunkRangeInfo hunkRangeInfo, IWpfTextView textView, IGitCommands gitCommands)
            : base(hunkRangeInfo, editorDiffMargin)
        {
            EditorDiffMargin = editorDiffMargin;
            HunkRangeInfo = hunkRangeInfo;
            _textView = textView;
            _gitCommands = gitCommands;
            EditorDiffMargin.BrushesChanged += HandleBrushesChanged;

            ShowPopup = false;

            UpdateDimensions();

            DiffText = GetDiffText();

            IsDiffTextVisible = GetIsDiffTextVisible();
        }

        public int LineNumber { get { return HunkRangeInfo.NewHunkRange.StartingLineNumber; } }

        private void HandleBrushesChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => DiffBrush);
        }

        private bool GetIsDiffTextVisible()
        {
            return HunkRangeInfo.IsDeletion || HunkRangeInfo.IsModification;
        }

        private string GetDiffText()
        {
            if (HunkRangeInfo.OriginalText != null && HunkRangeInfo.OriginalText.Any())
            {
                return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion ? String.Join(Environment.NewLine, HunkRangeInfo.OriginalText) : string.Empty;
            }

            return string.Empty;
        }

        protected override void UpdateDimensions()
        {
            if (_reverted || _textView.IsClosed)
                return;

            var snapshot = _textView.TextBuffer.CurrentSnapshot;

            var startLineNumber = HunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = startLineNumber + HunkRangeInfo.NewHunkRange.NumberOfLines - 1;
            if (startLineNumber < 0
                || startLineNumber >= snapshot.LineCount
                || endLineNumber < 0
                || endLineNumber >= snapshot.LineCount)
            {
                IsVisible = false;
                return;
            }

            var startLine = snapshot.GetLineFromLineNumber(startLineNumber);
            var endLine = snapshot.GetLineFromLineNumber(endLineNumber);

            if (startLine == null || endLine == null) return;


            if (endLine.LineNumber < startLine.LineNumber)
            {
                var span = new SnapshotSpan(endLine.Start, startLine.End);
                if (!_textView.TextViewLines.FormattedSpan.IntersectsWith(span))
                {
                    IsVisible = false;
                    return;
                }
            }
            else
            {
                var span = new SnapshotSpan(startLine.Start, endLine.End);
                if (!_textView.TextViewLines.FormattedSpan.IntersectsWith(span))
                {
                    IsVisible = false;
                    return;
                }
            }

            var startLineView = _textView.GetTextViewLineContainingBufferPosition(startLine.Start);
            var endLineView = _textView.GetTextViewLineContainingBufferPosition(endLine.Start);

            if (startLineView == null || endLineView == null)
            {
                IsVisible = false;
                return;
            }

            if (_textView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < startLineView.Start
                || _textView.TextViewLines.FirstVisibleLine.Start > endLineView.EndIncludingLineBreak)
            {
                IsVisible = false;
                return;
            }

            double startTop;
            switch (startLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                    startTop = startLineView.Top - _textView.ViewportTop;
                    break;

                case VisibilityState.Hidden:
                    startTop = startLineView.Top - _textView.ViewportTop;
                    break;

                case VisibilityState.PartiallyVisible:
                    startTop = startLineView.Top - _textView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was past the end we would have already returned
                    startTop = 0;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    IsVisible = false;
                    return;
            }

            if (startTop >= _textView.ViewportHeight + _textView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                IsVisible = false;
                return;
            }

            double stopBottom;
            switch (endLineView.VisibilityState)
            {
                case VisibilityState.FullyVisible:
                    stopBottom = endLineView.Bottom - _textView.ViewportTop;
                    break;

                case VisibilityState.Hidden:
                    stopBottom = endLineView.Bottom - _textView.ViewportTop;
                    break;

                case VisibilityState.PartiallyVisible:
                    stopBottom = endLineView.Bottom - _textView.ViewportTop;
                    break;

                case VisibilityState.Unattached:
                    // if the closest line was before the start we would have already returned
                    stopBottom = _textView.ViewportHeight;
                    break;

                default:
                    // shouldn't be reachable, but definitely hide if this is the case
                    IsVisible = false;
                    return;
            }

            if (stopBottom <= -_textView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                IsVisible = false;
                return;
            }

            if (stopBottom <= startTop)
            {
                if (HunkRangeInfo.IsDeletion)
                {
                    double center = (startTop + stopBottom) / 2.0;
                    Top = (center - (_textView.LineHeight / 2.0)) + _textView.LineHeight;
                    Height = _textView.LineHeight;
                    IsVisible = true;
                }
                else
                {
                    // could be reachable if translation changes an addition to empty
                    IsVisible = false;
                }

                return;
            }

            Top = startTop;
            Height = stopBottom - startTop;
            IsVisible = true;
        }

        public FontFamily FontFamily
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return new FontFamily("Consolas");

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.Typeface.FontFamily;
            }
        }

        public FontStretch FontStretch
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontStretches.Normal;

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.Typeface.Stretch;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontStyles.Normal;

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.Typeface.Style;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontWeights.Normal;

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.Typeface.Weight;
            }
        }

        public double FontSize
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.FontRenderingEmSizeEmpty)
                    return 12.0;

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.FontRenderingEmSize;
            }
        }

        public double MaxWidth
        {
            get
            {
                return _textView.ViewportWidth;
            }
        }

        public double MaxHeight
        {
            get
            {
                return Math.Max(_textView.ViewportHeight * 2.0 / 3.0, 400);
            }
        }

        public Brush Background
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.BackgroundBrushEmpty)
                    return _textView.Background;

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.BackgroundBrush;
            }
        }

        public Brush Foreground
        {
            get
            {
                if (EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.ForegroundBrushEmpty)
                    return (Brush)Application.Current.Resources[VsBrushes.ToolWindowTextKey];

                return EditorDiffMargin.ClassificationFormatMap.DefaultTextProperties.ForegroundBrush;
            }
        }

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
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void ShowDifference()
        {
            ITextDocument document;
            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);

            _gitCommands.StartExternalDiff(document);
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
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion;
        }

        private void CopyOldText()
        {
            Clipboard.SetText(DiffText);
            ShowPopup = false;
        }

        private bool RollbackCanExecute()
        {
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void Rollback()
        {
            var snapshot = _textView.TextSnapshot;
            
            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                Span newSpan;
                if (HunkRangeInfo.IsDeletion)
                {
                    var startLine = snapshot.GetLineFromLineNumber(HunkRangeInfo.NewHunkRange.StartingLineNumber + 1);
                    newSpan = new Span(startLine.Start.Position, 0);
                }
                else
                {
                    var startLine = snapshot.GetLineFromLineNumber(HunkRangeInfo.NewHunkRange.StartingLineNumber);
                    var endLine = snapshot.GetLineFromLineNumber(HunkRangeInfo.NewHunkRange.StartingLineNumber + HunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    newSpan = Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position);
                }

                if (HunkRangeInfo.IsAddition)
                {
                    var startLine = snapshot.GetLineFromLineNumber(HunkRangeInfo.NewHunkRange.StartingLineNumber);
                    var endLine = snapshot.GetLineFromLineNumber(HunkRangeInfo.NewHunkRange.StartingLineNumber + HunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    edit.Delete(Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position));
                }
                else
                {
                    var lineBreak = snapshot.GetLineFromLineNumber(0).GetLineBreakText();
                    if (string.IsNullOrEmpty(lineBreak))
                        lineBreak = Environment.NewLine;

                    var originalText = string.Join(lineBreak, HunkRangeInfo.OriginalText);
                    if (HunkRangeInfo.NewHunkRange.StartingLineNumber + HunkRangeInfo.NewHunkRange.NumberOfLines != snapshot.LineCount)
                        originalText += lineBreak;

                    edit.Replace(newSpan, originalText);
                }

                edit.Apply();

                // immediately hide the change
                _reverted = true;
                ShowPopup = false;
                IsVisible = false;
            }
        }

        private void ShowPopUp()
        {
            ShowPopup = true;
        }
    }
}