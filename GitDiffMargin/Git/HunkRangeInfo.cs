#region using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        private List<string> DiffLines { get; set; }

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines)
        {
            OriginalHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
            
            IsAddition = DiffLines.All(s => s.StartsWith("+") || string.IsNullOrWhiteSpace(s));
            IsDeletion = DiffLines.All(s => s.StartsWith("-") || string.IsNullOrWhiteSpace(s));
            IsModification = !IsAddition && !IsDeletion;

            if (IsDeletion || IsModification)
            {
                OriginalText = DiffLines.Where(s => s.StartsWith("-")).Select(s => s.Remove(0, 1).TrimEnd('\n').TrimEnd('\r')).ToList();                
            }
        }

        public HunkRange OriginalHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> OriginalText { get; private set; }

        public bool IsAddition { get; private set; }
        public bool IsModification { get; private set; }
        public bool IsDeletion { get; private set; }
    }
}