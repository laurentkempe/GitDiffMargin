using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using __VSDIFFSERVICEOPTIONS = Microsoft.VisualStudio.Shell.Interop.__VSDIFFSERVICEOPTIONS;
using __VSENUMPROJFLAGS = Microsoft.VisualStudio.Shell.Interop.__VSENUMPROJFLAGS;
using IEnumHierarchies = Microsoft.VisualStudio.Shell.Interop.IEnumHierarchies;
using IVsDifferenceService = Microsoft.VisualStudio.Shell.Interop.IVsDifferenceService;
using IVsHierarchy = Microsoft.VisualStudio.Shell.Interop.IVsHierarchy;
using IVsProject = Microsoft.VisualStudio.Shell.Interop.IVsProject;
using IVsSolution = Microsoft.VisualStudio.Shell.Interop.IVsSolution;
using SVsDifferenceService = Microsoft.VisualStudio.Shell.Interop.SVsDifferenceService;
using SVsSolution = Microsoft.VisualStudio.Shell.Interop.SVsSolution;

namespace GitDiffMargin.Git
{
    [Export(typeof(IGitCommands))]
    public class GitCommands : IGitCommands
    {
        private readonly SVsServiceProvider _serviceProvider;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        private class VersionAccessor : LibGit2Sharp.Version
        {
            public static readonly VersionAccessor Instance = new VersionAccessor();
        }

        static GitCommands()
        {
            try
            {
                var currentFolder = Path.GetDirectoryName(typeof(GitCommands).Assembly.Location);
                var subFolder = Environment.Is64BitProcess ? "x64" : "x86";
                LoadLibrary(Path.Combine(currentFolder, subFolder, $"git2-{VersionAccessor.Instance.LibGit2CommitSha}.dll"));
            }
            catch
            {
                // Ignore this error; if the library failed to load it will produce exceptions at the point where it is
                // used.
            }
        }

        [ImportingConstructor]
        public GitCommands(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private const int ContextLines = 0;

        public IEnumerable<HunkRangeInfo> GetGitDiffFor(ITextDocument textDocument, string originalPath, ITextSnapshot snapshot)
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

                // Determine 'from' tree.
                var currentBranch = repo.Head.FriendlyName;
                var baseCommitEntry = repo.Config.Get<string>(string.Format("branch.{0}.diffmarginbase", currentBranch));
                Commit from = null;
                if (baseCommitEntry != null)
                {
                    var baseCommit = repo.Lookup<Commit>(baseCommitEntry.Value);
                    if (baseCommit != null)
                    {
                        // Found a merge base to diff from.
                        from = baseCommit;
                    }
                }

                if (from == null
                    && retrieveStatus == FileStatus.Unaltered
                    && !textDocument.IsDirty
                    && Path.GetFullPath(filename) == originalPath)
                {
                    // Truly unaltered. The `IsDirty` check isn't valid for cases where the textDocument is a view of a
                    // temporary copy of the file, since the temporary copy could have been made using unsaved changes
                    // and still appear "not dirty".
                    yield break;
                }

                var content = GetCompleteContent(textDocument, snapshot);
                if (content == null) yield break;

                using (var currentContent = new MemoryStream(content))
                {
                    var relativeFilepath = originalPath;
                    if (relativeFilepath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                        relativeFilepath = relativeFilepath.Substring(workingDirectory.Length);

                    relativeFilepath = relativeFilepath.Replace('\\', '/');

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

                        from = from ?? repo.Head.Tip;
                        TreeEntry fromEntry = from[relativeFilepath];
                        if (fromEntry == null)
                        {
                            // try again using case-insensitive comparison
                            Tree tree = from.Tree;
                            foreach (string segment in relativeFilepath.Split('/'))
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

        public void StartExternalDiff(ITextDocument textDocument, string originalPath)
        {
            if (textDocument == null || string.IsNullOrEmpty(textDocument.FilePath)) return;

            var filename = textDocument.FilePath;
            var repositoryPath = GetGitRepository(Path.GetFullPath(filename), ref originalPath);
            if (repositoryPath == null)
                return;

            using (var repo = new Repository(repositoryPath))
            {
                string workingDirectory = repo.Info.WorkingDirectory;
                string relativePath = originalPath;
                if (relativePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(workingDirectory.Length);

                // the name of the object in the database
                string objectName = Path.GetFileName(filename);

                Blob oldBlob = null;
                var indexEntry = repo.Index[relativePath.Replace("\\", "/")];
                if (indexEntry != null)
                {
                    objectName = Path.GetFileName(indexEntry.Path);
                    oldBlob = repo.Lookup<Blob>(indexEntry.Id);
                }

                var tempFileName = Path.GetTempFileName();
                if (oldBlob != null)
                    File.WriteAllText(tempFileName, oldBlob.GetContentText(new FilteringOptions(relativePath)), GetEncoding(filename));

                IVsDifferenceService differenceService = _serviceProvider.GetService(typeof(SVsDifferenceService)) as IVsDifferenceService;
                string leftFileMoniker = tempFileName;
                // The difference service will automatically load the text from the file open in the editor, even if
                // it has changed. Don't use the original path here.
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

                string rightLabel = originalPath;

                string inlineLabel = null;
                string roles = null;
                __VSDIFFSERVICEOPTIONS grfDiffOptions = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
                differenceService.OpenComparisonWindow2(leftFileMoniker, rightFileMoniker, caption, tooltip, leftLabel, rightLabel, inlineLabel, roles, (uint)grfDiffOptions);

                // Since the file is marked as temporary, we can delete it now
                File.Delete(tempFileName);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool IsGitRepository(string path, string originalPath)
        {
            return GetGitRepository(path, originalPath) != null;
        }

        /// <inheritdoc/>
        public string GetGitRepository(string path, string originalPath)
        {
            if (originalPath == null)
                throw new ArgumentNullException(nameof(originalPath));

            return GetGitRepository(path, ref originalPath);
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

        /// <inheritdoc/>
        public string GetGitWorkingCopy(string path, string originalPath)
        {
            if (originalPath == null)
                throw new ArgumentNullException(nameof(originalPath));

            var repositoryPath = GetGitRepository(path, originalPath);
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

        static Encoding GetEncoding(string file)
        {
            if (File.Exists(file))
            {
                var encoding = Encoding.UTF8;
                if (HasPreamble(file, encoding))
                {
                    return encoding;
                }
            }

            return Encoding.Default;
        }

        static bool HasPreamble(string file, Encoding encoding)
        {
            using (var stream = File.OpenRead(file))
            {
                foreach (var b in encoding.GetPreamble())
                {
                    if (b != stream.ReadByte())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private string AdjustPath(string fullPath)
        {
            // Right now the only adjustment is for CPS-based project systems which open their project files in a
            // temporary location. There are several of these, such as .csproj, .vbproj, .shproj, and .fsproj, and more
            // could appear in the future.
            if (!fullPath.EndsWith("proj", StringComparison.Ordinal))
            {
                return fullPath;
            }

            // CPS will open the file in %TEMP%\{random name}\{ProjectFileName}
            string directoryName = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directoryName))
                return fullPath;

            directoryName = Path.GetDirectoryName(directoryName);
            if (!Path.GetTempPath().Equals(directoryName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return fullPath;

            IVsSolution solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
                return fullPath;

            if (!ErrorHandler.Succeeded(solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, Guid.Empty, out IEnumHierarchies ppenum))
                || ppenum == null)
            {
                return fullPath;
            }

            List<string> projectFiles = new List<string>();
            IVsHierarchy[] hierarchies = new IVsHierarchy[1];
            while (true)
            {
                int hr = ppenum.Next((uint)hierarchies.Length, hierarchies, out uint fetched);
                if (!ErrorHandler.Succeeded(hr))
                    return fullPath;

                for (uint i = 0; i < fetched; i++)
                {
                    if (!(hierarchies[0] is IVsProject project))
                        continue;

                    if (!ErrorHandler.Succeeded(project.GetMkDocument((uint)VSConstants.VSITEMID.Root, out string projectFilePath)))
                        continue;

                    if (!Path.GetFileName(projectFilePath).Equals(Path.GetFileName(fullPath), StringComparison.Ordinal))
                        continue;

                    projectFiles.Add(projectFilePath);
                }

                if (hr != VSConstants.S_OK)
                {
                    // No more projects
                    break;
                }
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
                long desiredSize = new FileInfo(fullPath).Length;
                foreach (var projectFilePath in projectFiles)
                {
                    if (File.Exists(projectFilePath) && new FileInfo(projectFilePath).Length == desiredSize)
                        return projectFilePath;
                }

                // No results found
                return fullPath;
            }
        }
    }
}