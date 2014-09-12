using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    [Export(typeof(IGitCommands))]
    public class GitCommands : IGitCommands
    {
        [ImportingConstructor]
        public GitCommands()
        {
        }

        private const int ContextLines = 0;

        public DiffResult GetGitDiffFor(ITextDocument textDocument, ITextSnapshot snapshot)
        {
            var filename = textDocument.FilePath;
            var repositoryPath = GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                return DiffResult.Empty;

            using (var repo = new Repository(repositoryPath))
            {
                var workingDirectory = repo.Info.WorkingDirectory;
                if (workingDirectory == null)
                    return DiffResult.Empty;

                var retrieveStatus = repo.Index.RetrieveStatus(filename);
                if (retrieveStatus == FileStatus.Nonexistent)
                {
                    // this occurs if a file within the repository itself (not the working copy) is opened.
                    return DiffResult.Empty;
                }

                if ((retrieveStatus & FileStatus.Ignored) != 0)
                {
                    // pointless to show diffs for ignored files
                    return DiffResult.Empty;
                }

                if (retrieveStatus == FileStatus.Unaltered && !textDocument.IsDirty)
                {
                    // truly unaltered
                    return DiffResult.Empty;
                }

                var content = GetCompleteContent(textDocument, snapshot);
                if (content == null)
                    return DiffResult.Empty;

                using (var currentContent = new MemoryStream(content))
                {
                    var relativeFilepath = filename;
                    if (relativeFilepath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                        relativeFilepath = relativeFilepath.Substring(workingDirectory.Length);

                    var newBlob = repo.ObjectDatabase.CreateBlob(currentContent, relativeFilepath);

                    bool headSuppressRollback;
                    bool indexSuppressRollback;
                    Blob headBlob;
                    Blob indexBlob;

                    if ((retrieveStatus & FileStatus.Untracked) != 0 || (retrieveStatus & FileStatus.Added) != 0)
                    {
                        headSuppressRollback = true;

                        // special handling for added files (would need updating to compare against index)
                        using (var emptyContent = new MemoryStream())
                        {
                            headBlob = repo.ObjectDatabase.CreateBlob(emptyContent, relativeFilepath);
                        }
                    }
                    else
                    {
                        headSuppressRollback = false;

                        Commit from = repo.Head.Tip;
                        TreeEntry fromEntry = from[relativeFilepath];
                        if (fromEntry == null)
                        {
                            // try again using case-insensitive comparison
                            Tree tree = from.Tree;
                            foreach (string segment in relativeFilepath.Split(Path.DirectorySeparatorChar))
                            {
                                if (tree == null)
                                    return DiffResult.Empty;

                                fromEntry = tree.FirstOrDefault(i => string.Equals(segment, i.Name, StringComparison.OrdinalIgnoreCase));
                                if (fromEntry == null)
                                    return DiffResult.Empty;

                                tree = fromEntry.Target as Tree;
                            }
                        }

                        headBlob = fromEntry.Target as Blob;
                        if (headBlob == null)
                            return DiffResult.Empty;
                    }

                    if ((retrieveStatus & FileStatus.Untracked) != 0)
                    {
                        indexSuppressRollback = true;
                        indexBlob = headBlob;
                    }
                    else
                    {
                        indexSuppressRollback = false;

                        // the index matches the head unless a specific IndexEntry exists
                        indexBlob = headBlob;
                        foreach (var indexEntry in repo.Index)
                        {
                            if (string.Equals(indexEntry.Path, relativeFilepath, StringComparison.OrdinalIgnoreCase))
                            {
                                indexBlob = repo.Lookup<Blob>(indexEntry.Id);
                                break;
                            }
                        }
                    }

                    ContentChanges treeChanges = repo.Diff.Compare(headBlob, newBlob, new CompareOptions { ContextLines = ContextLines, InterhunkLines = 0 });
                    var gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines, headSuppressRollback);
                    var diffToHead = gitDiffParser.Parse();

                    treeChanges = repo.Diff.Compare(indexBlob, newBlob, new CompareOptions { ContextLines = ContextLines, InterhunkLines = 0 });
                    gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines, indexSuppressRollback);
                    var diffToIndex = gitDiffParser.Parse();

                    return new DiffResult(diffToIndex, diffToHead);
                }
            }
        }

        private static byte[] GetCompleteContent(ITextDocument textDocument, ITextSnapshot snapshot)
        {
            var currentText = snapshot.GetText();

            var content = textDocument.Encoding.GetBytes(currentText);

            var preamble = textDocument.Encoding.GetPreamble();
            if (preamble.Length == 0) return content;

            var completeContent = new byte[preamble.Length + content.Length];
            Buffer.BlockCopy(preamble, 0, completeContent, 0, preamble.Length);
            Buffer.BlockCopy(content, 0, completeContent, preamble.Length, content.Length);

            return completeContent;
        }

        // http://msdn.microsoft.com/en-us/library/17w5ykft.aspx
        private const string UnquotedParameterPattern = @"[^ \t""]+";
        private const string QuotedParameterPattern = @"""(?:[^\\""]|\\[\\""]|\\[^\\""])*""";

        // Two alternatives:
        //   Unquoted (Quoted Unquoted)* Quoted?
        //   Quoted (Unquoted Quoted)* Unquoted?
        private const string ParameterPattern =
            "^(?:" +
            "(?:" + UnquotedParameterPattern + "(?:" + QuotedParameterPattern + UnquotedParameterPattern + ")*" + "(?:" + QuotedParameterPattern + ")?" + ")" +
            "|" + "(?:" + QuotedParameterPattern + "(?:" + UnquotedParameterPattern + QuotedParameterPattern + ")*" + "(?:" + UnquotedParameterPattern + ")?" + ")" +
            ")";

        public void StartExternalDiff(ITextDocument textDocument)
        {
            if (textDocument == null || string.IsNullOrEmpty(textDocument.FilePath)) return;

            var filename = textDocument.FilePath;

            var repositoryPath = GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                return;

            using (var repo = new Repository(repositoryPath))
            {
                var diffGuiTool = repo.Config.Get<string>("diff.guitool");
                if (diffGuiTool == null)
                {
                    diffGuiTool = repo.Config.Get<string>("diff.tool");
                    if (diffGuiTool == null)
                        return;
                }

                var diffCmd = repo.Config.Get<string>("difftool." + diffGuiTool.Value + ".cmd");
                if (diffCmd == null || diffCmd.Value == null)
                    return;

                string workingDirectory = repo.Info.WorkingDirectory;
                string relativePath = Path.GetFullPath(filename);
                if (relativePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(workingDirectory.Length);

                var indexEntry = repo.Index[relativePath];
                var blob = repo.Lookup<Blob>(indexEntry.Id);

                var tempFileName = Path.GetTempFileName();
                File.WriteAllText(tempFileName, blob.GetContentText(new FilteringOptions(relativePath)));
                File.SetAttributes(tempFileName, File.GetAttributes(tempFileName) | FileAttributes.ReadOnly);

                string remoteFile;
                if (textDocument.IsDirty)
                {
                    remoteFile = Path.GetTempFileName();
                    File.WriteAllBytes(remoteFile, GetCompleteContent(textDocument, textDocument.TextBuffer.CurrentSnapshot));
                    File.SetAttributes(remoteFile, File.GetAttributes(remoteFile) | FileAttributes.ReadOnly);
                }
                else
                {
                    remoteFile = filename;
                }

                var cmd = diffCmd.Value.Replace("$LOCAL", tempFileName).Replace("$REMOTE", remoteFile);

                string fileName = Regex.Match(cmd, ParameterPattern).Value;
                string arguments = cmd.Substring(fileName.Length);
                ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments);
                Process.Start(startInfo);
            }
        }

        /// <inheritdoc/>
        public bool IsGitRepository(string path)
        {
            return GetGitRepository(path) != null;
        }

        /// <inheritdoc/>
        public string GetGitRepository(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
                return null;

            var discoveredPath = Repository.Discover(Path.GetFullPath(path));
            // https://github.com/libgit2/libgit2sharp/issues/818#issuecomment-54760613
            return discoveredPath;
        }

        /// <inheritdoc/>
        public string GetGitWorkingCopy(string path)
        {
            var repositoryPath = GetGitRepository(path);
            if (repositoryPath == null)
                return null;

            using (Repository repository = new Repository(repositoryPath))
            {
                string workingDirectory = repository.Info.WorkingDirectory;
                if (workingDirectory == null)
                    return null;

                return Path.GetFullPath(workingDirectory);
            }
        }
    }
}