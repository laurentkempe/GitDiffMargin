#region using

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using GitDiffMargin.Git;
using System.Linq;

#endregion

namespace GitDiffMargin.ViewModel
{
    internal class EditorDiffViewModel : DiffViewModel
    {
        private readonly IMarginCore _marginCore;
        private readonly Action<EditorDiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private bool _isDiffTextVisible;
        private bool _showPopup;
        private bool _reverted;
        private ICommand _copyOldTextCommand;
        private ICommand _rollbackCommand;
        private ICommand _showPopUpCommand;

        internal EditorDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<EditorDiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore)
        {
            HunkRangeInfo = hunkRangeInfo;
            _marginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            _marginCore.BrushesChanged += HandleBrushesChanged;

            ShowPopup = false;

            UpdateDimensions();

            DiffText = GetDiffText();

            IsDiffTextVisible = GetIsDiffTextVisible();
        }

        private void HandleBrushesChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => DiffBrush);
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

            _updateDiffDimensions(this, HunkRangeInfo);
        }

        public FontFamily FontFamily
        {
            get { return _marginCore.FontFamily; }
        }

        public FontStretch FontStretch
        {
            get { return _marginCore.FontStretch; }
        }

        public FontStyle FontStyle
        {
            get
            {
                return _marginCore.FontStyle;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return _marginCore.FontWeight;
            }
        }

        public double FontSize
        {
            get
            {
                return _marginCore.FontSize;
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
                return _marginCore.Background;
            }
        }

        public Brush Foreground
        {
            get
            {
                return _marginCore.Foreground;
            }
        }

        public ICommand ShowPopUpCommand
        {
            get { return _showPopUpCommand ?? (_showPopUpCommand = new RelayCommand(ShowPopUp)); }
        }

        public bool ShowPopup
        {
            get { return _showPopup; }
            set
            {
                if (value == _showPopup) return;
                _showPopup = value;
                RaisePropertyChanged(() => ShowPopup);
            }
        }

        public string DiffText { get; private set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value;
            RaisePropertyChanged(() => IsVisible);}
        }

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

        private bool _isVisible;

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
            var document = _marginCore.GetTextDocument();
            if (document != null)
            {
                _marginCore.GitCommands.StartExternalDiff(document);
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
            return HunkRangeInfo.IsModification || HunkRangeInfo.IsDeletion || HunkRangeInfo.IsAddition;
        }

        private void Rollback()
        {
            if (!_marginCore.RollBack(HunkRangeInfo)) return;

            // immediately hide the change
            _reverted = true;
            ShowPopup = false;
            IsVisible = false;
        }

        private void ShowPopUp()
        {
            ShowPopup = true;
        }
    }
}