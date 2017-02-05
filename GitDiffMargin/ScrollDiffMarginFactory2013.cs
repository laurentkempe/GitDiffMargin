using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(ScrollDiffMargin.MarginNameConst + "2013")]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [MarginContainer(PredefinedMarginNames.VerticalScrollBar)]
    [Order(After = "OverviewChangeTrackingMargin")]
    [Order(Before = "OverviewErrorMargin")]
    [Order(Before = "OverviewMarkMargin")]
    [Order(Before = "OverviewSourceImageMargin")]
    internal sealed class ScrollDiffMarginFactory2013 : DiffMarginFactoryBase
    {
        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost,
            IWpfTextViewMargin containerMargin)
        {
            // Visual Studio uses assembly binding redirection for the Shell assembly.
            if (typeof(ErrorHandler).Assembly.GetName().Version.Major < 12)
                return null;

            var marginCore = TryGetMarginCore(textViewHost);
            if (marginCore == null)
                return null;

            return new ScrollDiffMargin(textViewHost.TextView, marginCore, containerMargin);
        }
    }
}