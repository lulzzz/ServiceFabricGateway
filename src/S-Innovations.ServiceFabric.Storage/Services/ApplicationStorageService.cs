﻿using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using SInnovations.ServiceFabric.Storage.Configuration;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingClientVersion = RemotingClientVersion.V2_1, RemotingListenerVersion = RemotingListenerVersion.V2_1)]

namespace SInnovations.ServiceFabric.Storage.Services
{

    public interface IApplicationStorageService : IService
    {
        Task<string> GetApplicationStorageSharedAccessSignature();
        Task<string> GetApplicationStorageAccountNameAsync();
        Task<string> GetApplicationStorageCertificateThumbprint();
    }


   
    public class ApplicationStorageService : StatelessService, IApplicationStorageService
    {
        protected StorageConfiguration Storage { get; set; }
        public ApplicationStorageService(StatelessServiceContext serviceContext, StorageConfiguration storage) : base(serviceContext)
        {
            Storage = storage;
        }



        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
            // return new[] { new ServiceInstanceListener(Factory) };
           //   return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context),"RPC") };

            //return new[]
            //{
            //     new ServiceInstanceListener((c) =>
            //     {
            //         return new FabricTransportServiceRemotingListener(c, this);

            //     },"RPC")
            // };
        }

        //private ICommunicationListener Factory(StatelessServiceContext arg)
        //{
            
        //    return new FabricTransportServiceRemotingListener(arg, this, new FabricTransportListenerSettings
        //    {
        //         EndpointResourceName = "ServiceEndpoint",
        //         KeepAliveTimeout = 
        //    });
        //}

        public async Task<string> GetApplicationStorageSharedAccessSignature()
        {
            var a = await Storage.GetApplicationStorageAccountAsync();

            return a.GetSharedAccessSignature(new SharedAccessAccountPolicy
            {
                Permissions = SharedAccessAccountPermissions.Add | SharedAccessAccountPermissions.Create | SharedAccessAccountPermissions.Delete | SharedAccessAccountPermissions.List | SharedAccessAccountPermissions.ProcessMessages | SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.Update | SharedAccessAccountPermissions.Write,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object | SharedAccessAccountResourceTypes.Service,
                Services = SharedAccessAccountServices.Blob | SharedAccessAccountServices.File | SharedAccessAccountServices.Queue | SharedAccessAccountServices.Table,
        //        SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMonths(3)
            });
        }

        public async Task<string> GetApplicationStorageAccountNameAsync()
        {
            var a = await Storage.GetApplicationStorageAccountAsync();
            return a.Credentials.AccountName;
        }

        public Task<string> GetApplicationStorageCertificateThumbprint()
        {
            return Task.FromResult(
                this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings.Sections["AzureResourceManager"].Parameters["SecretsCertificateThumbprint"].Value);

        }
    }
}
