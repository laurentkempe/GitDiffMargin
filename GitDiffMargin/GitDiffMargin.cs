#region using

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitDiffMargin.Git;
using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

#endregion

namespace GitDiffMargin
{
    internal class GitDiffMargin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "GitDiffMargin";

        internal const double ChangeLeft = 2.5;
        internal const double ChangeWidth = 5.0;
        private const double MarginWidth = 10.0;

        private readonly IWpfTextView _textView;
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IEditorFormatMap _editorFormatMap;
        private DiffMarginViewModel _viewModel;
        private readonly EditorDiffMarginControl _editorDiffMarginControl;
        private bool _isDisposed;

        private Brush _additionBrush;
        private Brush _modificationBrush;
        private Brush _removedBrush;

        internal GitDiffMargin(IWpfTextView textView, MarginFactory factory)
        {
            _textView = textView;
            _classificationFormatMap = factory.ClassificationFormatMapService.GetClassificationFormatMap(textView);
            _editorFormatMap = factory.EditorFormatMapService.GetEditorFormatMap(textView);

            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;
            _textView.Closed += (sender, e) => _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
            UpdateBrushes();

            _textView.Options.OptionChanged += HandleOptionChanged;

            _editorDiffMarginControl = new EditorDiffMarginControl();
            _viewModel = new DiffMarginViewModel(this, _textView, factory.TextDocumentFactoryService, new GitCommands(factory.ServiceProvider));
            _editorDiffMarginControl.DataContext = _viewModel;
            _editorDiffMarginControl.Width = MarginWidth;
        }

        public event EventHandler BrushesChanged;

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return _editorDiffMarginControl;
            }
        }

        public IClassificationFormatMap ClassificationFormatMap
        {
            get
            {
                return _classificationFormatMap;
            }
        }

        public IWpfTextView TextView
        {
            get { return _textView; }
        }
        
        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return _editorDiffMarginControl.ActualWidth;
            }
        }

        public bool Enabled
        {
            get
            {
                return _textView.Options.IsSelectionMarginEnabled();
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

        /// <summary>
        ///   Returns an instance of the margin if this is the margin that has been requested.
        /// </summary>
        /// <param name="marginName"> The name of the margin requested </param>
        /// <returns> An instance of GitDiffMargin or null </returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _viewModel.Cleanup();
            _isDisposed = true;
        }

        protected virtual void OnBrushesChanged(EventArgs e)
        {
            var t = BrushesChanged;
            if (t != null)
                t(this, e);
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

        private void HandleOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            if (!_isDisposed && e.OptionId == GitDiffMarginTextViewOptions.DiffMarginName)
                UpdateVisibility();
        }

        private void UpdateBrushes()
        {
            _additionBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Addition));
            _modificationBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Modification));
            _removedBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Removed));
            OnBrushesChanged(EventArgs.Empty);
        }

        private void UpdateVisibility()
        {
            ThrowIfDisposed();
            _editorDiffMarginControl.Visibility = Enabled ? Visibility.Visible : Visibility.Collapsed;
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

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}