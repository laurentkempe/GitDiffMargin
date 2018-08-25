using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin.LegacyCommands
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class LegacyCommandHandlerTextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly SVsServiceProvider _serviceProvider;

        [ImportingConstructor]
        public LegacyCommandHandlerTextViewCreationListener(SVsServiceProvider serviceProvider,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _serviceProvider = serviceProvider;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            var filter = new GitDiffMarginCommandHandler(textViewAdapter, _editorAdaptersFactoryService, textView);
            filter.Enabled = true;
            textView.Properties.AddProperty(typeof(GitDiffMarginCommandHandler), filter);
        }
    }
}