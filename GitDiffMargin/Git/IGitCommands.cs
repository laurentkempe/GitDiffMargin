#region using

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    internal interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, ITextSnapshot snapshot);

        void StartExternalDiff(ITextDocument textDocument);

        /// <summary>
        /// Determines if a file or directory is located within a Git repository.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="path"/> is a valid path of a file or directory located within a
        /// Git repository; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsGitRepository(string path);

        string GetGitRepository(string filePath);
    }
}