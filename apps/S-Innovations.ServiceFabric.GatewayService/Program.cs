﻿using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Practices.Unity;
using Serilog;
using SInnovations.LetsEncrypt;
using SInnovations.ServiceFabric.GatewayService.Actors;
using SInnovations.ServiceFabric.GatewayService.Configuration;
using SInnovations.ServiceFabric.GatewayService.Services;
using SInnovations.ServiceFabric.Storage.Configuration;
using SInnovations.ServiceFabric.Storage.Extensions;
using SInnovations.ServiceFabric.Storage.Services;
using SInnovations.ServiceFabric.Unity;

namespace SInnovations.ServiceFabric.GatewayService
{




    public class Program
    {
        
        public static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
               //.WriteTo.ApplicationInsightsEvents("10e77ea7-1d38-40f7-901c-ef3c2e7d48ef")
               .WriteTo.ApplicationInsightsTraces("10e77ea7-1d38-40f7-901c-ef3c2e7d48ef", Serilog.Events.LogEventLevel.Debug)
               .MinimumLevel.Debug()
               .CreateLogger();


            using (var container = new UnityContainer().AsFabricContainer())
            {
                container.AddOptions();
                container.ConfigureLogging(new LoggerFactory().AddSerilog(log));

                container.ConfigureApplicationStorage();

                


                var keyvaultINfo = container.Resolve<KeyVaultSecretManager>();
                var configuration = new ConfigurationBuilder()
                    .AddAzureKeyVault(keyvaultINfo.KeyVaultUrl, keyvaultINfo.Client, keyvaultINfo)
                    .Build(container);        

                container.Configure<KeyVaultOptions>("KeyVault");

                container.WithLetsEncryptService(new LetsEncryptServiceOptions
                {
                    BaseUri = "https://acme-v01.api.letsencrypt.org"
                });

                container.WithStatelessService<NginxGatewayService>("GatewayServiceType");
                container.WithStatelessService<ApplicationStorageService>("ApplicationStorageServiceType");

                container.WithActor<GatewayServiceManagerActor>();
 

                Thread.Sleep(Timeout.Infinite);
            }
            
        }

       
    }
}
