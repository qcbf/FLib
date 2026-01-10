//=================================================={By Qcbf|qcbf@qq.com|11/27/2024 12:09:02 PM}==================================================

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
            Log.GlobalOutputHandler -= OnOutput;
            AppDomain.CurrentDomain.ProcessExit -= OnSystemError;
            AppDomain.CurrentDomain.UnhandledException -= OnSystemError;
            TaskScheduler.UnobservedTaskException -= OnSystemError;
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
        protected void OnSystemError(object s, object e)
        {
            var text = $"{e.GetType().Name}|{DateTime.Now:s} Process Exited {s} {e}";
            Write(Log.Error, text);
            Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual LogWriter Start()
        {
            var thread = new Thread(Receiving)
            {
                Name = nameof(LogWriter),
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };
            thread.Start();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnOutput(Log log, string text)
        {
            Logs.Add((log, text));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Receiving()
        {
            Write(Log.Info, $"==================== {DateTime.Now:s} ===================={Environment.NewLine}");
            while (true)
            {
                if (!Logs.TryTake(out var log))
                {
                    Flush();
                    log = Logs.Take();
                }
                Write(log.Item1, log.Item2);
            }
        }

        public abstract void Write(Log log, string text);
        public abstract void Flush();
    }
}
