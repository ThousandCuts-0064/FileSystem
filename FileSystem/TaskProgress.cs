using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemNS
{
    public class TaskProgress
    {
        public string Name { get; set; }
        public double Progress { get; set; }

        public TaskProgress()
        {
            Name = "None";
            Progress = 1;
        }

        public override string ToString() => $"{Name}: {Progress:P}";
    }
}
