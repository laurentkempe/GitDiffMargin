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

        public DiffUpdateBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService, IGitCommands commands)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            _commands = commands;
            ReparseDelay = TimeSpan.FromMilliseconds(500);
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
    }
}