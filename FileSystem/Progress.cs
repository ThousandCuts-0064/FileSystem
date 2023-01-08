using CustomCollections;

namespace FileSystemNS
{
    public class Progress : ReadOnlyWrapper<double>
    {
        public Progress(Wrapper<double> wrapper) : base(wrapper) { }
    }
}
