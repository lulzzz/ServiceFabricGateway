﻿using SInnovations.ServiceFabric.Gateway.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.Gateway.Common.Model
{
    [DataContract]
    public class CertHttpChallengeInfo
    {
        [DataMember]
        public string KeyAuthString { get; set; }

        [DataMember]
        public string Location { get; private set; }
        [DataMember]
        public string Token { get; set; }

        public CertHttpChallengeInfo SetLocation(string location)
        {
            return new CertHttpChallengeInfo { Location = location, Token = Token, KeyAuthString = KeyAuthString };
        }
    }
    [DataContract]
    public class CertGenerationState
    {
        public CertGenerationState()
        {
            RunAt = DateTimeOffset.UtcNow;
        }
        public CertGenerationState(bool completed) : this()
        {
            Completed = completed;

        }

        [DataMember]
        public bool Completed { get; private set; }

        public CertGenerationState Complete()
        {
            return new CertGenerationState
            {
                Completed = true,
                HostName = HostName,
                SslOptions = new SslOptions { Enabled = SslOptions.Enabled, SignerEmail = SslOptions.SignerEmail, UseHttp01Challenge = SslOptions.UseHttp01Challenge },
                RunAt = RunAt,
                HttpChallengeInfo = HttpChallengeInfo
            };
        }
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public SslOptions SslOptions { get; set; }
        [DataMember]
        public DateTimeOffset? RunAt { get; private set; }
        [DataMember]
        public CertHttpChallengeInfo HttpChallengeInfo { get; private set; }

        public CertGenerationState SetCertHttpChallengeLocation(string location)
        {
            return SetCertHttpChallengeInfo(HttpChallengeInfo.SetLocation(location));
            
        }
        public CertGenerationState SetCertHttpChallengeInfo(CertHttpChallengeInfo certHttpChallengeInfo)
        {
            return new CertGenerationState
            {
                Completed = Completed,
                HostName = HostName,
                SslOptions = new SslOptions { Enabled = SslOptions.Enabled, SignerEmail = SslOptions.SignerEmail, UseHttp01Challenge = SslOptions.UseHttp01Challenge },
                RunAt = RunAt,
                HttpChallengeInfo = certHttpChallengeInfo
            };
        }
    }
}
