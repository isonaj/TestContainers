using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace TestVault.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IConfiguration _config;
        public ValuesController(IConfiguration config)
        {
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { _config["test"], Environment.MachineName };
        }

        // GET api/values/5
        [HttpGet("{key}")]
        public ActionResult<string> Get(string key)
        {
            return _config[key];
        }


        [HttpGet("login/{clientId}")]
        public async Task<ActionResult> GetLogin(string clientId)
        {
            var subscriptionId = "bb26ef43-6cb5-4663-a6a2-a9367f210d8c";
            //var clientId = "ec771276-ed35-44ea-a30f-5ecc73c13d55";
            var managementUri = "https://management.core.windows.net/";

            var appId = clientId; //connectionDetails.ClientId;
            var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={appId}");
            Console.WriteLine("Created ServiceTokenProvider");
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(managementUri);
            Console.WriteLine("Got AccessToken");
            var tokenCredentials = new TokenCredentials(accessToken);
            var credentials = new AzureCredentials(tokenCredentials, tokenCredentials, null, AzureEnvironment.AzureGlobalCloud);
            Console.WriteLine("Got Credentials");
            //return credentials;

            return Ok();
        }

        [HttpGet("secret/{clientId}")]
        public async Task<ActionResult> GetSecret(string clientId)
        {
            var subscriptionId = "bb26ef43-6cb5-4663-a6a2-a9367f210d8c";
            //var clientId = "ec771276-ed35-44ea-a30f-5ecc73c13d55";
            var managementUri = "https://management.core.windows.net/";

            var appId = clientId; //connectionDetails.ClientId;
            var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={appId}");
            Console.WriteLine("Created ServiceTokenProvider");
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(managementUri);
            Console.WriteLine("Got AccessToken");
            var tokenCredentials = new TokenCredentials(accessToken);
            var credentials = new AzureCredentials(tokenCredentials, tokenCredentials, null, AzureEnvironment.AzureGlobalCloud);
            Console.WriteLine("Got Credentials");

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(subscriptionId);

            Console.WriteLine($"Searching for keyvault");
            var vaults = await azure.Vaults.ListAsync();
            Console.WriteLine($"Found {vaults.Count()} vaults: {JsonConvert.SerializeObject(vaults.Select(v => v.Name))}");
            var identityProviderKeyVault = (await azure.Vaults.ListAsync())
                .First(x => x.Name.StartsWith("idp-", StringComparison.OrdinalIgnoreCase));

            Console.WriteLine($"Found keyvault: {identityProviderKeyVault.Name}");

            //var keyVault = new CustomKeyVaultClient(identityProviderKeyVault, connectionDetails);
            //var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={appId}");

            Console.WriteLine("Creating client");
            var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            Console.WriteLine("fetching secret");
            var secret = await keyVault.GetSecretAsync(identityProviderKeyVault.VaultUri, "idp-connectivity");

            Console.WriteLine("got secret");

            return Ok();
        }

        // WORKS!
        [HttpGet("secret2/{clientId}")]
        public async Task<ActionResult> GetSecret2(string clientId)
        {
            //var clientId = "ec771276-ed35-44ea-a30f-5ecc73c13d55";
            var appId = clientId;
            var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={appId}");

            Console.WriteLine("Creating client");
            var keyVault = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            Console.WriteLine("fetching secret");
            var secret = await keyVault.GetSecretAsync("https://idp-test-aus-east.vault.azure.net/", "idp-connectivity");
            Console.WriteLine("got secret");

            return Ok();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{key}")]
        public void Put(string key, [FromBody] string value)
        {
            _config[key] = value;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
