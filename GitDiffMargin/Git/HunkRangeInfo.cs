#region using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        private readonly bool _isAddition;
        private readonly bool _isModification;
        private List<string> DiffLines { get; set; }

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines)
        {
            OriginaleHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
            _isAddition = DiffLines.All(s => s.StartsWith("+"));
            _isModification = DiffLines.Any(s => s.StartsWith("-")) && !_isAddition;

            OriginalText = DiffLines.Where(s => s.StartsWith("-")).Select(s => s.Remove(0, 1).TrimEnd('\n').TrimEnd('\r')).ToList();
        }

        public HunkRange OriginaleHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> OriginalText { get; private set; }

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