using System;
using System.Collections.Generic;
using GitDiffMargin.Git;

namespace GitDiffMargin.Core
{
    public class HunksChangedEventArgs : EventArgs
    {
        public HunksChangedEventArgs(IEnumerable<HunkRangeInfo> hunks)
        {
            Hunks = hunks;
        }

        public IEnumerable<HunkRangeInfo> Hunks { get; }
    }
}