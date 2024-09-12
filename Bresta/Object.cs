using System.Collections.Generic;
using System.Linq;

namespace BrestaTest.Bresta
{
    public class Object
    {
        public Dictionary<string, Section> Sections { get; set; } = new Dictionary<string, Section>();

        public KeyValueParameter FindKeyValue(string paramName)
        {
            foreach (var section in Sections)
            {
                if(section.Value.Parameters.ContainsKey(paramName))
                {
                    return section.Value.Parameters[paramName];
                }
            }

            return null;
        }

        public void SetParameter(string paramName, string paramValue)
        {
            foreach (var section in Sections)
            {
                if (section.Value.Parameters.ContainsKey(paramName))
                {
                    section.Value.Parameters[paramName].Value = paramValue;
                }
            }
        }

        public VisualObject VisualObject { get; set; }

        public Object Clone()
        {
            return new Object
            {
                Sections =  Sections.ToDictionary(kv=>kv.Key, kv=>kv.Value.Clone()),
                VisualObject = VisualObject.Clone()
            };
        }

    }
}
