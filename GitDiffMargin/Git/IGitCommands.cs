#region using

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    internal interface IGitCommands
    {
        IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, string originalPath,
            ITextSnapshot snapshot);

        void StartExternalDiff(ITextDocument textDocument, string originalPath);

        /// <summary>
        ///     Attempts to gets the original file path for the specified path.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         In some (detectable) cases, documents are opened through a temporary copy of the file rather than
        ///         opening the original file directly. To preserve Git diff functionality in these cases, the original path is
        ///         detected and used for diff operations.
        ///     </para>
        /// </remarks>
        /// <param name="path">The path of a document opened in the editor.</param>
        /// <param name="originalPath">
        ///     The original path of the document in a source-controlled working copy; otherwise,
        ///     this is just set to <paramref name="path" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the resulting <paramref name="originalPath" /> was located (even if it
        ///     matches <paramref name="path" />; otherwise, <see langword="false" />.
        /// </returns>
        bool TryGetOriginalPath(string path, out string originalPath);

        /// <summary>
        ///     Determines if a file or directory is located within a Git repository.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <param name="originalPath">The original path of the file in a source-controlled working copy.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="path" /> is a valid path of a file or directory located within a
        ///     Git repository; otherwise, <see langword="false" />.
        /// </returns>
        bool IsGitRepository(string path, string originalPath);

        /// <summary>
        ///     Gets the absolute path to the folder containing the Git repository database for a specified path.
        /// </summary>
        /// <param name="path">The path to the file or directory within the repository.</param>
        /// <param name="originalPath">The original path of the file in a source-controlled working copy.</param>
        /// <returns>
        ///     <para>The absolute path to folder containing the repository database for <paramref name="path" />.</para>
        ///     <para>-or-</para>
        ///     <para>
        ///         <see langword="null" /> if <paramref name="path" /> is not a path to a file or directory within a Git
        ///         repository.
        ///     </para>
        /// </returns>
        string GetGitRepository(string path, string originalPath);

        /// <summary>
        ///     Gets the root of the working copy of the Git repository containing <paramref name="path" />.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <param name="originalPath">The original path of the file in a source-controlled working copy.</param>
        /// <returns>
        ///     <para>
        ///         The absolute path to the root of the working copy of the Git repository containing
        ///         <paramref name="path" />.
        ///     </para>
        ///     <para>-or-</para>
        ///     <para><see langword="null" /> if the Git repository is bare.</para>
        ///     <para>-or-</para>
        ///     <para>
        ///         <see langword="null" /> if <paramref name="path" /> is not a path to a file or directory within the
        ///         working copy of a Git repository.
        ///     </para>
        /// </returns>
        string GetGitWorkingCopy(string path, string originalPath);
    }
}