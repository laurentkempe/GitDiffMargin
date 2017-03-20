using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading; 
using GitDiffMargin.ViewModel;

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

        private static void DoubleClick(object sender, EventArgs e)
        {
            ClickWaitTimer.Stop();

            var button = ClickWaitTimer.Tag as Button;

            var editorDiffMarginViewModel = button?.Tag as EditorDiffMarginViewModel;
            if (editorDiffMarginViewModel == null) return;

            var editorDiffViewModel = button.DataContext as EditorDiffViewModel;
            editorDiffViewModel?.ShowPopUpCommand.Execute(editorDiffMarginViewModel);
        }
    }
}