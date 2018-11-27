using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hummingbird.TestFramework.Services
{
    [Serializable]
    public class AttributeIndexValue
    {
        public string Attribute { get; set; }
        public string Count { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    public class LdapObject
    {
        public string Name { get; set; }
        public List<AttributeIndexValue> Attributes { get; set; } = new List<AttributeIndexValue>();

        public override string ToString()
        {
            return Name;
        }
    }
}
