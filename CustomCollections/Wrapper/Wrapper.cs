using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCollections
{
    public class Wrapper<T> : IWrapper<T>, IReadOnlyWrapper<T>
    {
        public virtual T Item { get; set; }
    }
}
