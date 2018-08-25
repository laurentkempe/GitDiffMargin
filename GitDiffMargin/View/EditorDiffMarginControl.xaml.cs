using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitDiffMargin.View
{
    /// <summary>
    ///   Interaction logic for EditorDiffMarginControl.xaml
    /// </summary>
    public partial class EditorDiffMarginControl
    {
        private static readonly DispatcherTimer ClickWaitTimer =
            new DispatcherTimer(
                new TimeSpan(0, 0, 0, 0, 100),
                DispatcherPriority.Background,
                DoubleClick,
                Dispatcher.CurrentDispatcher);

        public EditorDiffMarginControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClickWaitTimer.Tag = sender as Button;

            ClickWaitTimer.Start();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClickWaitTimer.Stop();

            e.Handled = true;

            var button = sender as Button;
            var editorDiffViewModel = (EditorDiffViewModel)button?.DataContext;

            editorDiffViewModel?.ShowDifferenceCommand.Execute(null);
        }

        private void Popup_Close(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is Popup popup)) return;

            popup.IsOpen = false;

            var editorDiffViewModel = (EditorDiffMarginViewModel) DataContext;
            editorDiffViewModel.FocusTextView();
        }

        private static void DoubleClick(object sender, EventArgs e)
        {
            ClickWaitTimer.Stop();

            var button = ClickWaitTimer.Tag as Button;

            var editorDiffMarginViewModel = button?.Tag as EditorDiffMarginViewModel;
            if (editorDiffMarginViewModel == null) return;

            var editorDiffViewModel = button.DataContext as EditorDiffViewModel;
            editorDiffViewModel?.ShowPopUpCommand.Execute(editorDiffMarginViewModel);
        }

        public static CustomPopupPlacementCallback CustomPopupPlacementCallback => CustomPopupPlacementCallbackImpl;

        private static CustomPopupPlacement[] CustomPopupPlacementCallbackImpl(Size popupSize, Size targetSize, Point offset)
        {
            var verticalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Vertical);
            var horizontalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Horizontal);
            return new[] {verticalPlacement, horizontalPlacement};
        }

        private void Popup_OnOpened(object sender, EventArgs e)
        {
            var popup = sender as Popup;
            if (popup == null) return;

            var contentControl = (ContentControl)popup.FindName("Commands");
            if (contentControl == null) return;

            contentControl.Focus();
        }
    }
}