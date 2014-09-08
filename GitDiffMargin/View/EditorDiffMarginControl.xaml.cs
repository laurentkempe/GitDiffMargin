namespace GitDiffMargin.View
{
    using System.Windows;
    using System.Windows.Controls.Primitives;

    /// <summary>
    ///   Interaction logic for EditorDiffMarginControl.xaml
    /// </summary>
    public partial class EditorDiffMarginControl
    {
        public EditorDiffMarginControl()
        {
            InitializeComponent();
        }

        private CustomPopupPlacement[] HandleCustomPopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            // TODO: Change vertical placement to be relative to mouse position IFF the mouse is vertically within the
            // target diff glyph.
            CustomPopupPlacement verticalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Vertical);
            CustomPopupPlacement horizontalPlacement = new CustomPopupPlacement(offset, PopupPrimaryAxis.Horizontal);
            return new[] { verticalPlacement, horizontalPlacement };
        }
    }
}