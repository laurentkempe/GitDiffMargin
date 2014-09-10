using System.ComponentModel.Composition;
using GitDiffMargin.Core;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Path = System.IO.Path;

namespace GitDiffMargin
{
    internal abstract class DiffMarginFactoryBase : IWpfTextViewMarginProvider
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; private set; }

        [Import]
        internal IClassificationFormatMapService ClassificationFormatMapService { get; private set; }

        [Import]
        internal IEditorFormatMapService EditorFormatMapService { get; private set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; private set; }

        [Import]
        internal IGitCommands GitCommands { get; private set; }

        public abstract IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer);

        protected IMarginCore TryGetMarginCore(IWpfTextViewHost textViewHost)
        {
            MarginCore marginCore;
            if (textViewHost.TextView.Properties.TryGetProperty(typeof(MarginCore), out marginCore))
                return marginCore;

            // play nice with other source control providers
            ITextView textView = textViewHost.TextView;
            ITextDataModel textDataModel = textView != null ? textView.TextDataModel : null;
            ITextBuffer documentBuffer = textDataModel != null ? textDataModel.DocumentBuffer : null;
            if (documentBuffer == null)
                return null;

            ITextDocument textDocument;
            if (!TextDocumentFactoryService.TryGetTextDocument(documentBuffer, out textDocument))
                return null;

            var filename = textDocument.FilePath;
            var repositoryPath = GitCommands.GetGitRepository(Path.GetFullPath(filename));
            if (repositoryPath == null)
                return null;

            return textViewHost.TextView.Properties.GetOrCreateSingletonProperty(
                        () => new MarginCore(textViewHost.TextView, TextDocumentFactoryService, ClassificationFormatMapService, ServiceProvider, EditorFormatMapService, GitCommands));
        }
    }
}