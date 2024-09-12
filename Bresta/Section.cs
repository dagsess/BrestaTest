using System.Collections.Generic;
using System.Linq;

namespace BrestaTest.Bresta
{
    public class Section
    {
        public string Name { get; set; } = string.Empty;
        public string Comment { set; get; }

        public Dictionary<string , KeyValueParameter> Parameters { get; set;} = new Dictionary<string, KeyValueParameter>();

        public void AddParameter(KeyValueParameter keyValue)
        {
            if(Parameters.ContainsKey(keyValue.Key))
            {
                if (Parameters[keyValue.Key].Value != string.Empty) Parameters[keyValue.Key].Value += "\n";
                Parameters[keyValue.Key].Value += keyValue.Value;
            }
            else
            {
                Parameters.Add(keyValue.Key, keyValue);
            }
        }

        public Section Clone()
        {
            return new Section { Comment = Comment, Name = Name, Parameters = Parameters.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()) };
        }

        public string ToHeaderStyleString()
        {
            List<string> lines = new List<string>();

            foreach (var kv in Parameters)
            {
                lines.Add($"{kv.Key}: {kv.Value.Value}");
            }

            return $"{{{string.Join("; ", lines)}}}";
        }

    }
}
