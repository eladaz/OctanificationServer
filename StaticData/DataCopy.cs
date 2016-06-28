using System;
using System.Runtime.Serialization;

namespace OctanificationServer.StaticData
{
    [DataContract]
    public class DataCopy : IJSONable
    {
        [DataMember]
        public string str { get; set; }

    }
}
