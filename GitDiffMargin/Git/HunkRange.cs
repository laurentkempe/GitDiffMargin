namespace GitDiffMargin.Git
{
    public class HunkRange
    {
        public HunkRange(string hunkRange)
        {
            if (hunkRange.Contains(","))
            {
                var hunkParts = hunkRange.Split(',');
                StartingLineNumber = long.Parse(hunkParts[0]) - 1;
                NumberOfLines = long.Parse(hunkParts[1]);
            }
            else
            {
                StartingLineNumber = long.Parse(hunkRange) - 1;                
            }

        }

        public long StartingLineNumber { get; set; }
        public long NumberOfLines { get; set; }
    }
}