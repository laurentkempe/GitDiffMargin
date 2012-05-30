#region using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using GitDiffMargin.Git;
using GitDiffMargin.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#endregion

namespace GitDiffMargin
{
    /// <summary>
    ///   A class detailing the margin's visual definition including both size and content.
    /// </summary>
    internal class GitDiffMargin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "GitDiffMargin";
        private bool _isDisposed = false;
        private IWpfTextView _textView;
        private ITextDocument _document;
        private IEnumerable<HunkRangeInfo> _hunkRangeInfos;

        /// <summary>
        ///   Creates a <see cref="GitDiffMargin" /> for a given <see cref="IWpfTextView" /> .
        /// </summary>
        /// <param name="textView"> The <see cref="IWpfTextView" /> to attach the margin to. </param>
        public GitDiffMargin(IWpfTextView textView)
        {
            _textView = textView;

            var lineCount = _textView.TextSnapshot.LineCount;
            var windowHeight = textView.ViewportHeight;

            var gitDiffBarControl = new GitDiffBarControl();

            Children.Add(gitDiffBarControl);

            _textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out _document);
            if (_document != null)
            {
                _document.FileActionOccurred += FileActionOccurred;
                GetGitDiffFor(_document.FilePath);
            }

            _textView.Closed += TextViewClosed;
        }

        private void FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentLoadedFromDisk) != 0 ||
                (e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                GetGitDiffFor(_document.FilePath);
            }
        }

        private void TextViewClosed(object sender, EventArgs e)
        {
            CleanUp();
        }

        private void GetGitDiffFor(string filename)
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
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var gitDiffParser = new GitDiffParser(output);
            IEnumerable<HunkRangeInfo> rangeInfos = gitDiffParser.Parse();

            lock (this)
            {
                _hunkRangeInfos = rangeInfos.ToList();
            }
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
                _textView = null;
            }
        }

        #region IWpfTextViewMargin Members

        /// <summary>
        ///   The <see cref="Sytem.Windows.FrameworkElement" /> that implements the visual representation of the margin.
        /// </summary>
        public System.Windows.FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        public double MarginSize
        {
            // Since this is a horizontal margin, its width will be bound to the width of the text view.
            // Therefore, its size is its height.
            get
            {
                ThrowIfDisposed();
                return this.ActualHeight;
            }
        }

        public bool Enabled
        {
            // The margin should always be enabled
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        /// <summary>
        ///   Returns an instance of the margin if this is the margin that has been requested.
        /// </summary>
        /// <param name="marginName"> The name of the margin requested </param>
        /// <returns> An instance of GitDiffMargin or null </returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == GitDiffMargin.MarginName) ? (IWpfTextViewMargin) this : null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(MarginName);
        }
    }
}