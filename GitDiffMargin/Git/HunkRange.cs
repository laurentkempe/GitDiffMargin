namespace GitDiffMargin.Git
{
    public class HunkRange
    {
        public HunkRange(string hunkRange, int contextLines)
        {
            if (hunkRange.Contains(","))
            {
                var hunkParts = hunkRange.Split(',');
                StartingLineNumber = int.Parse(hunkParts[0]) - 1 + contextLines;
                NumberOfLines = int.Parse(hunkParts[1]) - 2 * contextLines;
            }
            else
            {
                StartingLineNumber = int.Parse(hunkRange) - 1 + contextLines;
                NumberOfLines = 1;
            }
        }

        public int StartingLineNumber { get; }
        public int NumberOfLines { get; }
    }
}