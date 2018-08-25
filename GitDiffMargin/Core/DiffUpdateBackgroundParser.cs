using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitDiffMargin.Git;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Core
{
    public class DiffUpdateBackgroundParser : BackgroundParser
    {
        private readonly IGitCommands _commands;
        private readonly ITextBuffer _documentBuffer;
        private readonly string _originalPath;
        private readonly ITextDocument _textDocument;
        private readonly FileSystemWatcher _watcher;

        internal DiffUpdateBackgroundParser(ITextBuffer textBuffer, ITextBuffer documentBuffer, string originalPath,
            TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService, IGitCommands commands)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            _documentBuffer = documentBuffer;
            _commands = commands;
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            if (!TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out _textDocument)) return;

            _originalPath = originalPath;

            if (!_commands.IsGitRepository(_textDocument.FilePath, _originalPath)) return;

            _textDocument.FileActionOccurred += OnFileActionOccurred;

            var repositoryDirectory = _commands.GetGitRepository(_textDocument.FilePath, _originalPath);
            if (repositoryDirectory == null) return;

            _watcher = new FileSystemWatcher(repositoryDirectory);
            _watcher.Changed += HandleFileSystemChanged;
            _watcher.Created += HandleFileSystemChanged;
            _watcher.Deleted += HandleFileSystemChanged;
            _watcher.Renamed += HandleFileSystemChanged;
            _watcher.EnableRaisingEvents = true;
        }

        public override string Name => "Git Diff Analyzer";

        private void HandleFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            void HandleFileSystemChanged()
            {
                try
                {
                    ProcessFileSystemChange(e);
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex)) throw;
                }
            }

            Task.Factory.StartNew(HandleFileSystemChanged, CancellationToken.None, TaskCreationOptions.None,
                TaskScheduler.Default);
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
            if ((e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0) MarkDirty(true);
        }

        protected override void ReParseImpl()
        {
            try
            {
                var snapshot = TextBuffer.CurrentSnapshot;
                if (!TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out var textDocument)) return;

                var diff = _commands.GetGitDiffFor(textDocument, _originalPath, snapshot);
                var result = new DiffParseResultEventArgs(diff.ToList());
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

            if (!disposing) return;

            if (_textDocument != null) _textDocument.FileActionOccurred -= OnFileActionOccurred;
            _watcher?.Dispose();
        }
    }
}