#region using

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Core;
using GitDiffMargin.Git;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget;
using OLECMD = Microsoft.VisualStudio.OLE.Interop.OLECMD;
using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffViewModel : DiffViewModel
    {
        private bool _isDiffTextVisible;
        private bool _showPopup;
        private bool _reverted;
        private ICommand _copyOldTextCommand;
        private ICommand _rollbackCommand;
        private ICommand _showPopUpCommand;

        private EditorDiffMarginViewModel _diffMarginViewModel;
        private IVsToolbarTrayHost _toolbarTrayHost;

        internal EditorDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore, updateDiffDimensions)
        {
            ShowPopup = false;

            DiffText = GetDiffText();

            IsDiffTextVisible = GetIsDiffTextVisible();

            UpdateDimensions();
        }

        private bool GetIsDiffTextVisible()
        {
            return HunkRangeInfo.IsDeletion || HunkRangeInfo.IsModification;
        }

        private string GetDiffText()
        {
            if (HunkRangeInfo.OriginalText != null && HunkRangeInfo.OriginalText.Any())
            {
                return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion ? String.Join(Environment.NewLine, HunkRangeInfo.OriginalText) : string.Empty;
            }

            return string.Empty;
        }

        protected override void UpdateDimensions()
        {
            if (_reverted) return;

            base.UpdateDimensions();
        }

        public FontFamily FontFamily
        {
            get { return MarginCore.FontFamily; }
        }

        public FontStretch FontStretch
        {
            get { return MarginCore.FontStretch; }
        }

        public FontStyle FontStyle
        {
            get
            {
                return MarginCore.FontStyle;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return MarginCore.FontWeight;
            }
        }

        public double FontSize
        {
            get
            {
                return MarginCore.FontSize;
            }
        }

        //public double MaxWidth
        //{
        //    get
        //    {
        //        return _textView.ViewportWidth;
        //    }
        //}

        //public double MaxHeight
        //{
        //    get
        //    {
        //        return Math.Max(_textView.ViewportHeight * 2.0 / 3.0, 400);
        //    }
        //}

        public Brush Background
        {
            get
            {
                return MarginCore.Background;
            }
        }

        public Brush Foreground
        {
            get
            {
                return MarginCore.Foreground;
            }
        }

        public ICommand ShowPopUpCommand
        {
            get { return _showPopUpCommand ?? (_showPopUpCommand = new RelayCommand<EditorDiffMarginViewModel>(ShowPopUp)); }
        }

        public object ToolBarTray
        {
            get
            {
                if (_toolbarTrayHost == null)
                    return null;

                IVsUIElement toolbarTray;
                ErrorHandler.ThrowOnFailure(_toolbarTrayHost.GetToolbarTray(out toolbarTray));

                object uiObject;
                ErrorHandler.ThrowOnFailure(toolbarTray.GetUIObject(out uiObject));

                IVsUIWpfElement wpfElement = uiObject as IVsUIWpfElement;

                object frameworkElement;
                ErrorHandler.ThrowOnFailure(wpfElement.GetFrameworkElement(out frameworkElement));

                return frameworkElement;
            }
        }

        public bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                if (value == _showPopup) return;
                _showPopup = value;
                if (value)
                {
                    IVsUIShell4 uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell4;
                    if (uiShell != null)
                    {
                        IVsToolbarTrayHost toolbarTrayHost;
                        ErrorHandler.ThrowOnFailure(uiShell.CreateToolbarTray(new ToolbarCommandHandler(this), out toolbarTrayHost));

                        Guid toolBarGuid = typeof(GitDiffMarginCommand).GUID;
                        ErrorHandler.ThrowOnFailure(toolbarTrayHost.AddToolbar(ref toolBarGuid, (int)GitDiffMarginCommand.GitDiffToolbar));

                        _toolbarTrayHost = toolbarTrayHost;
                    }
                }
                else
                {
                    var toolbarTrayHost = Interlocked.Exchange(ref _toolbarTrayHost, null);
                    if (toolbarTrayHost != null)
                        ErrorHandler.CallWithCOMConvention(() => toolbarTrayHost.Close());
                }

                RaisePropertyChanged(() => ShowPopup);
                RaisePropertyChanged(() => ToolBarTray);
            }
        }

        public string DiffText { get; private set; }

        public bool IsDiffTextVisible
        {
            get { return _isDiffTextVisible; }
            set
            {
                if (value == _isDiffTextVisible) return;
                _isDiffTextVisible = value;
                RaisePropertyChanged(() => IsDiffTextVisible);
            }
        }

        private ICommand _showDifferenceCommand;

        public ICommand ShowDifferenceCommand
        {
            get { return _showDifferenceCommand ?? (_showDifferenceCommand = new RelayCommand(ShowDifference, ShowDifferenceCanExecute)); }
        }

        private bool ShowDifferenceCanExecute()
        {
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void ShowDifference()
        {
            var document = MarginCore.GetTextDocument();
            if (document != null)
            {
                MarginCore.GitCommands.StartExternalDiff(document);
            } 
        }

        public ICommand CopyOldTextCommand
        {
            get { return _copyOldTextCommand ?? (_copyOldTextCommand = new RelayCommand(CopyOldText, CopyOldTextCanExecute)); }
        }

        public ICommand RollbackCommand
        {
            get { return _rollbackCommand ?? (_rollbackCommand = new RelayCommand(Rollback, RollbackCanExecute)); }
        }

        private bool CopyOldTextCanExecute()
        {
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion;
        }

        private void CopyOldText()
        {
            Clipboard.SetText(DiffText);
            ShowPopup = false;
        }

        private bool RollbackCanExecute()
        {
            if (HunkRangeInfo.SuppressRollback)
                return false;

            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void Rollback()
        {
            if (!MarginCore.RollBack(HunkRangeInfo)) return;

            // immediately hide the change
            _reverted = true;
            ShowPopup = false;
            IsVisible = false;

            // Make sure the view is focused afterwards
            MarginCore.TextView.VisualElement.Focus();
        }

        private void ShowPopUp(EditorDiffMarginViewModel diffMarginViewModel)
        {
            _diffMarginViewModel = diffMarginViewModel;
            ShowPopup = true;
        }

        private class ToolbarCommandHandler : IOleCommandTarget
        {
            private readonly EditorDiffViewModel _viewModel;

            public ToolbarCommandHandler(EditorDiffViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            {
                if (pguidCmdGroup == typeof(GitDiffMarginCommand).GUID)
                {
                    switch ((GitDiffMarginCommand)prgCmds[0].cmdID)
                    {
                    case GitDiffMarginCommand.PreviousChange:
                        if (_viewModel._diffMarginViewModel == null)
                        {
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            return VSConstants.S_OK;
                        }

                        prgCmds[0].cmdf = ToCommandFlags(_viewModel._diffMarginViewModel.PreviousChangeCommand.CanExecute(_viewModel));
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.NextChange:
                        if (_viewModel._diffMarginViewModel == null)
                        {
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            return VSConstants.S_OK;
                        }

                        prgCmds[0].cmdf = ToCommandFlags(_viewModel._diffMarginViewModel.NextChangeCommand.CanExecute(_viewModel));
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.RollbackChange:
                        prgCmds[0].cmdf = ToCommandFlags(_viewModel.RollbackCanExecute());
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.ShowDiff:
                        prgCmds[0].cmdf = ToCommandFlags(_viewModel.ShowDifferenceCanExecute());
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.CopyOldText:
                        prgCmds[0].cmdf = ToCommandFlags(_viewModel.CopyOldTextCanExecute());
                        return VSConstants.S_OK;

                    default:
                        prgCmds[0].cmdf = 0;
                        return VSConstants.S_OK;
                    }
                }

                prgCmds[0].cmdf = 0;
                return VSConstants.S_OK;
            }

            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                if (pguidCmdGroup == typeof(GitDiffMarginCommand).GUID)
                {
                    switch ((GitDiffMarginCommand)nCmdID)
                    {
                    case GitDiffMarginCommand.PreviousChange:
                        if (_viewModel._diffMarginViewModel == null)
                            return (int)OleConstants.MSOCMDERR_E_NOTSUPPORTED;

                        _viewModel._diffMarginViewModel.PreviousChangeCommand.Execute(_viewModel);
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.NextChange:
                        if (_viewModel._diffMarginViewModel == null)
                            return (int)OleConstants.MSOCMDERR_E_NOTSUPPORTED;

                        _viewModel._diffMarginViewModel.NextChangeCommand.Execute(_viewModel);
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.RollbackChange:
                        _viewModel.Rollback();
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.ShowDiff:
                        _viewModel.ShowDifference();
                        return VSConstants.S_OK;

                    case GitDiffMarginCommand.CopyOldText:
                        _viewModel.CopyOldText();
                        return VSConstants.S_OK;

                    default:
                        return (int)OleConstants.MSOCMDERR_E_NOTSUPPORTED;
                    }
                }

                return (int)OleConstants.MSOCMDERR_E_UNKNOWNGROUP;
            }

            private static uint ToCommandFlags(bool canExecute)
            {
                if (canExecute)
                    return (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                else
                    return (uint)OLECMDF.OLECMDF_SUPPORTED;
            }
        }
    }
}