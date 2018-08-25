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
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffViewModel : DiffViewModel
    {
        private ICommand _copyOldTextCommand;

        private EditorDiffMarginViewModel _diffMarginViewModel;
        private bool _isDiffTextVisible;
        private bool _reverted;
        private ICommand _rollbackCommand;

        private ICommand _showDifferenceCommand;
        private bool _showPopup;
        private ICommand _showPopUpCommand;
        private IVsToolbarTrayHost _toolbarTrayHost;

        internal EditorDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore,
            Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore, updateDiffDimensions)
        {
            ShowPopup = false;

            DiffText = GetDiffText();

            IsDiffTextVisible = GetIsDiffTextVisible();

            UpdateDimensions();
        }

        public FontFamily FontFamily => MarginCore.FontFamily;

        public FontStretch FontStretch => MarginCore.FontStretch;

        public FontStyle FontStyle => MarginCore.FontStyle;

        public FontWeight FontWeight => MarginCore.FontWeight;

        public double FontSize => MarginCore.FontSize;

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

        public Brush Background => MarginCore.Background;

        public Brush Foreground => MarginCore.Foreground;

        public ICommand ShowPopUpCommand => _showPopUpCommand ??
                                            (_showPopUpCommand =
                                                new RelayCommand<EditorDiffMarginViewModel>(ShowPopUp));

        public object ToolBarTray
        {
            get
            {
                if (_toolbarTrayHost == null)
                    return null;

                ErrorHandler.ThrowOnFailure(_toolbarTrayHost.GetToolbarTray(out var toolbarTray));

                ErrorHandler.ThrowOnFailure(toolbarTray.GetUIObject(out var uiObject));

                var wpfElement = uiObject as IVsUIWpfElement;

                ErrorHandler.ThrowOnFailure(wpfElement.GetFrameworkElement(out var frameworkElement));

                return frameworkElement;
            }
        }

        public bool ShowPopup
        {
            get => _showPopup;
            set
            {
                if (value == _showPopup) return;
                _showPopup = value;
                if (value)
                {
                    if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell4 uiShell)
                    {
                        IOleCommandTarget commandTarget =
                            MarginCore.TextView.Properties.GetProperty<GitDiffMarginCommandHandler>(
                                typeof(GitDiffMarginCommandHandler));

                        ErrorHandler.ThrowOnFailure(uiShell.CreateToolbarTray(commandTarget, out var toolbarTrayHost));

                        var toolBarGuid = typeof(GitDiffMarginCommand).GUID;
                        ErrorHandler.ThrowOnFailure(toolbarTrayHost.AddToolbar(ref toolBarGuid,
                            (int) GitDiffMarginCommand.GitDiffToolbar));

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

        public string DiffText { get; }

        public bool IsDiffTextVisible
        {
            get => _isDiffTextVisible;
            set
            {
                if (value == _isDiffTextVisible) return;
                _isDiffTextVisible = value;
                RaisePropertyChanged(() => IsDiffTextVisible);
            }
        }

        public ICommand ShowDifferenceCommand => _showDifferenceCommand ??
                                                 (_showDifferenceCommand = new RelayCommand(ShowDifference,
                                                     ShowDifferenceCanExecute));

        public ICommand CopyOldTextCommand => _copyOldTextCommand ??
                                              (_copyOldTextCommand =
                                                  new RelayCommand(CopyOldText, CopyOldTextCanExecute));

        public ICommand RollbackCommand =>
            _rollbackCommand ?? (_rollbackCommand = new RelayCommand(Rollback, RollbackCanExecute));

        private bool GetIsDiffTextVisible()
        {
            return HunkRangeInfo.IsDeletion || HunkRangeInfo.IsModification;
        }

        private string GetDiffText()
        {
            if (HunkRangeInfo.OriginalText != null && HunkRangeInfo.OriginalText.Any())
                return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion
                    ? string.Join(Environment.NewLine, HunkRangeInfo.OriginalText)
                    : string.Empty;

            return string.Empty;
        }

        protected override void UpdateDimensions()
        {
            if (_reverted) return;

            base.UpdateDimensions();
        }

        private bool ShowDifferenceCanExecute()
        {
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void ShowDifference()
        {
            var document = MarginCore.GetTextDocument();
            if (document != null) MarginCore.GitCommands.StartExternalDiff(document, MarginCore.OriginalPath);
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
    }
}