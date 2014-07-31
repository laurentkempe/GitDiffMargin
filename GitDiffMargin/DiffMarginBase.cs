using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitDiffMargin.Git;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace GitDiffMargin
{
    public abstract class DiffMarginBase : Canvas, IWpfTextViewMargin
    {
        private readonly ITextView _textView;
        private bool _isDisposed;
        protected DiffMarginViewModel _viewModel;
        protected UserControl _userControl;
        private IEditorFormatMap _editorFormatMap;

        private Brush _additionBrush;
        private Brush _modificationBrush;
        private Brush _removedBrush;
        protected IClassificationFormatMap _classificationFormatMap;

        protected abstract string MarginName { get; }

        protected DiffMarginBase(ITextView textView, EditorMarginFactory factory)
        {
            _textView = textView;
            _textView.Options.OptionChanged += HandleOptionChanged;
            _textView.LayoutChanged += OnLayoutChanged;

            _editorFormatMap = factory.EditorFormatMapService.GetEditorFormatMap(textView);
            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;
            _textView.Closed += (sender, e) => _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
            UpdateBrushes();
        }

        public event EventHandler BrushesChanged;

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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _viewModel.Cleanup();
            _isDisposed = true;
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return _userControl.ActualWidth;
            }
        }

        public bool Enabled
        {
            get
            {
                return _textView.Options.IsSelectionMarginEnabled();
            }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return _userControl;
            }
        }

        public IClassificationFormatMap ClassificationFormatMap
        {
            get
            {
                return _classificationFormatMap;
            }
        }

        public abstract double ChangeLeft { get; }

        public abstract double ChangeWidth { get; }

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

        private void OnBrushesChanged(EventArgs e)
        {
            var t = BrushesChanged;
            if (t != null)
                t(this, e);
        }

        private void HandleOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            if (!_isDisposed && e.OptionId == GitDiffMarginTextViewOptions.DiffMarginName)
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            ThrowIfDisposed();
            _userControl.Visibility = Enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            _viewModel.RefreshDiffViewModelPositions();
        }
    }
}