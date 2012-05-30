#region using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using GalaSoft.MvvmLight;
using GitDiffMargin.Git;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin.ViewModel
{
    public class DiffMarginViewModel : ViewModelBase
    {
        private readonly IWpfTextView _textView;
        private ITextDocument _document;

        public DiffMarginViewModel(IWpfTextView textView)
        {
            _textView = textView;
            DiffViewModels = new ObservableCollection<DiffViewModel>();

            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof (ITextDocument), out _document);
            if (_document != null)
            {
                _document.FileActionOccurred += FileActionOccurred;
                UpdateDiffViewModels();
            }

            _textView.Closed += TextViewClosed;
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; set; }

        private void UpdateDiffViewModels()
        {
            var rangeInfos = GetGitDiffFor(_document.FilePath);

            DiffViewModels.Clear();

            foreach (var hunkRangeInfo in rangeInfos)
            {
                DiffViewModels.Add(new DiffViewModel(hunkRangeInfo, _textView));
            }
        }

        private IEnumerable<HunkRangeInfo> GetGitDiffFor(string filename)
        {
            var p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = @"C:\@Tools\Development\Git\Git\bin\git.exe";
            p.StartInfo.Arguments = string.Format(@" diff --unified=0 {0}", Path.GetFileName(filename));
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var gitDiffParser = new GitDiffParser(output);
            return gitDiffParser.Parse();
        }

        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentLoadedFromDisk) != 0 ||
                (e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                UpdateDiffViewModels();
            }
        }

        private void TextViewClosed(object sender, EventArgs e)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            if (_document == null)
            {
                _document.FileActionOccurred -= FileActionOccurred;
                _document = null;
            }

            if (_textView != null)
            {
                _textView.Closed -= TextViewClosed;
            }
        }
    }
}