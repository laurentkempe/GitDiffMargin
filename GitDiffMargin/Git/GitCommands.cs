using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using LibGit2Sharp;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    [Export(typeof(IGitCommands))]
    public class GitCommands : IGitCommands
    {
        private readonly DTE _dte;

        [ImportingConstructor]
        public GitCommands(SVsServiceProvider serviceProvider)
        {
            _dte = (DTE)serviceProvider.GetService(typeof(_DTE));
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

                var retrieveStatus = repo.Index.RetrieveStatus(filename);
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

                    if ((retrieveStatus & FileStatus.Untracked) != 0 || (retrieveStatus & FileStatus.Added) != 0)
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

            if (textDocument.IsDirty)
            {
                var docu = _dte.Documents.AllDocuments().FirstOrDefault(doc => doc.FullName == filename);
                if (docu != null)
                {
                    docu.Save();
                }
            };

            var repositoryPath = GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                return;

            using (var repo = new Repository(repositoryPath))
            {
                var diffGuiTool = repo.Config.Get<string>("diff.tool");

                if (diffGuiTool == null) return;

                string workingDirectory = repo.Info.WorkingDirectory;
                string relativePath = Path.GetFullPath(filename);
                if (relativePath.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring(workingDirectory.Length);

                var indexEntry = repo.Index[relativePath];
                var blob = repo.Lookup<Blob>(indexEntry.Id);

                var tempFileName = Path.GetTempFileName();
                File.WriteAllText(tempFileName, blob.GetContentText());

                var diffCmd = repo.Config.Get<string>("difftool." + diffGuiTool.Value + ".cmd");
                var cmd = diffCmd.Value.Replace("$LOCAL", tempFileName).Replace("$REMOTE", filename);

                var si = new STARTUPINFO();
                PROCESS_INFORMATION pi;
                CreateProcess(
                    null,
                    cmd,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi);
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

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
        
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
    }
}