#region using

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    public interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename);
        void StartExternalDiff(string filename);
        bool IsGitRepository(string directory);
    }
}