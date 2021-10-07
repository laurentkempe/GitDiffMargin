using System;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Core;
using GitDiffMargin.Git;

namespace GitDiffMargin.ViewModel
{
    internal abstract class DiffViewModel : ViewModelBase
    {
        private double _height;
        private double _top;
        protected readonly HunkRangeInfo HunkRangeInfo;
        protected readonly IMarginCore MarginCore;
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private bool _isVisible;

        protected DiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            HunkRangeInfo = hunkRangeInfo;
            MarginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            MarginCore.BrushesChanged += HandleBrushesChanged;
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(nameof(Height));
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                RaisePropertyChanged(nameof(Top));
            }
        }

        public virtual double Width
        {
            get
            {
                return MarginCore.EditorChangeWidth;
            }
        }

        public Thickness Margin
        {
            get
            {
                return new Thickness(MarginCore.EditorChangeLeft, 0, 0, 0);
            }
        }

        public bool IsDeletion { get { return HunkRangeInfo.IsDeletion;} }

        public Brush DiffBrush
        {
            get
            {
                if (HunkRangeInfo.IsAddition)
                {
                    return MarginCore.AdditionBrush;
                }
                return HunkRangeInfo.IsModification ? MarginCore.ModificationBrush : MarginCore.RemovedBrush;
            }
        }

        public int LineNumber { get { return HunkRangeInfo.NewHunkRange.StartingLineNumber; } }

        public int NumberOfLines { get { return HunkRangeInfo.NewHunkRange.NumberOfLines; } }

        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));}
        }

        public double ScaleFactor => MarginCore.ScaleFactor;

        public void RefreshPosition()
        {
            UpdateDimensions();
        }

        public bool IsLineNumberBetweenDiff(int lineNumber)
        {
            var diffStartLine = LineNumber;
            var diffEndLine = diffStartLine + NumberOfLines - 1;

            if (IsDeletion)
            {
                diffEndLine = diffStartLine + NumberOfLines + 2;
            }

            return lineNumber >= diffStartLine && lineNumber <= diffEndLine;
        }

        public override void Cleanup()
        {
            MarginCore.BrushesChanged -= HandleBrushesChanged;

            base.Cleanup();
        }

        private void HandleBrushesChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(DiffBrush));
        }

        protected virtual void UpdateDimensions()
        {
            _updateDiffDimensions(this, HunkRangeInfo);
        }
    }
}