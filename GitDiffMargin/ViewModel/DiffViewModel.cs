#region using

using GalaSoft.MvvmLight;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly HunkRangeInfo _hunkRangeInfo;
        private readonly IWpfTextView _textView;
        private int _lineCount;
        private double _windowHeight;

        public DiffViewModel(HunkRangeInfo hunkRangeInfo, IWpfTextView textView)
        {
            _hunkRangeInfo = hunkRangeInfo;
            _textView = textView;

            _lineCount = _textView.TextSnapshot.LineCount;
            _windowHeight = textView.ViewportHeight;

            var lineHeight = _textView.LineHeight;
            Height = _hunkRangeInfo.NewHunkRange.NumberOfLines*lineHeight;
        }

        public double Height { get; set; }
    }
}