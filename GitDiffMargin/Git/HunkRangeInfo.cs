#region using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines,
            bool suppressRollback = false)
        {
            OriginalHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
            SuppressRollback = suppressRollback;

            IsAddition = DiffLines.All(s => s.StartsWith("+") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsDeletion = DiffLines.All(s => s.StartsWith("-") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsModification = !IsAddition && !IsDeletion;

            if (IsDeletion || IsModification)
                OriginalText = DiffLines.Where(s => s.StartsWith("-"))
                    .Select(s => s.Remove(0, 1).TrimEnd('\n').TrimEnd('\r')).ToList();
        }

        private List<string> DiffLines { get; }

        public HunkRange OriginalHunkRange { get; }
        public HunkRange NewHunkRange { get; }
        public List<string> OriginalText { get; }
        public bool SuppressRollback { get; }

        public bool IsAddition { get; }
        public bool IsModification { get; }
        public bool IsDeletion { get; }
    }
}