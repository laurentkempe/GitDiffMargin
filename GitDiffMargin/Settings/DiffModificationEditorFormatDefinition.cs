using System.ComponentModel.Composition;
using System.Windows.Media;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace GitDiffMargin.Settings
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(DiffFormatNames.Modification)]
    [UserVisible(true)]
    internal sealed class DiffModificationEditorFormatDefinition : EditorFormatDefinition
    {
        public DiffModificationEditorFormatDefinition()
        {
            BackgroundColor = Color.FromRgb(160, 200, 255);
            ForegroundCustomizable = false;
            DisplayName = "Git Diff Modification";
        }
    }
}