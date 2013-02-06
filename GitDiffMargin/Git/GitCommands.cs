using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Git
{
    public class GitCommands : IGitCommands
    {
        public IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename, ITextSnapshot snapshot)
        {
            var p = GetProcess(filename);
            p.StartInfo.Arguments = String.Format(@" diff --unified=0 {0}", filename);

            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var gitDiffParser = new GitDiffParser(output);
            return gitDiffParser.Parse(snapshot);
        }

        public void StartExternalDiff(string filename)
        {
            var p = GetProcess(filename);
            p.StartInfo.Arguments = String.Format(@" difftool -y {0}", filename);

            ActivityLog.LogInformation("GitDiffMargin", "Command:" + p.StartInfo.Arguments);

            p.Start();
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
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
            return p;
        }
    }
}