#region using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines)
            : this(originaleHunkRange, newHunkRange, diffLines, false)
        {
        }

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines,
            bool suppressRollback)
        {
            OriginalHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
            SuppressRollback = suppressRollback;

            IsAddition = DiffLines.All(s => s.StartsWith("+") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsDeletion = DiffLines.All(s => s.StartsWith("-") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsModification = !IsAddition && !IsDeletion;

            if (IsDeletion || IsModification)
                OriginalText =
                    DiffLines.Where(s => s.StartsWith("-"))
                        .Select(s => s.Remove(0, 1).TrimEnd('\n').TrimEnd('\r'))
                        .ToList();
        }

        private List<string> DiffLines { get; }

        public HunkRange OriginalHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> OriginalText { get; private set; }
        public bool SuppressRollback { get; private set; }

        public bool IsAddition { get; }
        public bool IsModification { get; }
        public bool IsDeletion { get; }
    }
}