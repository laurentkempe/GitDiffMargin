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
            var discoveredPath = Repository.Discover(Path.GetFullPath(filename));

            using (var repo = new Repository(discoveredPath))
            {
                var diffGuiTool = repo.Config.Get<string>("diff.guitool");

                if (diffGuiTool == null) return;

                var diffCmd = repo.Config.Get<string>("difftool." + diffGuiTool.Value + ".path");

                var indexEntry = repo.Index[filename.Replace(repo.Info.WorkingDirectory, "")];
                var blob = repo.Lookup<Blob>(indexEntry.Id);

                var tempFileName = Path.GetTempFileName();
                File.WriteAllBytes(tempFileName, blob.Content);
                    
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = diffCmd.Value,
                        Arguments = String.Format("{0} {1}", tempFileName, filename)
                    }
                };
                process.Start();
            }
        }

        public bool IsGitRepository(string directory)
        {
            var discoveredPath = Repository.Discover(Path.GetFullPath(directory));
            return Repository.IsValid(Path.GetFullPath(discoveredPath));
        }
    }
}