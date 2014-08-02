using System.Windows;
using System.Windows.Media;         
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal abstract class DiffViewModel : ViewModelBase
    {
        private double _height;
        private double _top;
        protected HunkRangeInfo HunkRangeInfo;
        private readonly IMarginCore _marginCore;

        protected DiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore)
        {
            HunkRangeInfo = hunkRangeInfo;
            _marginCore = marginCore;
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        public double Width
        {
            get
            {
                return _marginCore.EditorChangeWidth;
            }
        }

        public Thickness Margin
        {
            get
            {
                return new Thickness(_marginCore.EditorChangeLeft, 0, 0, 0);
            }
        }

        public bool IsDeletion { get { return HunkRangeInfo.IsDeletion;} }

        public Brush DiffBrush
        {
            get
            {
                if (HunkRangeInfo.IsAddition)
                {
                    return _marginCore.AdditionBrush;
                }
                return HunkRangeInfo.IsModification ? _marginCore.ModificationBrush : _marginCore.RemovedBrush;
            }
        }

        public int LineNumber { get { return HunkRangeInfo.NewHunkRange.StartingLineNumber; } }

        public void RefreshPosition()
        {
            UpdateDimensions();
        }
        
        protected abstract void UpdateDimensions();
    }
}