using System;
using System.Windows;
using System.Windows.Media;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace GitDiffMargin
{
    internal interface IMarginCore
    {
        GitCommands GitCommands { get; }
        ITextDocumentFactoryService TextDocumentFactoryService { get; }
        IEditorFormatMapService EditorFormatMapService { get; }
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

        event EventHandler BrushesChanged;
    }
}