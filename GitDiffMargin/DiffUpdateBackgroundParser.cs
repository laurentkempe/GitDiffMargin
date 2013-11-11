using System.IO;
using System.Threading;
using GitDiffMargin.Git;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Stopwatch = System.Diagnostics.Stopwatch;
using Task = System.Threading.Tasks.Task;

namespace GitDiffMargin
{
    public class DiffUpdateBackgroundParser : BackgroundParser
    {
        private readonly FileSystemWatcher _watcher;
        private readonly IGitCommands _commands;
        private readonly ITextDocument _textDocument;
        private readonly ITextBuffer _documentBuffer;

        public DiffUpdateBackgroundParser(ITextBuffer textBuffer, ITextBuffer documentBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService, IGitCommands commands)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            _documentBuffer = documentBuffer;
            _commands = commands;
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            if (TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out _textDocument))
            {
                if (_commands.IsGitRepository(_textDocument.FilePath))
                {
                    _textDocument.FileActionOccurred += OnFileActionOccurred;

                    var solutionDirectory = _commands.GetGitRepository(_textDocument.FilePath);

                    if (!string.IsNullOrWhiteSpace(solutionDirectory))
                    {
                        _watcher = new FileSystemWatcher(solutionDirectory) {IncludeSubdirectories = true};
                        _watcher.Changed += HandleFileSystemChanged;
                        _watcher.Created += HandleFileSystemChanged;
                        _watcher.Deleted += HandleFileSystemChanged;
                        _watcher.Renamed += HandleFileSystemChanged;
                        _watcher.EnableRaisingEvents = true;
                    }
                }
            }
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

        private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                MarkDirty(true);
            }
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
                ITextDocument textDocument;
                if (!TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out textDocument))
                {
                    textDocument = null;
                }
                var diff = textDocument != null ? _commands.GetGitDiffFor(textDocument.FilePath) : Enumerable.Empty<HunkRangeInfo>();

                var result = new DiffParseResultEventArgs(snapshot, stopwatch.Elapsed, diff.ToList());
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
                if (_textDocument != null)
                {
                    _textDocument.FileActionOccurred -= OnFileActionOccurred;
                }
                if (_watcher != null)
                {
                    _watcher.Dispose();
                }
            }
        }
    }
}