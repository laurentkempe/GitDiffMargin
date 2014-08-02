using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

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

        public abstract IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer);

        protected MarginCore GetMarginCore(IWpfTextViewHost textViewHost)
        {
            return textViewHost.TextView.Properties.GetOrCreateSingletonProperty(
                        () => new MarginCore(textViewHost.TextView, TextDocumentFactoryService, ClassificationFormatMapService, ServiceProvider, EditorFormatMapService));
        }
    }
}