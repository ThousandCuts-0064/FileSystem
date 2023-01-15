using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemNS
{
    public class TaskInfo
    {
        private CancellationTokenSource _cancellationTokenSource;
        public Task Task { get; private set; }
        public string Name { get; set; }
        public double Progress { get; set; }
        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;
        public bool IsRunning => Task != null && (!Task.IsCompleted || !Task.IsFaulted);

        public TaskInfo() => Reset();

        public override string ToString() => $"{Name}: {Progress:P}";

        public Task Run(Action action)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task = Task.Run(action, _cancellationTokenSource.Token);
            return Task;
        }

        public Task Cancel()
        {
            if (!IsRunning) return Task.CompletedTask;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            try { return Task; }
            finally { Reset(); }
        }

        private void Reset()
        {
            Name = "None";
            Progress = 1;
            Task = null;
            _cancellationTokenSource = null;
        }
    }
}
