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
                        IOleCommandTarget commandTarget = MarginCore.TextView.Properties.GetProperty<GitDiffMarginCommandHandler>(typeof(GitDiffMarginCommandHandler));

                        IVsToolbarTrayHost toolbarTrayHost;
                        ErrorHandler.ThrowOnFailure(uiShell.CreateToolbarTray(commandTarget, out toolbarTrayHost));

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
                MarginCore.GitCommands.StartExternalDiff(document, MarginCore.TextView.Options.GetOptionValue(GitDiffMarginTextViewOptions.CompareToIndexId));
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
    }
}