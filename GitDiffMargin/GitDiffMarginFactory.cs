#region using

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#endregion

namespace GitDiffMargin
{
    [Export(typeof (IWpfTextViewMarginProvider))]
    [Name(GitDiffMargin.MarginName)]
    [Order(Before = PredefinedMarginNames.LineNumber)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService { get; private set; }

        [Import]
        internal IClassificationFormatMapService ClassificationFormatMapService { get; private set; }

        [Import]
        internal IEditorFormatMapService EditorFormatMapService { get; private set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new GitDiffMargin(textViewHost.TextView, this);
        }
    }
}