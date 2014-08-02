using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace GitDiffMargin
{
    internal abstract class DiffMarginBase : Canvas, IWpfTextViewMargin
    {
        protected readonly ITextView TextView;
        private readonly IMarginCore _marginCore;
        private bool _isDisposed;
        protected EditorDiffMarginViewModel ViewModel;
        protected UserControl UserControl;

        protected abstract string MarginName { get; }

        protected DiffMarginBase(ITextView textView, IMarginCore marginCore)
        {
            TextView = textView;
            _marginCore = marginCore;
            TextView.Options.OptionChanged += HandleOptionChanged;
            TextView.LayoutChanged += OnLayoutChanged;
        }

        public Brush AdditionBrush
        {

            get
            {
                return _marginCore.AdditionBrush;
            }
        }

        public Brush ModificationBrush
        {
            get
            {
                return _marginCore.ModificationBrush;
            }
        }

        public Brush RemovedBrush
        {
            get
            {
                return _marginCore.RemovedBrush;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
                return UserControl.ActualWidth;
            }
        }

        public bool Enabled
        {
            get
            {
                return TextView.Options.IsSelectionMarginEnabled();
            }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return UserControl;
            }
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
            UserControl.Visibility = Enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            ViewModel.RefreshDiffViewModelPositions();
        }
    }
}