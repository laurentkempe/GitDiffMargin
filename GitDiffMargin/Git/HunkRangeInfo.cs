namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        public HunkRange OriginaleHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange)
        {
            OriginaleHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
        }
    }
}