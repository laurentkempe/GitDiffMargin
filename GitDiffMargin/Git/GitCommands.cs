﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    public class GitCommands : IGitCommands
    {
        private const int ContextLines = 3;

        public IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename, ITextSnapshot snapshot)
        {
            var discoveredPath = Repository.Discover(Path.GetFullPath(filename));

            using (var repo = new Repository(discoveredPath))
            {
                var treeChanges = repo.Diff.Compare(new List<string> {filename});
                var gitDiffParser = new GitDiffParser(treeChanges.Patch, ContextLines);
                var hunkRangeInfos = gitDiffParser.Parse(snapshot);
                return hunkRangeInfos;
            }

            //var p = GetProcess(filename);
            //p.StartInfo.Arguments = String.Format(@" diff --unified=0 {0}", filename);

            //p.Start();
            //// Do not wait for the child process to exit before
            //// reading to the end of its redirected stream.
            //// p.WaitForExit();
            //// Read the output stream first and then wait.
            //var output = p.StandardOutput.ReadToEnd();
            //p.WaitForExit();

        }

        public void StartExternalDiff(string filename)
        {
            var p = GetProcess(filename);
            p.StartInfo.Arguments = String.Format(@" difftool -y {0}", filename);

            p.Start();
        }

        public bool IsGitRepository(string directory)
        {
            var p = GetProcess(directory);
            p.StartInfo.Arguments = String.Format(@" rev-parse");

            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            p.WaitForExit();

            return p.ExitCode == 0;
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