namespace GitDiffMargin.Settings
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;

    [Export(typeof(EditorOptionDefinition))]
    public sealed class CompareToIndex : ViewOptionDefinition<bool>
    {
        public override bool Default
        {
            get
            {
                // default compare to head
                return false;
            }
        }

        public override EditorOptionKey<bool> Key
        {
            get
            {
                return GitDiffMarginTextViewOptions.CompareToIndexId;
            }
        }
    }
}
