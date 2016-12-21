#region using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace GitDiffMargin.Git
{
    public class HunkRangeInfo
    {
        private readonly List<string> _diffLines;
        private readonly Lazy<bool> _isWhiteSpaceChange;

        public HunkRangeInfo(HunkRange originaleHunkRange, HunkRange newHunkRange, IEnumerable<string> diffLines,
            bool suppressRollback = false)
        {
            OriginalHunkRange = originaleHunkRange;
            NewHunkRange = newHunkRange;
            _diffLines = diffLines.ToList();
            SuppressRollback = suppressRollback;

            IsAddition = _diffLines.All(s => s.StartsWith("+") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsDeletion = _diffLines.All(s => s.StartsWith("-") || s.StartsWith("\\") || string.IsNullOrWhiteSpace(s));
            IsModification = !IsAddition && !IsDeletion;

            if (IsDeletion || IsModification)
            {
                OriginalText = _diffLines.Where(s => s.StartsWith("-"))
                    .Select(s => s.Remove(0, 1).TrimEnd('\n').TrimEnd('\r'))
                    .ToList();
            }
            else
            {
                OriginalText = new List<string>();
            }

            _isWhiteSpaceChange = new Lazy<bool>(IsWhiteSpaceChangeFunc);
        }

        public HunkRange OriginalHunkRange { get; private set; }
        public HunkRange NewHunkRange { get; private set; }
        public List<string> OriginalText { get; private set; }
        public bool SuppressRollback { get; private set; }

        public bool IsAddition { get; }
        public bool IsModification { get; }
        public bool IsDeletion { get; }
        public bool IsWhiteSpaceChange => _isWhiteSpaceChange.Value;

        private bool IsWhiteSpaceChangeFunc()
        {
            var oldText = TrimAll(string.Join(string.Empty, OriginalText));

            var newText = TrimAll(string.Join(string.Empty, _diffLines.Where(s => s.StartsWith("+"))
                    .Select(s => s.Remove(0, 1))
                    .ToList()));

            return oldText == newText;
        }

        private static string TrimAll(string value)
        {
            return new string(value.Where(t => !char.IsWhiteSpace(t)).ToArray());
        }
    }
}