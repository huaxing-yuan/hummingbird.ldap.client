using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Hummingbird.TestFramework.Services
{
    [Serializable]
    [DataContract]
    public class AttributeIndexValue
    {
        [DataMember]
        public string Attribute { get; set; }
        [DataMember]
        public string Count { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [Serializable]
    [DataContract]
    public class LdapObject
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ObservableCollection<AttributeIndexValue> Attributes { get; set; } = new ObservableCollection<AttributeIndexValue>();


        public override string ToString()
        {
            return Name;
        }
    }
}
