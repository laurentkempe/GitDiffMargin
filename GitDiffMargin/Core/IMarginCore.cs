using System;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitDiffMargin.Core
{
    internal interface IMarginCore
    {
        IWpfTextView TextView { get; }
        string OriginalPath { get; }
        IGitCommands GitCommands { get; }
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
        event EventHandler BrushesChanged;

        event EventHandler<HunksChangedEventArgs> HunksChanged;
        void MoveToChange(int lineNumber);
        bool RollBack(HunkRangeInfo hunkRangeInfo);
        ITextDocument GetTextDocument();
        void FocusTextView();
    }
}