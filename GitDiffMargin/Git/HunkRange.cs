namespace GitDiffMargin.Git
{
    public class HunkRange
    {
        public HunkRange(string hunkRange)
        {
            if (hunkRange.Contains(","))
            {
                var hunkParts = hunkRange.Split(',');
                StartingLineNumber = int.Parse(hunkParts[0]) - 1;
                NumberOfLines = int.Parse(hunkParts[1]);
            }
            else
            {
                StartingLineNumber = int.Parse(hunkRange) - 1;
                NumberOfLines = 1;
            }

        }

        public int StartingLineNumber { get; private set; }
        public int NumberOfLines { get; private set; }
    }
}