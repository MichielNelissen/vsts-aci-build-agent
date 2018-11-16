﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions
{
    public static class VSTSBuildAgent
    {
        private static IAzure _azure = GetAzure();

        [FunctionName("StartVSTSBuildAgent")]
        public static async Task<HttpResponseMessage> StartVSTSBuildAgenttAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var resourceGroupName = ConfigurationManager.AppSettings["ResourceGroupName"];
            var resourceGroup = await _azure.ResourceGroups.GetByNameAsync(resourceGroupName);
            var agentName = await GetNameAsync(req, "name");
            var env = new Dictionary<string, string>
            {
                { "VSTS_AGENT_INPUT_URL", ConfigurationManager.AppSettings["VSTS_AGENT_INPUT_URL"] },
                { "VSTS_AGENT_INPUT_AUTH", "pat" },
                { "VSTS_AGENT_INPUT_TOKEN", ConfigurationManager.AppSettings["VSTS_AGENT_INPUT_TOKEN"] },
                { "VSTS_AGENT_INPUT_POOL", ConfigurationManager.AppSettings["VSTS_AGENT_INPUT_POOL"] },
                { "VSTS_AGENT_INPUT_AGENT", agentName }
            };

            var containerGroup = await _azure.ContainerGroups.Define(agentName)
                .WithRegion(resourceGroup.RegionName)
                .WithExistingResourceGroup(resourceGroup)
                .WithLinux()
                .WithPrivateImageRegistry(ConfigurationManager.AppSettings["RegistryUrl"], ConfigurationManager.AppSettings["RegistryUser"], ConfigurationManager.AppSettings["RegistryPassword"])
                .WithoutVolume()
                .DefineContainerInstance(agentName)
                    .WithImage(ConfigurationManager.AppSettings["BuildServerImage"])
                    .WithoutPorts()
                    .WithEnvironmentVariables(env)
                    .Attach()
                .CreateAsync();

            return req.CreateResponse(HttpStatusCode.OK, "VSTS agent is running");
        }

        [FunctionName("StopVSTSBuildAgent")]
        public static async Task<HttpResponseMessage> StopVSTSBuildAgenttAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var agentName = await GetNameAsync(req, "name");
            await _azure.ContainerGroups.DeleteByResourceGroupAsync("vsts", agentName);
            return req.CreateResponse(HttpStatusCode.OK, "VSTS agent has been removed");
        }

        private static async Task<string> GetNameAsync(HttpRequestMessage req, string key)
        {
            // parse query parameter
            var name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Equals(q.Key, key, StringComparison.OrdinalIgnoreCase))
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            return name ?? data?.name;
        }

        private static IAzure GetAzure()
        {
            var tenantId = ConfigurationManager.AppSettings["tenantId"];
            var sp = new ServicePrincipalLoginInformation
            {
                ClientId = ConfigurationManager.AppSettings["clientId"],
                ClientSecret = ConfigurationManager.AppSettings["clientSecret"]
            };
            return Azure.Authenticate(new AzureCredentials(sp, tenantId, AzureEnvironment.AzureGlobalCloud)).WithDefaultSubscription();
        }
    }
}
