using System;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Git;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitDiffMargin
{
    internal class MarginCore : IMarginCore
    {
        private readonly IWpfTextView _textView;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly IEditorFormatMapService _editorFormatMapService;

        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;

        private Brush _additionBrush;
        private Brush _modificationBrush;
        private Brush _removedBrush;

        public MarginCore(IWpfTextView textView, ITextDocumentFactoryService textDocumentFactoryService, IClassificationFormatMapService classificationFormatMapService, SVsServiceProvider serviceProvider, IEditorFormatMapService editorFormatMapService)
        {
            _textView = textView;
            _textDocumentFactoryService = textDocumentFactoryService;
            _editorFormatMapService = editorFormatMapService;

            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap(textView);

            _editorFormatMap = _editorFormatMapService.GetEditorFormatMap(textView);
            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;

            _textView.Closed += (sender, e) => _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;

            GitCommands = new GitCommands(serviceProvider);

            UpdateBrushes();
        }

        public GitCommands GitCommands { get; private set; }

        public ITextDocumentFactoryService TextDocumentFactoryService
        {
            get { return _textDocumentFactoryService; }
        }

        public IEditorFormatMapService EditorFormatMapService
        {
            get { return _editorFormatMapService; }
        }

        public FontFamily FontFamily
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return new FontFamily("Consolas");

                return _classificationFormatMap.DefaultTextProperties.Typeface.FontFamily;
            }
        }

        public FontStretch FontStretch
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontStretches.Normal;

                return _classificationFormatMap.DefaultTextProperties.Typeface.Stretch;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontStyles.Normal;

                return _classificationFormatMap.DefaultTextProperties.Typeface.Style;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.TypefaceEmpty)
                    return FontWeights.Normal;

                return _classificationFormatMap.DefaultTextProperties.Typeface.Weight;
            }
        }

        public double FontSize
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.FontRenderingEmSizeEmpty)
                    return 12.0;

                return _classificationFormatMap.DefaultTextProperties.FontRenderingEmSize;
            }
        }

        public Brush Background
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.BackgroundBrushEmpty)
                    return _textView.Background;

                return _classificationFormatMap.DefaultTextProperties.BackgroundBrush;
            }
        }

        public Brush Foreground
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.ForegroundBrushEmpty)
                    return (Brush)Application.Current.Resources[VsBrushes.ToolWindowTextKey];

                return _classificationFormatMap.DefaultTextProperties.ForegroundBrush;
            }
        }

        public Brush AdditionBrush
        {

            get
            {
                return _additionBrush ?? Brushes.Transparent;
            }
        }

        public Brush ModificationBrush
        {
            get
            {
                return _modificationBrush ?? Brushes.Transparent;
            }
        }

        public Brush RemovedBrush
        {
            get
            {
                return _removedBrush ?? Brushes.Transparent;
            }
        }

        public double EditorChangeLeft
        {
            get { return 2.5; }
        }

        public double EditorChangeWidth
        {
            get { return 5.0; }
        }
        
        public event EventHandler BrushesChanged;

        public void MoveToChange(int lineNumber)
        {
            var diffLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);

            _textView.VisualElement.Focus();
            _textView.Caret.MoveTo(diffLine.Start);
            _textView.Caret.EnsureVisible();
        }

        public void CheckBeginInvokeOnUI(Action action)
        {
            if (_textView.VisualElement.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _textView.VisualElement.Dispatcher.BeginInvoke(action);
            } 
        }

        private void HandleFormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            if (e.ChangedItems.Contains(DiffFormatNames.Addition)
                || e.ChangedItems.Contains(DiffFormatNames.Modification)
                || e.ChangedItems.Contains(DiffFormatNames.Removed))
            {
                UpdateBrushes();
            }
        }

        private void UpdateBrushes()
        {
            _additionBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Addition));
            _modificationBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Modification));
            _removedBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Removed));
            OnBrushesChanged(EventArgs.Empty);
        }

        private void OnBrushesChanged(EventArgs e)
        {
            var t = BrushesChanged;
            if (t != null)
                t(this, e);
        }

        private static Brush GetBrush(ResourceDictionary properties)
        {
            if (properties == null)
                return Brushes.Transparent;

            if (properties.Contains(EditorFormatDefinition.BackgroundColorId))
            {
                var color = (Color)properties[EditorFormatDefinition.BackgroundColorId];
                var brush = new SolidColorBrush(color);
                if (brush.CanFreeze)
                {
                    brush.Freeze();
                }
                return brush;
            }
            if (properties.Contains(EditorFormatDefinition.BackgroundBrushId))
            {
                var brush = (Brush)properties[EditorFormatDefinition.BackgroundBrushId];
                if (brush.CanFreeze)
                {
                    brush.Freeze();
                }
                return brush;
            }

            return Brushes.Transparent;
        }

        public void UpdateEditorDimensions(EditorDiffViewModel editorDiffViewModel, HunkRangeInfo hunkRangeInfo)
        {
            if (_textView.IsClosed)
                return;

            var snapshot = _textView.TextBuffer.CurrentSnapshot;

            var startLineNumber = hunkRangeInfo.NewHunkRange.StartingLineNumber;
            var endLineNumber = startLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1;
            if (startLineNumber < 0
                || startLineNumber >= snapshot.LineCount
                || endLineNumber < 0
                || endLineNumber >= snapshot.LineCount)
            {
                editorDiffViewModel.IsVisible = false;
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
                    editorDiffViewModel.IsVisible = false;
                    return;
                }
            }
            else
            {
                var span = new SnapshotSpan(startLine.Start, endLine.End);
                if (!_textView.TextViewLines.FormattedSpan.IntersectsWith(span))
                {
                    editorDiffViewModel.IsVisible = false;
                    return;
                }
            }

            var startLineView = _textView.GetTextViewLineContainingBufferPosition(startLine.Start);
            var endLineView = _textView.GetTextViewLineContainingBufferPosition(endLine.Start);

            if (startLineView == null || endLineView == null)
            {
                editorDiffViewModel.IsVisible = false;
                return;
            }

            if (_textView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < startLineView.Start
                || _textView.TextViewLines.FirstVisibleLine.Start > endLineView.EndIncludingLineBreak)
            {
                editorDiffViewModel.IsVisible = false;
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
                    editorDiffViewModel.IsVisible = false;
                    return;
            }

            if (startTop >= _textView.ViewportHeight + _textView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                editorDiffViewModel.IsVisible = false;
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
                    editorDiffViewModel.IsVisible = false;
                    return;
            }

            if (stopBottom <= -_textView.LineHeight)
            {
                // shouldn't be reachable, but definitely hide if this is the case
                editorDiffViewModel.IsVisible = false;
                return;
            }

            if (stopBottom <= startTop)
            {
                if (hunkRangeInfo.IsDeletion)
                {
                    double center = (startTop + stopBottom)/2.0;
                    editorDiffViewModel.Top = (center - (_textView.LineHeight/2.0)) + _textView.LineHeight;
                    editorDiffViewModel.Height = _textView.LineHeight;
                    editorDiffViewModel.IsVisible = true;
                }
                else
                {
                    // could be reachable if translation changes an addition to empty
                    editorDiffViewModel.IsVisible = false;
                }

                return;
            }

            editorDiffViewModel.Top = startTop;
            editorDiffViewModel.Height = stopBottom - startTop;
            editorDiffViewModel.IsVisible = true;
        }

        public bool RollBack(HunkRangeInfo hunkRangeInfo)
        {
            var snapshot = _textView.TextSnapshot;

            if (snapshot != snapshot.TextBuffer.CurrentSnapshot)
                return false;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                Span newSpan;
                if (hunkRangeInfo.IsDeletion)
                {
                    var startLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber + 1);
                    newSpan = new Span(startLine.Start.Position, 0);
                }
                else
                {
                    var startLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber);
                    var endLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    newSpan = Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position);
                }

                if (hunkRangeInfo.IsAddition)
                {
                    var startLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber);
                    var endLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    edit.Delete(Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position));
                }
                else
                {
                    var lineBreak = snapshot.GetLineFromLineNumber(0).GetLineBreakText();
                    if (String.IsNullOrEmpty(lineBreak))
                        lineBreak = Environment.NewLine;

                    var originalText = String.Join(lineBreak, hunkRangeInfo.OriginalText);
                    if (hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines != snapshot.LineCount)
                        originalText += lineBreak;

                    edit.Replace(newSpan, originalText);
                }

                edit.Apply();

                return true;
            }
        }

        public ITextDocument GetTextDocument()
        {
            ITextDocument document;
            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
            return document;
        }
    }
}