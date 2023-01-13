using System;
using CustomCollections;

namespace FileSystemNS
{
    public class ReadOnlyTaskProgress
    {
        private readonly TaskProgress _task;

        public string Name => _task.Name;
        public double Progress => _task.Progress;

        public ReadOnlyTaskProgress(TaskProgress task) => _task = task ?? throw new ArgumentNullException(nameof(task));

        public override string ToString() => _task.ToString();
    }
}
