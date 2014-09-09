#region using

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    internal interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, ITextSnapshot snapshot);
        void StartExternalDiff(ITextDocument textDocument);
        bool IsGitRepository(string directory);
        string GetGitRepository(string filePath);
    }
}