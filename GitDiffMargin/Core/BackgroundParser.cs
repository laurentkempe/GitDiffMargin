/* The MIT License
 *
 * Copyright (c) 2013 Sam Harwell, Tunnel Vision Labs, LLC
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;

namespace GitDiffMargin.Core
{
    public abstract class BackgroundParser : IDisposable
    {
        private readonly WeakReference<ITextBuffer> _textBuffer;
        private readonly TaskScheduler _taskScheduler;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly Timer _timer;

        private TimeSpan _reparseDelay;
        private DateTimeOffset _lastEdit;
        private bool _dirty;
        private int _parsing;
        private bool _disposed;

        public event EventHandler<ParseResultEventArgs> ParseComplete;

        protected BackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
        {
            if (textBuffer == null)
                throw new ArgumentNullException("textBuffer");
            if (taskScheduler == null)
                throw new ArgumentNullException("taskScheduler");
            if (textDocumentFactoryService == null)
                throw new ArgumentNullException("textDocumentFactoryService");

            _textBuffer = new WeakReference<ITextBuffer>(textBuffer);
            _taskScheduler = taskScheduler;
            _textDocumentFactoryService = textDocumentFactoryService;

            textBuffer.PostChanged += TextBufferPostChanged;

            _dirty = true;
            _reparseDelay = TimeSpan.FromMilliseconds(1500);
            _timer = new Timer(ParseTimerCallback, null, _reparseDelay, _reparseDelay);
            _lastEdit = DateTimeOffset.MinValue;
        }

        public ITextBuffer TextBuffer
        {
            get
            {
                return _textBuffer.Target;
            }
        }

        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        public TimeSpan ReparseDelay
        {
            get
            {
                return _reparseDelay;
            }

            set
            {
                TimeSpan originalDelay = _reparseDelay;
                try
                {
                    _reparseDelay = value;
                    _timer.Change(value, value);
                }
                catch (ArgumentException)
                {
                    _reparseDelay = originalDelay;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual string Name
        {
            get
            {
                return string.Empty;
            }
        }

        protected ITextDocumentFactoryService TextDocumentFactoryService
        {
            get
            {
                return _textDocumentFactoryService;
            }
        }

        public void RequestParse(bool forceReparse)
        {
            TryReparse(forceReparse);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ITextBuffer textBuffer = TextBuffer;
                if (textBuffer != null)
                    textBuffer.PostChanged -= TextBufferPostChanged;

                _timer.Dispose();
            }

            _disposed = true;
        }

        protected abstract void ReParseImpl();

        protected virtual void OnParseComplete(ParseResultEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            var t = ParseComplete;
            if (t != null)
                t(this, e);
        }

        protected void MarkDirty(bool resetTimer)
        {
            this._dirty = true;
            this._lastEdit = DateTimeOffset.Now;

            if (resetTimer)
                _timer.Change(_reparseDelay, _reparseDelay);
        }

        private void TextBufferPostChanged(object sender, EventArgs e)
        {
            MarkDirty(true);
        }

        private void ParseTimerCallback(object state)
        {
            if (TextBuffer == null)
            {
                Dispose();
                return;
            }

            TryReparse(_dirty);
        }

        private void TryReparse(bool forceReparse)
        {
            if (!_dirty && !forceReparse)
                return;

            if (DateTimeOffset.Now - _lastEdit < ReparseDelay)
                return;

            if (Interlocked.CompareExchange(ref _parsing, 1, 0) == 0)
            {
                try
                {
                    Task task = Task.Factory.StartNew(ReParse, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
                    task.ContinueWith(_ => _parsing = 0);
                }
                catch
                {
                    _parsing = 0;
                    throw;
                }
            }
        }

        private void ReParse()
        {
            try
            {
                _dirty = false;
                ReParseImpl();
            }
            catch (Exception ex)
            {
                // Failure to handle TPL exception in .NET 4 would bring down Visual Studio 2010
                if (ErrorHandler.IsCriticalException(ex))
                    throw;
            }
        }
    }
}
