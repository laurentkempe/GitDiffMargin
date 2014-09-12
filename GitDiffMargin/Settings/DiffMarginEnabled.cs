namespace GitDiffMargin.Settings
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;

    [Export(typeof(EditorOptionDefinition))]
    public sealed class DiffMarginEnabled : ViewOptionDefinition<bool>
    {
        public override bool Default
        {
            get
            {
                return true;
            }
        }

        public override EditorOptionKey<bool> Key
        {
            get
            {
                return GitDiffMarginTextViewOptions.DiffMarginEnabledId;
            }
        }
    }
}
