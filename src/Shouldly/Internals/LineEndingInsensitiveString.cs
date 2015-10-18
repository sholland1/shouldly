namespace Shouldly.Internals
{
    internal class LineEndingInsensitiveString
    {
        private string _value;
        public LineEndingInsensitiveString(string s)
        {
            _value = s;
        }
        public override string ToString()
        {
            return _value.NormalizeLineEndings();
        }
    }
}
