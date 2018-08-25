using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using GitDiffMargin.ViewModel;

namespace GitDiffMargin.View
{
    /// <summary>
    ///     Interaction logic for EditorDiffMarginControl.xaml
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

        public static CustomPopupPlacementCallback CustomPopupPlacementCallback => CustomPopupPlacementCallbackImpl;

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
            var editorDiffViewModel = (EditorDiffViewModel) button?.DataContext;

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

            if (!(button?.Tag is EditorDiffMarginViewModel editorDiffMarginViewModel)) return;

            var editorDiffViewModel = button.DataContext as EditorDiffViewModel;
            editorDiffViewModel?.ShowPopUpCommand.Execute(editorDiffMarginViewModel);
        }

        private static CustomPopupPlacement[] CustomPopupPlacementCallbackImpl(Size popupSize, Size targetSize,
            Point offset)
        {
            var verticalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Vertical);
            var horizontalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Horizontal);
            return new[] {verticalPlacement, horizontalPlacement};
        }

        private void Popup_OnOpened(object sender, EventArgs e)
        {
            var popup = sender as Popup;
            var contentControl = popup?.FindName("Commands") as ContentControl;

            contentControl?.Focus();
        }
    }
}