namespace CustomCollections
{
    public class StringBuilder_
    {
        private readonly List_<string> _strings = new List_<string>();
        public int CharCount { get; private set; }

        public StringBuilder_ Append(string str)
        {
            _strings.Add(str);
            CharCount += str.Length;
            return this;
        }

        public StringBuilder_ Prepend(string str)
        {
            _strings.Insert(0, str);
            CharCount += str.Length;
            return this;
        }

        public override string ToString()
        {
            char[] chars = new char[CharCount];
            int index = 0;
            for (int s = 0; s < _strings.Count; s++)
            {
                string str = _strings[s];
                for (int c = 0; c < str.Length; c++)
                    chars[index++] = str[c];
            }
            return new string(chars);
        }

        public StringBuilder_ Clear()
        {
            _strings.Clear();
            return this;
        }
    }
}
