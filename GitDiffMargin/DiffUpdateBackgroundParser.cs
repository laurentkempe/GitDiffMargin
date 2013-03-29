using GitDiffMargin.Git;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace GitDiffMargin
{
    public class DiffUpdateBackgroundParser : BackgroundParser
    {
        private readonly IGitCommands _commands;
        private ITextDocument _textDocument;

        public DiffUpdateBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService, IGitCommands commands)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            _commands = commands;
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            if (TextDocumentFactoryService.TryGetTextDocument(TextBuffer, out _textDocument))
            {
                _textDocument.FileActionOccurred += OnFileActionOccurred;
            }
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
                if (!TextDocumentFactoryService.TryGetTextDocument(TextBuffer, out textDocument))
                {
                    textDocument = null;
                }
                var diff = textDocument != null ? _commands.GetGitDiffFor(textDocument.FilePath, snapshot) : Enumerable.Empty<HunkRangeInfo>();

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
                _textDocument.FileActionOccurred -= OnFileActionOccurred;
            }
        }
    }
}