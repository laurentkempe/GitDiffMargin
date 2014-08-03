using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin
{
    internal interface IMarginCore
    {
        event EventHandler BrushesChanged;

        event EventHandler<IEnumerable<HunkRangeInfo>> HunksChanged;

        GitCommands GitCommands { get; }
        FontFamily FontFamily { get; }
        FontStretch FontStretch { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        double FontSize { get; }
        Brush Background { get; }
        Brush Foreground { get; }
        Brush AdditionBrush { get; }
        Brush ModificationBrush { get; }
        Brush RemovedBrush { get; }
        double EditorChangeLeft { get; }
        double EditorChangeWidth { get; }
        double ScrollChangeWidth { get; }
        void MoveToChange(int lineNumber);
        bool RollBack(HunkRangeInfo hunkRangeInfo);
        ITextDocument GetTextDocument();
    }
}