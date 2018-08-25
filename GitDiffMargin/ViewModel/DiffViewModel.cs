using System;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal abstract class DiffViewModel : ViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        protected readonly HunkRangeInfo HunkRangeInfo;
        protected readonly IMarginCore MarginCore;
        private double _height;
        private bool _isVisible;
        private double _top;

        protected DiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore,
            Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            HunkRangeInfo = hunkRangeInfo;
            MarginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            MarginCore.BrushesChanged += HandleBrushesChanged;
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged(() => Top);
            }
        }

        public virtual double Width => MarginCore.EditorChangeWidth;

        public Thickness Margin => new Thickness(MarginCore.EditorChangeLeft, 0, 0, 0);

        public bool IsDeletion => HunkRangeInfo.IsDeletion;

        public Brush DiffBrush
        {
            get
            {
                if (HunkRangeInfo.IsAddition) return MarginCore.AdditionBrush;
                return HunkRangeInfo.IsModification ? MarginCore.ModificationBrush : MarginCore.RemovedBrush;
            }
        }

        public int LineNumber => HunkRangeInfo.NewHunkRange.StartingLineNumber;

        public virtual bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        public void RefreshPosition()
        {
            UpdateDimensions();
        }

        public bool IsLineNumberBetweenDiff(int lineNumber)
        {
            var numberOfLines = HunkRangeInfo.NewHunkRange.NumberOfLines;

            var diffStartLine = LineNumber;
            var diffEndLine = diffStartLine + numberOfLines - 1;

            if (IsDeletion)
            {
                diffEndLine = diffStartLine + numberOfLines + 2;
            }

            return lineNumber >= diffStartLine && lineNumber <= diffEndLine;
        }

        private void HandleBrushesChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => DiffBrush);
        }

        protected virtual void UpdateDimensions()
        {
            _updateDiffDimensions(this, HunkRangeInfo);
        }
    }
}