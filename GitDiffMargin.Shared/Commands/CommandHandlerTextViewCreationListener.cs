#if !LEGACY_COMMANDS

namespace GitDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CommandHandlerTextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;

        [ImportingConstructor]
        public CommandHandlerTextViewCreationListener(SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _serviceProvider = serviceProvider;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            // The new command handling approach does not require that the command filter be enabled. The command
            // implementations interact directly with the handler via its IOleCommandTarget interface.
            GitDiffMarginCommandHandler filter = new GitDiffMarginCommandHandler(textViewAdapter, _editorAdaptersFactoryService, textView);
            textView.Properties.AddProperty(typeof(GitDiffMarginCommandHandler), filter);
        }
    }
}

#endif
