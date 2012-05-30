using System;
using System.Collections.Generic;
using System.Linq;

namespace GitDiffMargin.Git
{
    public class GitDiffParser
    {
        private readonly string _gitDiff;

        public GitDiffParser(string gitDiff)
        {
            _gitDiff = gitDiff;
        }

        public IEnumerable<HunkRangeInfo> Parse()
        {
            return
                GetUnifiedFormatHunkLines().Select(
                    hunkLine => new HunkRangeInfo(new HunkRange(GetHunkOriginalFile(hunkLine)), new HunkRange(GetHunkNewFile(hunkLine))));
        }

        public IEnumerable<string> GetUnifiedFormatHunkLines()
        {
            var split = _gitDiff.Split('\n');
            //var split = _gitDiff.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            return
                split.Where(line => line.StartsWith("@@"))
                     .Select(line => line.Trim());
        }

        public string GetHunkOriginalFile(string hunkLine)
        {
            return hunkLine.Split(new[] {"@@ -", " +"}, StringSplitOptions.RemoveEmptyEntries).First();
        }

        public string GetHunkNewFile(string hunkLine)
        {
            return hunkLine.Split(new[] { "@@ -", " +" }, StringSplitOptions.RemoveEmptyEntries).ToArray()[1].Split(' ')[0];
        }
    }
}