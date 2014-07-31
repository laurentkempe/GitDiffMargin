#region using

using GitDiffMargin.View;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin
{
    public sealed class EditorDiffMargin : DiffMarginBase
    {
        private const double MarginWidth = 10.0;

        public const string MarginNameConst = "EditorDiffMargin";

        protected override string MarginName
        {
            get { return MarginNameConst; }
        }

        public override double ChangeLeft
        {
            get { return 2.5; }
        }

        public override double ChangeWidth
        {
            get { return 5.0; }
        }

        internal EditorDiffMargin(IWpfTextView textView, EditorMarginFactory factory)
            : base (textView, factory)
        {
            _classificationFormatMap = factory.ClassificationFormatMapService.GetClassificationFormatMap(textView);

            _userControl = new EditorDiffMarginControl();
            _viewModel = new DiffMarginViewModel(this, textView, factory);
            _userControl.DataContext = _viewModel;
            _userControl.Width = MarginWidth;
        }
    }
}