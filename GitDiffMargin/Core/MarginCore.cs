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
        private readonly IWpfTextView _textView;

        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;
        private readonly IGitCommands _gitCommands;

        private readonly DiffUpdateBackgroundParser _parser;

        private Brush _additionBrush;
        private Brush _modificationBrush;
        private Brush _removedBrush;

        private bool _isDisposed;

        public MarginCore(IWpfTextView textView, ITextDocumentFactoryService textDocumentFactoryService, IClassificationFormatMapService classificationFormatMapService, IEditorFormatMapService editorFormatMapService, IGitCommands gitCommands)
        {
            _textView = textView;

            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap(textView);

            _editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;

            _gitCommands = gitCommands;

            _parser = new DiffUpdateBackgroundParser(textView.TextBuffer, textView.TextDataModel.DocumentBuffer, TaskScheduler.Default, textDocumentFactoryService, GitCommands);
            _parser.ParseComplete += HandleParseComplete;
            _parser.RequestParse(false);

            _textView.Options.OptionChanged += HandleOptionChanged;

            _textView.Closed += (sender, e) =>
            {
                _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
                _parser.ParseComplete -= HandleParseComplete;
            };

            UpdateBrushes();
        }

        public IWpfTextView TextView
        {
            get
            {
                return _textView;
            }
        }

        public IGitCommands GitCommands
        {
            get
            {
                return _gitCommands;
            }
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

        public double ScrollChangeWidth
        {
            get { return 3.0; }
        }

        public event EventHandler BrushesChanged;

        public event EventHandler<HunksChangedEventArgs> HunksChanged;

        public void MoveToChange(int lineNumber)
        {
            var diffLine = _textView.TextSnapshot.GetLineFromLineNumber(lineNumber);

            _textView.VisualElement.Focus();
            _textView.Caret.MoveTo(diffLine.Start);
            _textView.Caret.EnsureVisible();
        }

        private void CheckBeginInvokeOnUi(Action action)
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

        private void HandleOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            if (_isDisposed)
                return;

            if (string.Equals(e.OptionId, GitDiffMarginTextViewOptions.CompareToIndexId.Name, StringComparison.Ordinal))
            {
                _parser.RequestParse(true);
            }
        }

        public bool RollBack(HunkRangeInfo hunkRangeInfo)
        {
            if (hunkRangeInfo.SuppressRollback)
                return false;

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

        private void HandleParseComplete(object sender, ParseResultEventArgs e)
        {
            var diffResult = e as DiffParseResultEventArgs;
            if (diffResult == null) return;

            CheckBeginInvokeOnUi(
                () =>
                {
                    bool compareToIndex = _textView.Options.GetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId);
                    OnHunksChanged(compareToIndex ? diffResult.DiffToIndex : diffResult.DiffToHead);
                });
        }

        private void OnHunksChanged(IEnumerable<HunkRangeInfo> hunkRangeInfos)
        {
            var t = HunksChanged;
            if (t != null)
                t(this, new HunksChangedEventArgs(hunkRangeInfos));
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _parser.Dispose();
        }
    }
}