#region using

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    public interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename, ITextSnapshot snapshot);
        void StartExternalDiff(string filename);
        bool IsGitRepository(string directory);
    }
}