using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using __VSDIFFSERVICEOPTIONS = Microsoft.VisualStudio.Shell.Interop.__VSDIFFSERVICEOPTIONS;
using IVsDifferenceService = Microsoft.VisualStudio.Shell.Interop.IVsDifferenceService;
using SVsDifferenceService = Microsoft.VisualStudio.Shell.Interop.SVsDifferenceService;

namespace GitDiffMargin.Git
{
    [Export(typeof(IGitCommands))]
    public class GitCommands : IGitCommands
    {
        private readonly SVsServiceProvider _serviceProvider;

        [ImportingConstructor]
        public GitCommands(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private const int ContextLines = 0;

        public IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, ITextSnapshot snapshot)
        {
            var filename = textDocument.FilePath;
            var repositoryPath = GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                yield break;

            using (var repo = new Repository(repositoryPath))
            {
                var workingDirectory = repo.Info.WorkingDirectory;
                if (workingDirectory == null)
                    yield break;

                var retrieveStatus = repo.RetrieveStatus(filename);
                if (retrieveStatus == FileStatus.Nonexistent)
                {
                    // this occurs if a file within the repository itself (not the working copy) is opened.
                    yield break;
                }

                if ((retrieveStatus & FileStatus.Ignored) != 0)
                {
                    // pointless to show diffs for ignored files
                    yield break;
                }

                if (retrieveStatus == FileStatus.Unaltered && !textDocument.IsDirty)
                {
                    // truly unaltered
                    yield break;
                }

                var content = GetCompleteContent(textDocument, snapshot);
                if (content == null) yield break;

                using (var currentContent = new MemoryStream(content))
                {
                    var relativeFilepath = filename;
                    if (relativeFilepath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                        relativeFilepath = relativeFilepath.Substring(workingDirectory.Length);

                    var newBlob = repo.ObjectDatabase.CreateBlob(currentContent, relativeFilepath);

                    bool suppressRollback;
                    Blob blob;

                    if ((retrieveStatus & FileStatus.NewInWorkdir) != 0 || (retrieveStatus & FileStatus.NewInIndex) != 0)
                    {
                        suppressRollback = true;

                        // special handling for added files (would need updating to compare against index)
                        using (var emptyContent = new MemoryStream())
                        {
                            blob = repo.ObjectDatabase.CreateBlob(emptyContent, relativeFilepath);
                        }
                    }
                    else
                    {
                        suppressRollback = false;

                        Commit from = repo.Head.Tip;
                        TreeEntry fromEntry = from[relativeFilepath];
                        if (fromEntry == null)
                        {
                            // try again using case-insensitive comparison
                            Tree tree = from.Tree;
                            foreach (string segment in relativeFilepath.Split(Path.DirectorySeparatorChar))
                            {
                                if (tree == null)
                                    yield break;

                                fromEntry = tree.FirstOrDefault(i => string.Equals(segment, i.Name, StringComparison.OrdinalIgnoreCase));
                                if (fromEntry == null)
                                    yield break;

                                tree = fromEntry.Target as Tree;
                            }
                        }

                        blob = fromEntry.Target as Blob;
                        if (blob == null)
                            yield break;
                    }

                    var treeChanges = repo.Diff.Compare(blob, newBlob, new CompareOptions { ContextLines = ContextLines, InterhunkLines = 0 });

                    var gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines, suppressRollback);
                    var hunkRangeInfos = gitDiffParser.Parse();

                    foreach (var hunkRangeInfo in hunkRangeInfos)
                    {
                        yield return hunkRangeInfo;
                    }
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

        public void StartExternalDiff(ITextDocument textDocument)
        {
            if (textDocument == null || string.IsNullOrEmpty(textDocument.FilePath)) return;

            var filename = textDocument.FilePath;

            var repositoryPath = GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                return;

            using (var repo = new Repository(repositoryPath))
            {
                string workingDirectory = repo.Info.WorkingDirectory;
                string relativePath = Path.GetFullPath(filename);
                if (relativePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(workingDirectory.Length);

                // the name of the object in the database
                string objectName = Path.GetFileName(filename);

                Blob oldBlob = null;
                var indexEntry = repo.Index[relativePath];
                if (indexEntry != null)
                {
                    objectName = Path.GetFileName(indexEntry.Path);
                    oldBlob = repo.Lookup<Blob>(indexEntry.Id);
                }

                var tempFileName = Path.GetTempFileName();
                if (oldBlob != null)
                    File.WriteAllText(tempFileName, oldBlob.GetContentText(new FilteringOptions(relativePath)));

                IVsDifferenceService differenceService = _serviceProvider.GetService(typeof(SVsDifferenceService)) as IVsDifferenceService;
                string leftFileMoniker = tempFileName;
                // The difference service will automatically load the text from the file open in the editor, even if
                // it has changed.
                string rightFileMoniker = filename;

                string actualFilename = objectName;
                string tempPrefix = Path.GetRandomFileName().Substring(0, 5);
                string caption = string.Format("{0}_{1} vs. {1}", tempPrefix, actualFilename);

                string tooltip = null;

                string leftLabel;
                if (indexEntry != null)
                {
                    // determine if the file has been staged
                    string revision;
                    var stagedMask = FileStatus.NewInIndex | FileStatus.ModifiedInIndex;
                    if ((repo.RetrieveStatus(relativePath) & stagedMask) != 0)
                        revision = "index";
                    else
                        revision = repo.Head.Tip.Sha.Substring(0, 7);

                    leftLabel = string.Format("{0}@{1}", objectName, revision);
                }
                else if (oldBlob != null)
                {
                    // file was added
                    leftLabel = null;
                }
                else
                {
                    // we just compared to head
                    leftLabel = string.Format("{0}@{1}", objectName, repo.Head.Tip.Sha.Substring(0, 7));
                }

                string rightLabel = filename;

                string inlineLabel = null;
                string roles = null;
                __VSDIFFSERVICEOPTIONS grfDiffOptions = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
                differenceService.OpenComparisonWindow2(leftFileMoniker, rightFileMoniker, caption, tooltip, leftLabel, rightLabel, inlineLabel, roles, (uint)grfDiffOptions);

                // Since the file is marked as temporary, we can delete it now
                File.Delete(tempFileName);
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