using System.IO;
using System.Threading;
using EnvDTE;
using GitDiffMargin.Git;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Stopwatch = System.Diagnostics.Stopwatch;
using Task = System.Threading.Tasks.Task;

namespace GitDiffMargin
{
    public class DiffUpdateBackgroundParser : BackgroundParser
    {
        private readonly FileSystemWatcher _watcher;
        private readonly IGitCommands _commands;
        private readonly DTE _dte;

        public DiffUpdateBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService, SVsServiceProvider serviceProvider, IGitCommands commands)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
             _dte = (DTE)serviceProvider.GetService(typeof(DTE));

            _commands = commands;
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            if (!_commands.IsGitRepository(_dte.Solution.FullName)) return;

            var solutionDirectory = Path.GetDirectoryName(_dte.Solution.FullName);

            _watcher = new FileSystemWatcher(solutionDirectory);
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += HandleFileSystemChanged;
            _watcher.Created += HandleFileSystemChanged;
            _watcher.Deleted += HandleFileSystemChanged;
            _watcher.Renamed += HandleFileSystemChanged;
            _watcher.EnableRaisingEvents = true;
        }
        
        private void HandleFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            Action action = () => ProcessFileSystemChange(e);
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private void ProcessFileSystemChange(FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed && Directory.Exists(e.FullPath))
                return;

            if (string.Equals(Path.GetExtension(e.Name), ".lock", StringComparison.OrdinalIgnoreCase))
                return;

            MarkDirty(true);
        }

        public override string Name
        {
            get
            {
                return "Git Diff Analyzer";
            }
        }

        protected override void ReParseImpl()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var snapshot = TextBuffer.CurrentSnapshot;
                var fullName = _dte.ActiveDocument.ActiveWindow.Document.FullName;

                var diffs = _commands.GetGitDiffFor(fullName).ToList();
                var result = new DiffParseResultEventArgs(snapshot, stopwatch.Elapsed, diffs);
                OnParseComplete(result);
            }
            catch (InvalidOperationException)
            {
                MarkDirty(true);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                }
            }
        }
    }
}