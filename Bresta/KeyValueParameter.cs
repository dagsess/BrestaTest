namespace BrestaTest.Bresta
{
    public class KeyValueParameter
    {
        public string Comment { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public static KeyValueParameter Parse(string s)
        {
            KeyValueParameter parameter = new KeyValueParameter { };
            var spl = s.Split(':');

            if (spl.Length > 0)
            {
                parameter.Key = spl[0].Trim();
            }
            if (spl.Length > 1)
            {
                parameter.Value = spl[1].Trim();
            }
            return parameter;
        }

        public KeyValueParameter Clone()
        {
            return new KeyValueParameter { Comment = Comment, Key = Key, Value = Value };
        }
    }
}
