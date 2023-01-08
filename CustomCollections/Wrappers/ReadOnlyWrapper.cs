using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCollections
{
    public class ReadOnlyWrapper<T> : IReadOnlyWrapper<T>
    {
        private readonly Wrapper<T> _wrapper;
        public T Value => _wrapper.Value;

        public ReadOnlyWrapper(Wrapper<T> wrapper) =>
            _wrapper = wrapper;
    }
}
