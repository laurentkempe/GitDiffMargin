#region using

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#endregion

namespace GitDiffMargin
{
    [Export(typeof (IWpfTextViewMarginProvider))]
    [Name(EditorDiffMargin.MarginNameConst)]
    [Order(Before = PredefinedMarginNames.LineNumber)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class EditorDiffMarginFactory : DiffMarginFactoryBase
    {
        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            var marginCore = TryGetMarginCore(textViewHost);
            if (marginCore == null)
                return null;

            return new EditorDiffMargin(textViewHost.TextView, marginCore);
        }
    }
}