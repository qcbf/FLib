//=================================================={By Qcbf|qcbf@qq.com|11/27/2024 12:09:02 PM}==================================================

#nullable enable

using FLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FLib
{
    public abstract class LogWriter : IDisposable
    {
        public readonly BlockingCollection<(Log, string)> Logs;
        private Thread? _thread;
        private int _started;
        private int _disposed;

        protected LogWriter(int capacity = 128)
        {
            Logs = new BlockingCollection<(Log, string)>(capacity);
            AppDomain.CurrentDomain.ProcessExit += OnSystemError;
            AppDomain.CurrentDomain.UnhandledException += OnSystemError;
            TaskScheduler.UnobservedTaskException += OnSystemError;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            Log.GlobalOutputHandler -= OnOutput;
            AppDomain.CurrentDomain.ProcessExit -= OnSystemError;
            AppDomain.CurrentDomain.UnhandledException -= OnSystemError;
            TaskScheduler.UnobservedTaskException -= OnSystemError;
            Logs.CompleteAdding();
            if (_thread is { } thread && thread != Thread.CurrentThread)
                thread.Join();
            Flush();
            Logs.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public LogWriter RegisterToLog()
        {
            Log.GlobalOutputHandler += OnOutput;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        protected void OnSystemError(object? s, object? e)
        {
            var text = $"{e?.GetType().Name ?? nameof(Exception)}|{DateTime.Now:s} Process Exited {s} {e}";
            Write(Log.Error!, text);
            Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual LogWriter Start()
        {
            if (Interlocked.Exchange(ref _started, 1) != 0)
                return this;
            var thread = new Thread(Receiving)
            {
                Name = nameof(LogWriter),
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };
            _thread = thread;
            thread.Start();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnOutput(Log log, string text)
        {
            if (Volatile.Read(ref _disposed) == 0)
                Logs.TryAdd((log, text));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Receiving()
        {
            Write(Log.Info!, $"==================== {DateTime.Now:s} ===================={Environment.NewLine}");
            while (true)
            {
                if (!Logs.TryTake(out var log, 1000))
                {
                    Flush();
                    if (Logs.IsCompleted)
                        break;
                    continue;
                }
                Write(log.Item1, log.Item2);
            }
            Flush();
        }

        public abstract void Write(Log log, string text);
        public abstract void Flush();
    }
}
