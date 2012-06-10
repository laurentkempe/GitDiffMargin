#region using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        private readonly bool _isAddition;
        private readonly bool _isModification;

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines)
        {
            OriginaleHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
            _isAddition = DiffLines.All(s => s.StartsWith("+"));
            _isModification = DiffLines.Any(s => s.StartsWith("-")) && !_isAddition;

            OriginalText = string.Join("\n", DiffLines.Where(s => s.StartsWith("-")).Select(s => s.Remove(0, 2)));
        }

        public HunkRange OriginaleHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> DiffLines { get; private set; }
        public string OriginalText { get; private set; }

        public bool IsAddition
        {
            get { return _isAddition; }
        }

        public bool IsModification
        {
            get { return _isModification; }
        }
    }
}