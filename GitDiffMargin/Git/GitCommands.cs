using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    [Export(typeof(IGitCommands))]
    public class GitCommands : IGitCommands
    {
        private const int ContextLines = 0;
        private readonly SVsServiceProvider _serviceProvider;

        [ImportingConstructor]
        public GitCommands(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, string originalPath,
            ITextSnapshot snapshot)
        {
            var filename = textDocument.FilePath;
            var repositoryPath = GetGitRepository(Path.GetFullPath(filename), ref originalPath);
            if (repositoryPath == null)
                yield break;

            using (var repo = new Repository(repositoryPath))
            {
                var workingDirectory = repo.Info.WorkingDirectory;
                if (workingDirectory == null)
                    yield break;

                var retrieveStatus = repo.RetrieveStatus(originalPath);
                if (retrieveStatus == FileStatus.Nonexistent) yield break;

                if ((retrieveStatus & FileStatus.Ignored) != 0) yield break;

                if (retrieveStatus == FileStatus.Unaltered
                    && !textDocument.IsDirty
                    && Path.GetFullPath(filename) == originalPath)
                    yield break;

                var content = GetCompleteContent(textDocument, snapshot);
                if (content == null) yield break;

                using (var currentContent = new MemoryStream(content))
                {
                    var relativeFilepath = originalPath;
                    if (relativeFilepath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                        relativeFilepath = relativeFilepath.Substring(workingDirectory.Length);

                    var newBlob = repo.ObjectDatabase.CreateBlob(currentContent, relativeFilepath);

                    bool suppressRollback;
                    Blob blob;

                    if ((retrieveStatus & FileStatus.NewInWorkdir) != 0 ||
                        (retrieveStatus & FileStatus.NewInIndex) != 0)
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

                        var from = repo.Head.Tip;
                        var fromEntry = from[relativeFilepath];
                        if (fromEntry == null)
                        {
                            // try again using case-insensitive comparison
                            var tree = from.Tree;
                            foreach (var segment in relativeFilepath.Split(Path.DirectorySeparatorChar))
                            {
                                if (tree == null)
                                    yield break;

                                fromEntry = tree.FirstOrDefault(i =>
                                    string.Equals(segment, i.Name, StringComparison.OrdinalIgnoreCase));
                                if (fromEntry == null)
                                    yield break;

                                tree = fromEntry.Target as Tree;
                            }
                        }

                        blob = fromEntry.Target as Blob;
                        if (blob == null)
                            yield break;
                    }

                    var treeChanges = repo.Diff.Compare(blob, newBlob,
                        new CompareOptions {ContextLines = ContextLines, InterhunkLines = 0});

                    var gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines, suppressRollback);
                    var hunkRangeInfos = gitDiffParser.Parse();

                    foreach (var hunkRangeInfo in hunkRangeInfos) yield return hunkRangeInfo;
                }
            }
        }

        public void StartExternalDiff(ITextDocument textDocument, string originalPath)
        {
            if (textDocument == null || string.IsNullOrEmpty(textDocument.FilePath)) return;

            var filename = textDocument.FilePath;
            var repositoryPath = GetGitRepository(Path.GetFullPath(filename), ref originalPath);
            if (repositoryPath == null)
                return;

            using (var repo = new Repository(repositoryPath))
            {
                var workingDirectory = repo.Info.WorkingDirectory;
                var relativePath = originalPath;
                if (relativePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(workingDirectory.Length);

                // the name of the object in the database
                var objectName = Path.GetFileName(filename);

                Blob oldBlob = null;
                var indexEntry = repo.Index[relativePath];
                if (indexEntry != null)
                {
                    objectName = Path.GetFileName(indexEntry.Path);
                    oldBlob = repo.Lookup<Blob>(indexEntry.Id);
                }

                var tempFileName = Path.GetTempFileName();
                if (oldBlob != null)
                    File.WriteAllText(tempFileName, oldBlob.GetContentText(new FilteringOptions(relativePath)),
                        GetEncoding(filename));

                var differenceService =
                    _serviceProvider.GetService(typeof(SVsDifferenceService)) as IVsDifferenceService;
                var leftFileMoniker = tempFileName;
                // The difference service will automatically load the text from the file open in the editor, even if
                // it has changed. Don't use the original path here.
                var rightFileMoniker = filename;

                var actualFilename = objectName;
                var tempPrefix = Path.GetRandomFileName().Substring(0, 5);
                var caption = string.Format("{0}_{1} vs. {1}", tempPrefix, actualFilename);

                string tooltip = null;

                string leftLabel;
                if (indexEntry != null)
                {
                    // determine if the file has been staged
                    var stagedMask = FileStatus.NewInIndex | FileStatus.ModifiedInIndex;
                    var revision = (repo.RetrieveStatus(relativePath) & stagedMask) != 0
                        ? "index"
                        : repo.Head.Tip.Sha.Substring(0, 7);

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

                var rightLabel = originalPath;

                string inlineLabel = null;
                string roles = null;
                var grfDiffOptions = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
                differenceService.OpenComparisonWindow2(leftFileMoniker, rightFileMoniker, caption, tooltip, leftLabel,
                    rightLabel, inlineLabel, roles, (uint) grfDiffOptions);

                // Since the file is marked as temporary, we can delete it now
                File.Delete(tempFileName);
            }
        }

        /// <inheritdoc />
        public bool TryGetOriginalPath(string path, out string originalPath)
        {
            originalPath = null;
            if (GetGitRepository(path, ref originalPath) == null)
            {
                originalPath = path;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool IsGitRepository(string path, string originalPath)
        {
            return GetGitRepository(path, originalPath) != null;
        }

        /// <inheritdoc />
        public string GetGitRepository(string path, string originalPath)
        {
            if (originalPath == null)
                throw new ArgumentNullException(nameof(originalPath));

            return GetGitRepository(path, ref originalPath);
        }

        /// <inheritdoc />
        public string GetGitWorkingCopy(string path, string originalPath)
        {
            if (originalPath == null)
                throw new ArgumentNullException(nameof(originalPath));

            var repositoryPath = GetGitRepository(path, originalPath);
            if (repositoryPath == null)
                return null;

            using (var repository = new Repository(repositoryPath))
            {
                var workingDirectory = repository.Info.WorkingDirectory;

                return workingDirectory == null ? null : Path.GetFullPath(workingDirectory);
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

        private string GetGitRepository(string path, ref string originalPath)
        {
            if (originalPath == null)
            {
                originalPath = path;
                if (!Directory.Exists(path) && !File.Exists(path))
                    return null;

                var discoveredPath = Repository.Discover(Path.GetFullPath(path));
                if (discoveredPath != null)
                    return discoveredPath;

                originalPath = AdjustPath(path);
                if (originalPath == path)
                    return null;
            }

            if (!Directory.Exists(path) && !File.Exists(originalPath))
                return null;

            return Repository.Discover(Path.GetFullPath(originalPath));
        }

        private static Encoding GetEncoding(string file)
        {
            if (File.Exists(file))
            {
                var encoding = Encoding.UTF8;
                if (HasPreamble(file, encoding)) return encoding;
            }

            return Encoding.Default;
        }

        private static bool HasPreamble(string file, Encoding encoding)
        {
            using (var stream = File.OpenRead(file))
            {
                foreach (var b in encoding.GetPreamble())
                    if (b != stream.ReadByte())
                        return false;
            }

            return true;
        }

        private string AdjustPath(string fullPath)
        {
            // Right now the only adjustment is for CPS-based project systems which open their project files in a
            // temporary location. There are several of these, such as .csproj, .vbproj, .shproj, and .fsproj, and more
            // could appear in the future.
            if (!fullPath.EndsWith("proj", StringComparison.Ordinal)) return fullPath;

            // CPS will open the file in %TEMP%\{random name}\{ProjectFileName}
            var directoryName = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directoryName))
                return fullPath;

            directoryName = Path.GetDirectoryName(directoryName);
            if (!Path.GetTempPath().Equals(directoryName + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
                return fullPath;

            var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
                return fullPath;

            if (!ErrorHandler.Succeeded(solution.GetProjectEnum((uint) __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,
                    Guid.Empty, out var ppenum))
                || ppenum == null)
                return fullPath;

            var projectFiles = new List<string>();
            var hierarchies = new IVsHierarchy[1];
            while (true)
            {
                var hr = ppenum.Next((uint) hierarchies.Length, hierarchies, out var fetched);
                if (!ErrorHandler.Succeeded(hr))
                    return fullPath;

                for (uint i = 0; i < fetched; i++)
                {
                    if (!(hierarchies[0] is IVsProject project))
                        continue;

                    if (!ErrorHandler.Succeeded(project.GetMkDocument((uint) VSConstants.VSITEMID.Root,
                        out var projectFilePath)))
                        continue;

                    if (!Path.GetFileName(projectFilePath).Equals(Path.GetFileName(fullPath), StringComparison.Ordinal))
                        continue;

                    projectFiles.Add(projectFilePath);
                }

                if (hr != VSConstants.S_OK) break;
            }

            switch (projectFiles.Count)
            {
                case 0:
                    // No matching project file found in solution
                    return fullPath;

                case 1:
                    // Exactly one matching project file found in solution
                    return projectFiles[0];

                default:
                    // Multiple project files found in solution; try to find one with a matching file size
                    var desiredSize = new FileInfo(fullPath).Length;
                    foreach (var projectFilePath in projectFiles)
                        if (File.Exists(projectFilePath) && new FileInfo(projectFilePath).Length == desiredSize)
                            return projectFilePath;

                    // No results found
                    return fullPath;
            }
        }
    }
}