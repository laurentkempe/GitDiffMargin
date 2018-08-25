using System;
using System.Linq;
using GitDiffMargin.ViewModel;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Tvl.VisualStudio.Shell;
using Tvl.VisualStudio.Text;

namespace GitDiffMargin
{
    internal sealed class GitDiffMarginCommandHandler : TextViewCommandFilter
    {
        internal const string GitDiffMarginCommandSet = "691DB887-6D82-46A9-B0AF-407C7F0E39BE";

        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ITextView _textView;

        public GitDiffMarginCommandHandler(IVsTextView textViewAdapter,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ITextView textView)
            : base(textViewAdapter)
        {
            _editorAdaptersFactoryService = editorAdaptersFactoryService ?? throw new ArgumentNullException(nameof(editorAdaptersFactoryService));
            _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        }

        protected override OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId,
            OleCommandText oleCommandText)
        {
            if (commandGroup == typeof(GitDiffMarginCommand).GUID)
                switch ((GitDiffMarginCommand) commandId)
                {
                    case GitDiffMarginCommand.ShowPopup:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        var diffViewModel = GetCurrentDiffViewModel(viewModel);

                        if (diffViewModel != null)
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        return OLECMDF.OLECMDF_SUPPORTED;
                    }
                    case GitDiffMarginCommand.PreviousChange:
                    case GitDiffMarginCommand.NextChange:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        // First look for a diff already showing a popup
                        var diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                            .FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            var command = (GitDiffMarginCommand) commandId == GitDiffMarginCommand.NextChange
                                ? viewModel.NextChangeCommand
                                : viewModel.PreviousChangeCommand;
                            if (command.CanExecute(diffViewModel))
                                return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                            return OLECMDF.OLECMDF_SUPPORTED;
                        }

                        diffViewModel = GetDiffViewModelToMoveTo(commandId, viewModel);

                        if (diffViewModel != null)
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        return OLECMDF.OLECMDF_SUPPORTED;
                    }

                    case GitDiffMarginCommand.RollbackChange:
                    case GitDiffMarginCommand.CopyOldText:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        var diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                            .FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            var command = (GitDiffMarginCommand) commandId == GitDiffMarginCommand.RollbackChange
                                ? diffViewModel.RollbackCommand
                                : diffViewModel.CopyOldTextCommand;
                            if (command.CanExecute(diffViewModel))
                                return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        }

                        // This command only works when a popup is open
                        return OLECMDF.OLECMDF_SUPPORTED;
                    }

                    case GitDiffMarginCommand.ShowDiff:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        if (viewModel.DiffViewModels.Any())
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        return OLECMDF.OLECMDF_SUPPORTED;
                    }

                    case GitDiffMarginCommand.GitDiffToolbar:
                    case GitDiffMarginCommand.GitDiffToolbarGroup:
                        // these aren't actually commands, but IDs of the command bars and groups
                        break;
                }

            return 0;
        }

        protected override bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions,
            IntPtr pvaIn, IntPtr pvaOut)
        {
            if (commandGroup == typeof(GitDiffMarginCommand).GUID)
            {
                EditorDiffMarginViewModel viewModel = null;
                EditorDiffViewModel diffViewModel = null;
                if (TryGetMarginViewModel(out viewModel))
                    diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                        .FirstOrDefault(i => i.ShowPopup);

                switch ((GitDiffMarginCommand) commandId)
                {
                    case GitDiffMarginCommand.ShowPopup:
                    {
                        diffViewModel = GetCurrentDiffViewModel(viewModel);

                        if (diffViewModel != null)
                        {
                            diffViewModel.ShowPopup = true;
                            return true;
                        }

                        return false;
                    }
                    case GitDiffMarginCommand.PreviousChange:
                    case GitDiffMarginCommand.NextChange:
                    {
                        if (viewModel == null)
                            return false;

                        var command = (GitDiffMarginCommand) commandId == GitDiffMarginCommand.NextChange
                            ? viewModel.NextChangeCommand
                            : viewModel.PreviousChangeCommand;

                        // First look for a diff already showing a popup
                        if (diffViewModel != null)
                        {
                            command.Execute(diffViewModel);
                            return true;
                        }

                        diffViewModel = GetDiffViewModelToMoveTo(commandId, viewModel);

                        if (diffViewModel == null) return false;

                        viewModel.MoveToChange(diffViewModel, 0);
                        return true;
                    }

                    case GitDiffMarginCommand.RollbackChange:
                    case GitDiffMarginCommand.CopyOldText:
                    {
                        if (diffViewModel == null)
                            return false;

                        var command = (GitDiffMarginCommand) commandId == GitDiffMarginCommand.RollbackChange
                            ? diffViewModel.RollbackCommand
                            : diffViewModel.CopyOldTextCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                    case GitDiffMarginCommand.ShowDiff:
                    {
                        if (diffViewModel == null)
                            return false;

                        var command = diffViewModel.ShowDifferenceCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                    case GitDiffMarginCommand.GitDiffToolbar:
                    case GitDiffMarginCommand.GitDiffToolbarGroup:
                        // these aren't actually commands, but IDs of the command bars and groups
                        break;
                }
            }

            return false;
        }

        private EditorDiffViewModel GetDiffViewModelToMoveTo(uint commandId, DiffMarginViewModelBase viewModel)
        {
            var lineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

            return (GitDiffMarginCommand) commandId == GitDiffMarginCommand.NextChange
                ? viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                    .FirstOrDefault(model => model.LineNumber > lineNumber)
                : viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                    .LastOrDefault(model => model.LineNumber < lineNumber);
        }

        private EditorDiffViewModel GetCurrentDiffViewModel(DiffMarginViewModelBase viewModel)
        {
            var caretLineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

            return viewModel.DiffViewModels.OfType<EditorDiffViewModel>()
                .FirstOrDefault(diff => diff.IsLineNumberBetweenDiff(caretLineNumber));
        }

        private bool TryGetMarginViewModel(out EditorDiffMarginViewModel viewModel)
        {
            viewModel = null;

            var textViewHost = _editorAdaptersFactoryService.GetWpfTextViewHost(TextViewAdapter);
            if (textViewHost == null)
                return false;

            var margin = textViewHost.GetTextViewMargin(EditorDiffMargin.MarginNameConst) as EditorDiffMargin;
            if (margin == null)
                return false;

            viewModel = margin.VisualElement.DataContext as EditorDiffMarginViewModel;
            return viewModel != null;
        }
    }
}