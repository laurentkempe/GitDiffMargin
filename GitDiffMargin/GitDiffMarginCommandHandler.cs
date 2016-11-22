namespace GitDiffMargin
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using GitDiffMargin.ViewModel;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Text.Editor;
    using IVsTextView = Microsoft.VisualStudio.TextManager.Interop.IVsTextView;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;

    internal sealed class GitDiffMarginCommandHandler : TextViewCommandFilter
    {
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ITextView _textView;

        public GitDiffMarginCommandHandler(IVsTextView textViewAdapter, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ITextView textView)
            : base(textViewAdapter)
        {
            if (editorAdaptersFactoryService == null)
                throw new ArgumentNullException("editorAdaptersFactoryService");
            if (textView == null)
                throw new ArgumentNullException("textView");

            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _textView = textView;
        }

        protected override OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId)
        {
            if (commandGroup == typeof(GitDiffMarginCommand).GUID)
            {
                switch ((GitDiffMarginCommand)commandId)
                {
                case GitDiffMarginCommand.PreviousChange:
                case GitDiffMarginCommand.NextChange:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        // First look for a diff already showing a popup
                        EditorDiffViewModel diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            RelayCommand<DiffViewModel> command = (GitDiffMarginCommand)commandId == GitDiffMarginCommand.NextChange ? viewModel.NextChangeCommand : viewModel.PreviousChangeCommand;
                            if (command.CanExecute(diffViewModel))
                                return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                            else
                                return OLECMDF.OLECMDF_SUPPORTED;
                        }

                        diffViewModel = GetDiffViewModelToMoveTo(commandId, viewModel);

                        if (diffViewModel != null)
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        else
                            return OLECMDF.OLECMDF_SUPPORTED;
                    }

                case GitDiffMarginCommand.RollbackChange:
                case GitDiffMarginCommand.CopyOldText:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        EditorDiffViewModel diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            ICommand command = (GitDiffMarginCommand)commandId == GitDiffMarginCommand.RollbackChange ? diffViewModel.RollbackCommand : diffViewModel.CopyOldTextCommand;
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
                        else
                            return OLECMDF.OLECMDF_SUPPORTED;
                    }

                case GitDiffMarginCommand.ToggleCompareToIndex:
                    if (!_textView.Options.GetOptionValue(GitDiffMarginTextViewOptions.DiffMarginEnabledId))
                        return OLECMDF.OLECMDF_SUPPORTED;
                    else if (!_textView.Options.GetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId))
                        return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                    else
                        return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_LATCHED;

                case GitDiffMarginCommand.CompareToIndex:
                case GitDiffMarginCommand.CompareToHead:
                    if (!_textView.Options.GetOptionValue(GitDiffMarginTextViewOptions.DiffMarginEnabledId))
                        return OLECMDF.OLECMDF_SUPPORTED;
                    else
                        return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;

                case GitDiffMarginCommand.GitDiffToolbar:
                case GitDiffMarginCommand.GitDiffToolbarGroup:
                    // these aren't actually commands, but IDs of the command bars and groups
                    break;

                default:
                    break;
                }
            }

            return 0;
        }

        protected override bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (commandGroup == typeof(GitDiffMarginCommand).GUID)
            {
                EditorDiffMarginViewModel viewModel = null;
                EditorDiffViewModel diffViewModel = null;
                if (TryGetMarginViewModel(out viewModel))
                    diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);

                switch ((GitDiffMarginCommand)commandId)
                {
                case GitDiffMarginCommand.PreviousChange:
                case GitDiffMarginCommand.NextChange:
                    {
                        if (viewModel == null)
                            return false;

                        RelayCommand<DiffViewModel> command = (GitDiffMarginCommand)commandId == GitDiffMarginCommand.NextChange ? viewModel.NextChangeCommand : viewModel.PreviousChangeCommand;

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

                        ICommand command = (GitDiffMarginCommand)commandId == GitDiffMarginCommand.RollbackChange ? diffViewModel.RollbackCommand : diffViewModel.CopyOldTextCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                case GitDiffMarginCommand.ShowDiff:
                    {
                        if (diffViewModel == null)
                            return false;

                        ICommand command = diffViewModel.ShowDifferenceCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                case GitDiffMarginCommand.ToggleCompareToIndex:
                    if (diffViewModel != null)
                    {
                        // The margin will not update if the pop-up is visible
                        diffViewModel.ShowPopup = false;
                    }

                    ToggleCompareToIndex();
                    return true;

                case GitDiffMarginCommand.CompareToIndex:
                    if (diffViewModel != null)
                    {
                        // The margin will not update if the pop-up is visible
                        diffViewModel.ShowPopup = false;
                    }

                    CompareToIndex();
                    return true;

                case GitDiffMarginCommand.CompareToHead:
                    if (diffViewModel != null)
                    {
                        // The margin will not update if the pop-up is visible
                        diffViewModel.ShowPopup = false;
                    }

                    CompareToHead();
                    return true;

                case GitDiffMarginCommand.GitDiffToolbar:
                case GitDiffMarginCommand.GitDiffToolbarGroup:
                    // these aren't actually commands, but IDs of the command bars and groups
                    break;

                default:
                    break;
                }
            }

            return false;
        }

        private EditorDiffViewModel GetDiffViewModelToMoveTo(uint commandId, DiffMarginViewModelBase viewModel)
        {
            var lineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

            return (GitDiffMarginCommand) commandId == GitDiffMarginCommand.NextChange ?
                viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(model => model.LineNumber > lineNumber) :
                viewModel.DiffViewModels.OfType<EditorDiffViewModel>().LastOrDefault(model => model.LineNumber < lineNumber);
        }

        private bool TryGetMarginViewModel(out EditorDiffMarginViewModel viewModel)
        {
            viewModel = null;

            IWpfTextViewHost textViewHost = _editorAdaptersFactoryService.GetWpfTextViewHost(TextViewAdapter);
            if (textViewHost == null)
                return false;

            EditorDiffMargin margin = textViewHost.GetTextViewMargin(EditorDiffMargin.MarginNameConst) as EditorDiffMargin;
            if (margin == null)
                return false;

            viewModel = margin.VisualElement.DataContext as EditorDiffMarginViewModel;
            return viewModel != null;
        }

        private void ToggleCompareToIndex()
        {
            bool currentCompareToIndex = _textView.Options.GetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId);
            if (currentCompareToIndex)
                CompareToHead();
            else
                CompareToIndex();
        }

        private void CompareToIndex()
        {
            _textView.Options.SetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId, true);
        }

        private void CompareToHead()
        {
            _textView.Options.SetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId, false);
        }
    }
}
