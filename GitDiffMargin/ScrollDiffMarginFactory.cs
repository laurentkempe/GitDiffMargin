using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(ScrollDiffMargin.MarginNameConst)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [MarginContainer(PredefinedMarginNames.VerticalScrollBar)]
    [Order(After = PredefinedMarginNames.OverviewChangeTracking)]
    [Order(Before = PredefinedMarginNames.OverviewError)]
    [Order(Before = PredefinedMarginNames.OverviewMark)]
    [Order(Before = PredefinedMarginNames.OverviewSourceImage)]
    internal sealed class ScrollDiffMarginFactory : DiffMarginFactoryBase
    {
        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            var marginCore = GetMarginCore(textViewHost);
            return new ScrollDiffMargin(textViewHost.TextView, marginCore, containerMargin);
        }
    }
}