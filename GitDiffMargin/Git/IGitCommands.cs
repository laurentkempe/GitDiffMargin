#region using

using System.Collections.Generic;

#endregion

namespace GitDiffMargin.Git
{
    public interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename);
        void StartExternalDiff(string filename);
    }
}