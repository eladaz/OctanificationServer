using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OctanificationServer.StaticData
{
    [DataContract]
    public class Member : IJSONable
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Entities { get; set; }


        [DataMember]
        public List<string> Following { get; set; }


        [DataMember]
        public List<string> Followed { get; set; }


        [DataMember]
        public string Url { get; set; }
    }
}
