#region Copyright

// 
// 
// Copyright 2007 - 2012 Innoveo Solutions AG, Zurich/Switzerland 
// All rights reserved. Use is subject to license terms.
// 
// 

#endregion

#region using

using System;
using System.Linq;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffViewModel : ViewModelBase
    {
        private readonly HunkRangeInfo _hunkRangeInfo;
        private readonly double _lineCount;
        private readonly IWpfTextView _textView;
        private double _windowHeight;

        public DiffViewModel(HunkRangeInfo hunkRangeInfo, IWpfTextView textView)
        {
            _hunkRangeInfo = hunkRangeInfo;
            _textView = textView;

            var lineHeight = _textView.LineHeight;

            _windowHeight = textView.ViewportHeight;
            //_lineCount = _textView.TextSnapshot.LineCount;
            _lineCount = _windowHeight / lineHeight;

            Height = _hunkRangeInfo.NewHunkRange.NumberOfLines*lineHeight;

            var ratio = (double) _hunkRangeInfo.NewHunkRange.StartingLineNumber/(double) _lineCount;
            Top = Math.Ceiling(ratio*_windowHeight);

            DiffBrush = _hunkRangeInfo.IsAddition ? Brushes.SeaGreen : Brushes.RoyalBlue;
        }

        public double Height { get; set; }

        public double Top { get; set; }

        public Brush DiffBrush { get; private set; }

        public string Coordinates { get
        {
            return string.Format("Top:{0}, Height:{1}, New number of Lines: {2}, StartingLineNumber: {3}\n{4}", Top, Height,
                                 _hunkRangeInfo.NewHunkRange.NumberOfLines, _hunkRangeInfo.NewHunkRange.StartingLineNumber,
                                 string.Join("\n", _hunkRangeInfo.DiffLines.Select(s => s)));
        } }
    }
}