using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin.Core
{
    internal sealed class MarginCore : IMarginCore, IDisposable
    {
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;

        private readonly DiffUpdateBackgroundParser _parser;

        private Brush _additionBrush;

        private bool _isDisposed;
        private Brush _modificationBrush;
        private Brush _removedBrush;

        public MarginCore(IWpfTextView textView, string originalPath,
            ITextDocumentFactoryService textDocumentFactoryService,
            IClassificationFormatMapService classificationFormatMapService,
            IEditorFormatMapService editorFormatMapService, IGitCommands gitCommands)
        {
            TextView = textView;

            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap(textView);

            _editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;

            GitCommands = gitCommands;

            _parser = new DiffUpdateBackgroundParser(textView.TextBuffer, textView.TextDataModel.DocumentBuffer,
                originalPath, TaskScheduler.Default, textDocumentFactoryService, GitCommands);
            _parser.ParseComplete += HandleParseComplete;
            _parser.RequestParse(false);

            TextView.Closed += (sender, e) =>
            {
                _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
                _parser.ParseComplete -= HandleParseComplete;
            };

            UpdateBrushes();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _parser.Dispose();
        }

        public IWpfTextView TextView { get; }

        public string OriginalPath { get; }

        public IGitCommands GitCommands { get; }

        public FontFamily FontFamily => _classificationFormatMap.DefaultTextProperties.TypefaceEmpty
            ? new FontFamily("Consolas")
            : _classificationFormatMap.DefaultTextProperties.Typeface.FontFamily;

        public FontStretch FontStretch => _classificationFormatMap.DefaultTextProperties.TypefaceEmpty
            ? FontStretches.Normal
            : _classificationFormatMap.DefaultTextProperties.Typeface.Stretch;

        public FontStyle FontStyle => _classificationFormatMap.DefaultTextProperties.TypefaceEmpty
            ? FontStyles.Normal
            : _classificationFormatMap.DefaultTextProperties.Typeface.Style;

        public FontWeight FontWeight => _classificationFormatMap.DefaultTextProperties.TypefaceEmpty
            ? FontWeights.Normal
            : _classificationFormatMap.DefaultTextProperties.Typeface.Weight;

        public double FontSize => _classificationFormatMap.DefaultTextProperties.FontRenderingEmSizeEmpty
            ? 12.0
            : _classificationFormatMap.DefaultTextProperties.FontRenderingEmSize;

        public Brush Background => _classificationFormatMap.DefaultTextProperties.BackgroundBrushEmpty
            ? TextView.Background
            : _classificationFormatMap.DefaultTextProperties.BackgroundBrush;

        public Brush Foreground
        {
            get
            {
                if (_classificationFormatMap.DefaultTextProperties.ForegroundBrushEmpty)
                    return (Brush) Application.Current.Resources[VsBrushes.ToolWindowTextKey];

                return _classificationFormatMap.DefaultTextProperties.ForegroundBrush;
            }
        }

        public Brush AdditionBrush => _additionBrush ?? Brushes.Transparent;

        public Brush ModificationBrush => _modificationBrush ?? Brushes.Transparent;

        public Brush RemovedBrush => _removedBrush ?? Brushes.Transparent;

        public double EditorChangeLeft => 2.5;

        public double EditorChangeWidth => 5.0;

        public double ScrollChangeWidth => 3.0;

        public event EventHandler BrushesChanged;

        public event EventHandler<HunksChangedEventArgs> HunksChanged;

        public void MoveToChange(int lineNumber)
        {
            var diffLine = TextView.TextSnapshot.GetLineFromLineNumber(lineNumber);

            TextView.VisualElement.Focus();
            TextView.Caret.MoveTo(diffLine.Start);
            TextView.ViewScroller.EnsureSpanVisible(diffLine.ExtentIncludingLineBreak,
                EnsureSpanVisibleOptions.AlwaysCenter);
        }

        public bool RollBack(HunkRangeInfo hunkRangeInfo)
        {
            if (hunkRangeInfo.SuppressRollback)
                return false;

            var snapshot = TextView.TextSnapshot;

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
                    var endLine = snapshot.GetLineFromLineNumber(
                        hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    newSpan = Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position);
                }

                if (hunkRangeInfo.IsAddition)
                {
                    var startLine = snapshot.GetLineFromLineNumber(hunkRangeInfo.NewHunkRange.StartingLineNumber);
                    var endLine = snapshot.GetLineFromLineNumber(
                        hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines - 1);
                    edit.Delete(Span.FromBounds(startLine.Start.Position, endLine.EndIncludingLineBreak.Position));
                }
                else
                {
                    var lineBreak = snapshot.GetLineFromLineNumber(0).GetLineBreakText();
                    if (string.IsNullOrEmpty(lineBreak))
                        lineBreak = Environment.NewLine;

                    var originalText = string.Join(lineBreak, hunkRangeInfo.OriginalText);
                    if (hunkRangeInfo.NewHunkRange.StartingLineNumber + hunkRangeInfo.NewHunkRange.NumberOfLines !=
                        snapshot.LineCount)
                        originalText += lineBreak;

                    edit.Replace(newSpan, originalText);
                }

                edit.Apply();

                return true;
            }
        }

        public ITextDocument GetTextDocument()
        {
            TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument),
                out ITextDocument document);
            return document;
        }

        public void FocusTextView()
        {
            TextView.VisualElement.Focus();
        }

        private void CheckBeginInvokeOnUi(Action action)
        {
            if (TextView.VisualElement.Dispatcher.CheckAccess())
                action();
            else
                TextView.VisualElement.Dispatcher.BeginInvoke(action);
        }

        private void HandleFormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            if (e.ChangedItems.Contains(DiffFormatNames.Addition)
                || e.ChangedItems.Contains(DiffFormatNames.Modification)
                || e.ChangedItems.Contains(DiffFormatNames.Removed))
                UpdateBrushes();
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
            BrushesChanged?.Invoke(this, e);
        }

        private static Brush GetBrush(ResourceDictionary properties)
        {
            if (properties == null)
                return Brushes.Transparent;

            if (properties.Contains(EditorFormatDefinition.BackgroundColorId))
            {
                var color = (Color) properties[EditorFormatDefinition.BackgroundColorId];
                var brush = new SolidColorBrush(color);
                if (brush.CanFreeze) brush.Freeze();
                return brush;
            }

            if (properties.Contains(EditorFormatDefinition.BackgroundBrushId))
            {
                var brush = (Brush) properties[EditorFormatDefinition.BackgroundBrushId];
                if (brush.CanFreeze) brush.Freeze();
                return brush;
            }

            return Brushes.Transparent;
        }

        private void HandleParseComplete(object sender, ParseResultEventArgs e)
        {
            if (e is DiffParseResultEventArgs diffResult) CheckBeginInvokeOnUi(() => OnHunksChanged(diffResult.Diff));
        }

        private void OnHunksChanged(IEnumerable<HunkRangeInfo> hunkRangeInfos)
        {
            HunksChanged?.Invoke(this, new HunksChangedEventArgs(hunkRangeInfos));
        }
    }
}