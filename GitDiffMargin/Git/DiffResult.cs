namespace GitDiffMargin.Git
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class DiffResult
    {
        private static readonly DiffResult _empty = new DiffResult();

        private DiffResult()
            : this(Enumerable.Empty<HunkRangeInfo>(), Enumerable.Empty<HunkRangeInfo>())
        {
        }

        public DiffResult(IEnumerable<HunkRangeInfo> diffToIndex, IEnumerable<HunkRangeInfo> diffToHead)
        {
            DiffToIndex = diffToIndex.ToList().AsReadOnly();
            DiffToHead = diffToHead.ToList().AsReadOnly();
        }

        public static DiffResult Empty
        {
            get
            {
                return _empty;
            }
        }

        public ReadOnlyCollection<HunkRangeInfo> DiffToIndex
        {
            get;
            private set;
        }

        public ReadOnlyCollection<HunkRangeInfo> DiffToHead
        {
            get;
            private set;
        }
    }
}
