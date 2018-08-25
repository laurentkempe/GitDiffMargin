#region using

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#endregion

namespace GitDiffMargin
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(EditorDiffMargin.MarginNameConst)]
    [Order(After = PredefinedMarginNames.Spacer, Before = PredefinedMarginNames.Outlining)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class EditorDiffMarginFactory : DiffMarginFactoryBase
    {
        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost,
            IWpfTextViewMargin containerMargin)
        {
            var marginCore = TryGetMarginCore(textViewHost);

            return marginCore == null ? null : new EditorDiffMargin(textViewHost.TextView, marginCore);
        }
    }
}