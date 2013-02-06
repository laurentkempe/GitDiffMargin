#region using

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitDiffMargin.Git;
using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

#endregion

namespace GitDiffMargin
{
    public class GitDiffMargin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "GitDiffMargin";

        internal const double ChangeLeft = 2.5;
        internal const double ChangeWidth = 5.0;
        private const double MarginWidth = 10.0;

        private readonly IWpfTextView _textView;
        private readonly IEditorFormatMap _editorFormatMap;
        private readonly DiffMarginControl _gitDiffBarControl;
        private bool _isDisposed;

        private Brush _additionBrush;
        private Brush _modificationBrush;
        private Brush _removedBrush;

        public GitDiffMargin(IWpfTextView textView, ITextDocumentFactoryService textDocumentFactoryService, IEditorFormatMapService editorFormatMapService)
        {
            _textView = textView;
            _editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);

            _editorFormatMap.FormatMappingChanged += HandleFormatMappingChanged;
            _textView.Closed += (sender, e) => _editorFormatMap.FormatMappingChanged -= HandleFormatMappingChanged;
            UpdateBrushes();

            HandleOptionChanged(null, null);
            _textView.Options.OptionChanged += HandleOptionChanged;

            _gitDiffBarControl = new DiffMarginControl();
            _gitDiffBarControl.DataContext = new DiffMarginViewModel(this, _textView, textDocumentFactoryService, new GitCommands());
            _gitDiffBarControl.Width = MarginWidth;
        }

        public System.Windows.FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return _gitDiffBarControl;
            }
        }

        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return _gitDiffBarControl.ActualWidth;
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
            _isDisposed = true;
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
        }

        private void UpdateBrushes()
        {
            _additionBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Addition));
            _modificationBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Modification));
            _removedBrush = GetBrush(_editorFormatMap.GetProperties(DiffFormatNames.Removed));
        }

        private static Brush GetBrush(ResourceDictionary properties)
        {
            if (properties == null)
                return Brushes.Transparent;

            if (properties.Contains(EditorFormatDefinition.BackgroundColorId))
            {
                Color color = (Color)properties[EditorFormatDefinition.BackgroundColorId];
                Brush brush = new SolidColorBrush(color);
                if (brush.CanFreeze)
                    brush.Freeze();

                return brush;
            }
            else if (properties.Contains(EditorFormatDefinition.BackgroundBrushId))
            {
                Brush brush = (Brush)properties[EditorFormatDefinition.BackgroundBrushId];
                if (brush.CanFreeze)
                    brush.Freeze();

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