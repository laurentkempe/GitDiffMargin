using System.Collections.Generic;
using System.Linq;

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        public HunkRange OriginaleHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> DiffLines { get; private set; }

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines)
        {
            OriginaleHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            DiffLines = diffLines.ToList();
        }
    }
}