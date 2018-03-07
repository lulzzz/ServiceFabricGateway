﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.Gateway.Model
{
    [DataContract]
    public class ProxyPassCacheOptions
    {
        [DataMember]
        public bool Enabled { get; set; }
    }
    [DataContract]
    public class GatewayServiceRegistrationData
    {
        [DataMember]
        public string ReverseProxyLocation { get; set; }
        [DataMember]
        public string BackendPath { get; set; }
        [DataMember]
        public string IPAddressOrFQDN { get; set; }

        [DataMember]
        public string ServerName { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public SslOptions Ssl { get; set; } = new SslOptions();

        [DataMember]
        public Uri ServiceName { get; set; }

        [DataMember]
        public string ServiceVersion { get; set; }

        [DataMember]
        public ProxyPassCacheOptions CacheOptions { get; set; } = new ProxyPassCacheOptions();
        [DataMember]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
