using System;
using System.Collections.ObjectModel;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Core
{
    public class DiffParseResultEventArgs : ParseResultEventArgs
    {
        private readonly ReadOnlyCollection<HunkRangeInfo> _diffToIndex;
        private readonly ReadOnlyCollection<HunkRangeInfo> _diffToHead;

        public DiffParseResultEventArgs(ITextSnapshot snapshot, TimeSpan elapsedTime, ReadOnlyCollection<HunkRangeInfo> diffToIndex, ReadOnlyCollection<HunkRangeInfo> diffToHead)
            : base(snapshot, elapsedTime)
        {
            _diffToIndex = diffToIndex;
            _diffToHead = diffToHead;
        }

        public ReadOnlyCollection<HunkRangeInfo> DiffToIndex
        {
            get
            {
                return _diffToIndex;
            }
        }

        public ReadOnlyCollection<HunkRangeInfo> DiffToHead
        {
            get
            {
                return _diffToHead;
            }
        }
    }
}