using System.Collections.Generic;
using GitDiffMargin.Git;

namespace GitDiffMargin.Core
{
    public class DiffParseResultEventArgs
    {
        private readonly List<HunkRangeInfo> _diff;

        public DiffParseResultEventArgs(List<HunkRangeInfo> diff)
        {
            _diff = diff;
        }

        public IEnumerable<HunkRangeInfo> Diff => _diff;
    }
}