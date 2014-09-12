#region using

using Microsoft.VisualStudio.Text;

#endregion

namespace GitDiffMargin.Git
{
    internal interface IGitCommands
    {
        DiffResult GetGitDiffFor(ITextDocument textDocument, ITextSnapshot snapshot);

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

        /// <summary>
        /// Gets the absolute path to the folder containing the Git repository database for a specified path.
        /// </summary>
        /// <param name="path">The path to the file or directory within the repository.</param>
        /// <returns>
        /// <para>The absolute path to folder containing the repository database for <paramref name="path"/>.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if <paramref name="path"/> is not a path to a file or directory within a Git
        /// repository.</para>
        /// </returns>
        string GetGitRepository(string path);

        /// <summary>
        /// Gets the root of the working copy of the Git repository containing <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the file or directory.</param>
        /// <returns>
        /// <para>The absolute path to the root of the working copy of the Git repository containing
        /// <paramref name="path"/>.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if the Git repository is bare.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if <paramref name="path"/> is not a path to a file or directory within the
        /// working copy of a Git repository.</para>
        /// </returns>
        string GetGitWorkingCopy(string path);
    }
}