#region Copyright
// 
// 
// Copyright 2007 - 2012 Innoveo Solutions AG, Zurich/Switzerland 
// All rights reserved. Use is subject to license terms.
// 
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GitDiffMargin.Git
{
    public class GitCommands : IGitCommands
    {
        public IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename)
        {
            var p = GetProcess(filename);
            p.StartInfo.Arguments = String.Format(@" diff --unified=0 {0}", Path.GetFileName(filename));
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var gitDiffParser = new GitDiffParser(output);
            return gitDiffParser.Parse();
        }

        private static Process GetProcess(string filename)
        {
            var p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = @"C:\@Tools\Development\Git\Git\bin\git.exe";
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
            return p;
        }
    }
}