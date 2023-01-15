using System;
using CustomCollections;

namespace FileSystemNS
{
    public class ReadOnlyTaskInfo
    {
        private readonly TaskInfo _taskInfo;

        public string Name => _taskInfo.Name;
        public double Progress => _taskInfo.Progress;
        public bool IsRunning => _taskInfo.IsRunning;

        public ReadOnlyTaskInfo(TaskInfo task) => _taskInfo = task ?? throw new ArgumentNullException(nameof(task));

        public override string ToString() => _taskInfo.ToString();
    }
}
