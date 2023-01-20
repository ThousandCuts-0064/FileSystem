using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystemNS;

namespace UI
{
    public class Shortcut
    {
        public Action Action { get; }
        public string Name { get; }

        public Shortcut(string name, Action action)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override string ToString() => Name;
    }
}
