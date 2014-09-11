using System;
using System.Collections.Generic;
using System.Linq;

namespace GitDiffMargin.Git
{
    public class GitDiffParser
    {
        private readonly string _gitDiff;
        private readonly int _contextLines;
        private readonly bool _suppressRollback;

        public GitDiffParser(string gitDiff, int contextLines)
            : this(gitDiff, contextLines, false)
        {
        }

        public GitDiffParser(string gitDiff, int contextLines, bool suppressRollback)
        {
            _gitDiff = gitDiff;
            _contextLines = contextLines;
            _suppressRollback = suppressRollback;
        }

        public IEnumerable<HunkRangeInfo> Parse()
        {
            return from hunkLine in GetUnifiedFormatHunkLines() 
                   where !string.IsNullOrEmpty(hunkLine.Item1)
                   select new HunkRangeInfo(new HunkRange(GetHunkOriginalFile(hunkLine.Item1), _contextLines), new HunkRange(GetHunkNewFile(hunkLine.Item1), _contextLines), hunkLine.Item2, _suppressRollback);
        }

        public IEnumerable<Tuple<string, IEnumerable<string>>> GetUnifiedFormatHunkLines()
        {
            var split = _gitDiff.Split('\n');
            //var split = _gitDiff.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            var withoutHeader = split.SkipWhile(s => !s.StartsWith("@@")).ToList();

            var splitHunks = SplitHunks(withoutHeader).ToList();

            return splitHunks.Any() ? 
                splitHunks.Select(splitHunk => new Tuple<string, IEnumerable<string>>(splitHunk[0], splitHunk.Skip(1).TakeWhile((s, i) => i < splitHunk.Count))) : 
                Enumerable.Empty<Tuple<string, IEnumerable<string>>>();
        }

        private static IEnumerable<List<string>> SplitHunks(List<string> lines)
        {
            if (!lines.Any()) yield break;

            var firstHunk = true;
            var hunks = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("@@"))
                {
                    if (firstHunk)
                    {
                        hunks.Add(line.Trim());
                        firstHunk = false;
                    }
                    else
                    {
                        yield return new List<string>(hunks);
                        hunks.Clear();
                        hunks.Add(line.Trim());
                    }
                }
                else
                {
                    hunks.Add(line);
                }
            }

            yield return new List<string>(hunks);
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