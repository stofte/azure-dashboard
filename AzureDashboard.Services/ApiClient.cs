using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AzureDashboard.Services.Helpers;
using AzureDashboard.Services.Models;
using AzureDashboard.Services.Models.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace AzureDashboard.Services
{
    public class ApiClient
    {
        public readonly string ArmBaseUrl = "https://management.azure.com/";
        public readonly string AuthBaseUrl = "https://login.microsoftonline.com/";

        public string TenantId { get; private set; }

        string _armBaseUrl = "https://management.azure.com/";
        string _isoFormatStr = "yyyy-MM-ddTHH:mm:ssZ";
        Uri _powershellReturnUrl = new Uri("urn:ietf:wg:oauth:2.0:oob");
        string _powershellApplicationId = "1950a258-227b-4e31-a9cf-717495945fc2";
        string _clientId = null;
        string _clientSecret = null;
        string _tenantId = null;
        IEnumerable<string> _supportedProviders = new string[] { };
        HttpClient client;

        IEnumerable<Subscription> _subscriptions;
        IEnumerable<Resource> _resources;
        Subscription _selectedSubscription;


        public ApiClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<TenantIdentifier>> AvailableTenants(AzureAccessToken token)
        {
            var msg = new HttpRequestMessage()
                .Get($"{ArmBaseUrl}/tenants?api-version=2016-06-01")
                .WithHeader("Authorization", $"Bearer {token.Value}");
            var response = await client.SendAsync(msg).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var json = body.Substring(9);
            json = json.Substring(0, json.Length - 1);
            var data = JsonConvert.DeserializeObject<IEnumerable<TenantIdentifier>>(json);
            return data;
        }

        public async Task<Tenant> Tenant(AzureAccessToken token)
        {
            var msg = new HttpRequestMessage()
                .Get($"https://graph.microsoft.com/v1.0/{token.TenantId}/organization/")
                .WithHeader("Authorization", $"Bearer {token.Value}");
            var response = await client.SendAsync(msg).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var marker = "\"value\":";
            var json = body.Substring(body.IndexOf(marker) + marker.Length);
            json = json.Substring(0, json.Length - 1);
            var tenants = JsonConvert.DeserializeObject<IEnumerable<Tenant>>(json);
            return tenants.Single(t => t.Id == token.TenantId);
        }

        
        
        public async Task<bool> Initialize()
        {
            // todo re-add service principal path
            var authString = "https://login.microsoftonline.com/" + _tenantId;
            //var authenticationContext = new AuthenticationContext(authString, false);
            //var clientCred = new ClientCredential(_clientId);
            
            bool success = false;
            try
            {
                var cache = new FileCache();
                var cached = cache.ReadItems().ToList();
                if (cached.Count() > 0)
                {
                    foreach(var c in cached)
                    {
                        var ac = new AuthenticationContext("https://login.microsoftonline.com/" + c.TenantId, cache);
                        var ar = await ac.AcquireTokenAsync(_armBaseUrl, _powershellApplicationId, _powershellReturnUrl, new PlatformParameters(PromptBehavior.Auto));
                    }
                }
                //var authenticationResult = await authenticationContext.AcquireTokenAsync(_armBaseUrl, clientCred).ConfigureAwait(false);
                client = new HttpClient
                {
                    BaseAddress = new Uri(_armBaseUrl)
                };
                
                {
                    
                    // https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-devhowto-multi-tenant-overview
                    // https://dev.office.com/blogs/microsoft-graph-or-azure-ad-graph

                    var commonCtx = new AuthenticationContext("https://login.microsoftonline.com/common", cache);
                    
                    var commonAuth = await commonCtx.AcquireTokenAsync(_armBaseUrl, _powershellApplicationId, _powershellReturnUrl, new PlatformParameters(PromptBehavior.Auto));
                    var ui = commonAuth.UserInfo;

                    commonCtx = new AuthenticationContext(commonAuth.Authority, cache);
                    
                    var x = await commonCtx.AcquireTokenAsync(_armBaseUrl, _powershellApplicationId, _powershellReturnUrl, new PlatformParameters(PromptBehavior.Never));

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {x.AccessToken}");

                    var response = await client.GetAsync("/tenants?api-version=2016-06-01").ConfigureAwait(false);
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = body.Substring(9);
                    json = json.Substring(0, json.Length - 1);
                    var data = JsonConvert.DeserializeObject<IEnumerable<TenantIdentifier>>(json);
                    foreach(var d in data)
                    {
                        var orgQuery = $"https://graph.microsoft.com/v1.0/{d.TenantId}/organization/";
                        var ac = new AuthenticationContext($"https://login.microsoftonline.com/{d.TenantId}", cache);
                        var ar = await ac.AcquireTokenAsync("https://graph.microsoft.com", _powershellApplicationId, _powershellReturnUrl, new PlatformParameters(PromptBehavior.Never));
                        var oclient = new HttpClient();
                        oclient.DefaultRequestHeaders.Clear();
                        oclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ar.AccessToken}");
                        var oresponse = await oclient.GetAsync(orgQuery).ConfigureAwait(false);
                        var obody = await oresponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    success = true;
                }
            }
            catch (AdalServiceException exn) 
                when (exn.Message.Contains("AADSTS70002") || // Error validating credentials.
                      exn.Message.Contains("AADSTS50012") || // Invalid client secret is provided.
                      exn.Message.Contains("User canceled authentication")) // cancelled
            { }
            
            return success;
        }

        public void SetSubscription(Subscription subscription)
        {
            _selectedSubscription = subscription;
        }

        public async Task<IEnumerable<Subscription>> Subscriptions()
        {
            var response = await client.GetAsync("subscriptions?api-version=2014-04-01").ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _subscriptions = Parse<IEnumerable<Subscription>>(body, simpleValue: true).OrderBy(x => x.DisplayName);
            return _subscriptions;
        }

        public async Task<IEnumerable<ResourceType>> ResourceTypes()
        {
            var resources = await Resources().ConfigureAwait(false);
            var supported = await SupportedResourceTypes(resources);
            var res = supported.Where(x => x.IsMetricsSupported).ToList();
            return res;
        }

        public async Task<IEnumerable<Resource>> Resources(ResourceType typeFilter = null)
        {
            if (_resources == null)
            {
                var url = $"/subscriptions/{_selectedSubscription.SubscriptionId}/resources?api-version=2017-05-10";
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _resources = Parse<IEnumerable<Resource>>(body, simpleValue: true).OrderBy(x => x.Name);
            }
            return typeFilter == null ? _resources : _resources.Where(x => x.Type == typeFilter.Id).ToList();
        }

        public async Task<IEnumerable<Metric>> Metrics(Resource resource)
        {
            var url = $"{resource.Id}/providers/microsoft.insights/metricDefinitions?api-version=2018-01-01";
            var response = await client.GetAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var metrics = Parse<IEnumerable<Metric>>(body, simpleValue: true).OrderBy(x => x.Name.LocalizedValue).ToList();
                return metrics;
            }
            else
            {
                return new Metric[] { };
            }
        }

        public async Task<IEnumerable<DimensionValue>> DimensionValues(Resource resource, Metric metric, string dimension, DateTime start, DateTime end)
        {
            
            var filter = $"{dimension} eq '*'";
            var url = $"{resource.Id}/providers/microsoft.insights/metrics?" +
                $"metricnames={metric.Name.Value}&timespan={start.ToString(_isoFormatStr)}/{end.ToString(_isoFormatStr)}" +
                $"&resultType=metadata&$filter={filter}&api-version=2018-01-01";
            var response = await client.GetAsync(url).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var dimVals = Parse<IEnumerable<MetricResult>>(body, metricResult: true).SelectMany(m => m.MetadataValues).ToList();
            await Task.Delay(3000);
            return dimVals;
        }

        public async Task<IEnumerable<MetricResult>> MetricValues(Resource resource, Metric metric, string interval, DateTime start, DateTime end, string dimension = null, string dimensionValue = null)
        {
            var filter = "";
            if (!string.IsNullOrWhiteSpace(dimension) && !string.IsNullOrWhiteSpace(dimensionValue))
            {
                filter = $"&$filter={dimension} eq '{dimensionValue}'";
            }
            var url = $"{resource.Id}/providers/microsoft.insights/metrics?" +
                $"metricnames={metric.Name.Value}&timespan={start.ToString(_isoFormatStr)}/{end.ToString(_isoFormatStr)}" +
                $"{filter}&interval={interval}&api-version=2018-01-01";
            var response = await client.GetAsync(url).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = Parse<IEnumerable<MetricResult>>(body, metricResult: true);
            return data;
        }

        async Task<IEnumerable<ResourceType>> SupportedResourceTypes(IEnumerable<Resource> resources)
        {
            var supported = new List<string>();
            var types = resources.GroupBy(r => r.Type).Select(x => new ResourceType { Name = x.Key, Id = x.Key }).OrderBy(x => x.Name).ToList();
            foreach (var type in types)
            {
                if (supported.Count > 0)
                {
                    type.IsMetricsSupported = supported.Any(x => x.Equals(type.Id, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    var res = resources.First(x => x.Type == type.Id);
                    var url = $"{res.Id}/providers/microsoft.insights/metricDefinitions?api-version=2018-01-01";
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    type.IsMetricsSupported = response.IsSuccessStatusCode;
                    if (supported.Count == 0 && !response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        // todo must be a better way?
                        var errStr = "is not a supported platform metric namespace, supported ones are ";
                        var idx = body.IndexOf(errStr) + errStr.Length;
                        var listStr = body.Substring(idx, body.Length - idx - 2);
                        supported.AddRange(listStr.Split(','));
                    }
                }
            }
            return types;
        }

        T Parse<T>(string response, bool simpleValue = false, bool metricResult = false)
        {
            if (simpleValue && metricResult)
            {
                throw new InvalidOperationException("mixed values");
            }
            string body = response;
            if (simpleValue)
            {
                body = response.Substring(9, response.Length - 10);
            }
            else if (metricResult)
            {
                var valuesStart = "\"timeseries\":";
                var idxStart = response.IndexOf(valuesStart) + valuesStart.Length;
                var idxEnd = response.LastIndexOf("]");
                var part = response.Substring(idxStart, idxEnd - idxStart + 1);
                body = part.Substring(0, part.Length - 2);
            }
            var json = JsonConvert.DeserializeObject<T>(body);
            return json;
        }
    }
}
