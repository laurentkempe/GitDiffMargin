using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    public class GitCommands : IGitCommands
    {
        private const int ContextLines = 0;

        public IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename, ITextSnapshot snapshot)
        {
            var discoveredPath = Repository.Discover(Path.GetFullPath(filename));

            using (var repo = new Repository(discoveredPath))
            {
                var treeChanges = repo.Diff.Compare(new List<string> { filename }, compareOptions: new CompareOptions { ContextLines = ContextLines, InterhunkLines = 0 });
                var gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines);
                var hunkRangeInfos = gitDiffParser.Parse(snapshot);
                return hunkRangeInfos;
            }
        }

        public void StartExternalDiff(string filename)
        {
            var p = GetProcess(filename);
            p.StartInfo.Arguments = String.Format(@" difftool -y {0}", filename);

            p.Start();
        }

        public bool IsGitRepository(string directory)
        {
            var discoveredPath = Repository.Discover(Path.GetFullPath(directory));
            return Repository.IsValid(Path.GetFullPath(discoveredPath));
        }

        private static Process GetProcess(string filename)
        {
            var p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = @"git.exe";
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename) ?? string.Empty;
            return p;
        }
    }
}