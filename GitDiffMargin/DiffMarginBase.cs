using System;
using System.Windows;
using System.Windows.Controls;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace GitDiffMargin
{
    internal abstract class DiffMarginBase : Canvas, IWpfTextViewMargin
    {
        protected readonly ITextView TextView;
        private bool _isDisposed;
        protected UserControl UserControl;
        protected DiffMarginViewModelBase ViewModel;

        protected DiffMarginBase(ITextView textView)
        {
            TextView = textView;

            TextView.Options.OptionChanged += HandleOptionChanged;
            TextView.LayoutChanged += OnLayoutChanged;
        }

        protected abstract string MarginName { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            get { return TextView.Options.IsSelectionMarginEnabled(); }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return UserControl;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;
        }

        private void HandleOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            if (!_isDisposed && e.OptionId == GitDiffMarginTextViewOptions.DiffMarginName)
                UpdateVisibility();
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