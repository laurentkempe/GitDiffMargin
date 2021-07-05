using System;
using System.Collections.Generic;
using GitDiffMargin.Git;

namespace GitDiffMargin.Core
{
    public class HunksChangedEventArgs : EventArgs
    {
        private readonly IEnumerable<HunkRangeInfo> _hunks;

        public HunksChangedEventArgs(IEnumerable<HunkRangeInfo> hunks)
        {
            _hunks = hunks;
        }

        public IEnumerable<HunkRangeInfo> Hunks
        {
            get
            {
                return _hunks;
            }
        }
    }
}
