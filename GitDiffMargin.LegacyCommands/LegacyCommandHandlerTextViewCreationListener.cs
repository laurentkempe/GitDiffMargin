namespace GitDiffMargin.LegacyCommands
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
    internal class LegacyCommandHandlerTextViewCreationListener : IVsTextViewCreationListener
    {
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;

        [ImportingConstructor]
        public LegacyCommandHandlerTextViewCreationListener(SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _serviceProvider = serviceProvider;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            GitDiffMarginCommandHandler filter = new GitDiffMarginCommandHandler(textViewAdapter, _editorAdaptersFactoryService, textView);
            filter.Enabled = true;
            textView.Properties.AddProperty(typeof(GitDiffMarginCommandHandler), filter);
        }
    }
}
