﻿using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unity;
using Serilog;
using SInnovations.LetsEncrypt;
using SInnovations.ServiceFabric.GatewayService.Actors;
using SInnovations.ServiceFabric.GatewayService.Configuration;
using SInnovations.ServiceFabric.GatewayService.Services;
using SInnovations.ServiceFabric.Storage.Configuration;
using SInnovations.ServiceFabric.Storage.Extensions;
using SInnovations.ServiceFabric.Storage.Services;
using SInnovations.ServiceFabric.Unity;
using SInnovations.ServiceFabric.RegistrationMiddleware.AspNetCore.Services;
using SInnovations.ServiceFabric.RegistrationMiddleware.AspNetCore.Extensions;
using SInnovations.Unity.AspNetCore;
using SInnovations.ServiceFabric.RegistrationMiddleware.AspNetCore;
//using ACMESharp.PKI;
using SInnovations.ServiceFabric.ResourceProvider;
using Unity.Injection;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.GatewayService
{




    public class Program
    {
        /// <summary>
        /// Event Handler delegate to log if an unhandled AppDomain exception occurs.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the exception details</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
          //  ServiceEventSource.Current.UnhandledException(ex.GetType().Name, ex.Message, ex.StackTrace);
        }

        /// <summary>
        /// Event Handler delegate to log if an unobserved task exception occurs.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the exception details</param>
        /// <remarks>
        /// We intentionally do not mark the exception as Observed, which would prevent the process from being terminated.
        /// We want the unobserved exception to take out the process. Note, as of .NET 4.5 this relies on the ThrowUnobservedTaskExceptions
        /// runtime configuration in the host App.Config settings.
        /// </remarks>
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
          //  ServiceEventSource.Current.UnobservedTaskException(e.Exception?.GetType().Name, e.Exception?.Message, e.Exception?.StackTrace);

            AggregateException flattened = e.Exception?.Flatten();
            foreach (Exception ex in flattened?.InnerExceptions)
            {
             //   ServiceEventSource.Current.UnobservedTaskException(ex.GetType().Name, ex.Message, ex.StackTrace);
            }

            // Marking as observed to prevent process exit.
           // e.SetObserved();
        }
        public static void Main(string[] args)
        {
            // var cp = CertificateProvider.GetProvider("BouncyCastle");

            // Setup unhandled exception handlers.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            using (var container = new FabricContainer())
            {
                container.AddOptions();
                container.ConfigureSerilogging(logConfiguration =>
                         logConfiguration.MinimumLevel.Debug()
                         .Enrich.FromLogContext()
                         .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                         .WriteTo.ApplicationInsightsTraces("10e77ea7-1d38-40f7-901c-ef3c2e7d48ef", Serilog.Events.LogEventLevel.Warning));



                container.ConfigureApplicationStorage();


                var keyvaultINfo = container.Resolve<KeyVaultSecretManager>();

                container.UseConfiguration(new ConfigurationBuilder()
                    .AddAzureKeyVault(keyvaultINfo.KeyVaultUrl, keyvaultINfo.Client, keyvaultINfo));



                container.Configure<KeyVaultOptions>("KeyVault");

                container.WithLetsEncryptService(new LetsEncryptServiceOptions
                {
                    BaseUri = Certes.Acme.WellKnownServers.LetsEncryptV2.AbsoluteUri// "https://acme-v01.api.letsencrypt.org"
                });

                container.WithStatelessService<NginxGatewayService>("GatewayServiceType");
                container.WithStatelessService<ApplicationStorageService>("ApplicationStorageServiceType");
                container.WithStatelessService<KeyVaultService>("KeyVaultServiceType");
                container.WithStatelessService<ResourceProviderService>("ResourceProviderServiceType");


                container.WithStatefullService<GatewayManagementService>("GatewayManagementServiceType");
                 

                Thread.Sleep(Timeout.Infinite);
            }


        }

         
    }
}
