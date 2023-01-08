namespace CustomCollections
{
    public class StringBuilder_
    {
        private readonly LinkedList_<string> _strings = new LinkedList_<string>();
        public int CharCount { get; private set; }

        public StringBuilder_ Append(string str)
        {
            _strings.AddLast(str);
            CharCount += str.Length;
            return this;
        }

        public StringBuilder_ Prepend(string str)
        {
            _strings.AddFirst(str);
            CharCount += str.Length;
            return this;
        }

        public override string ToString()
        {
            char[] chars = new char[CharCount];
            int index = 0;
            for (var node = _strings.First; !(node is null); node = node.Next)
            {
                string str = node.Value;
                for (int c = 0; c < str.Length; c++)
                    chars[index++] = str[c];
            }
            return new string(chars);
        }

        public StringBuilder_ Clear()
        {
            _strings.Clear();
            CharCount = 0;
            return this;
        }
    }
}
