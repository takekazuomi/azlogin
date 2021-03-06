﻿/* 
 * Copyright 2015-2017 Takekazu Omi
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace azlogin.console
{
    public class AzOAuth2
    {
        private const string OAuth2Uri = "https://login.microsoftonline.com/common/oauth2";
        private const string LoginUri = OAuth2Uri + "/devicecode?api-version=1.0";
        private const string TokenUri = OAuth2Uri + "/token";
        private const string ResourceUri = "https://management.core.windows.net/";

        // used by powershell cmdlet
        // https://github.com/Azure/azure-powershell/blob/master/src/Common/Commands.Common.Authentication/Authentication/AdalConfiguration.cs#L31
        // private const string ClientId =  "1950a258-227b-4e31-a9cf-717495945fc2";

        // used by az and xplat tools 
        private const string ClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

        // used by devcode sample code
        // https://github.com/Azure-Samples/active-directory-dotnet-deviceprofile/blob/master/DirSearcherClient/Program.cs#L18
        // private const string ClientId = "b78054de-7478-45a6-be1c-09f696a91d64";

        private const string MediaType = "application/x-www-form-urlencoded";

        private readonly HttpClient _client;

        private readonly JsonSerializerSettings _snakeSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
        private readonly JsonSerializerSettings _camelSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        public bool Verbose { get; set; }

        public AzOAuth2(HttpClient client)
        {
            _client = client;
        }

        public async Task<AuthResult> DeviceFlow()
        {
            var sc = ToStringContent(new { client_id = ClientId, resource = ResourceUri }, Encoding.UTF8, MediaType);

            var longin = await _client.PostAsync(new Uri(LoginUri), sc);
            var result = await longin.Content.ReadAsStringAsync();
            if ((int)longin.StatusCode != 200)
            {
                Console.Error.WriteLine("{0}\n{1}", longin.StatusCode, result);
                return new AuthResult();
            }

            if (Verbose)
                Console.Error.WriteLine(JsonFormatter(result));
            dynamic data = JsonConvert.DeserializeObject(result, _snakeSettings);

            Console.Error.WriteLine(data.message);

            var wait = TimeSpan.FromSeconds(int.Parse(data.interval.ToString()));
            for (;;)
            {
                var vc = ToStringContent(new
                {
                    grant_type = "device_code",
                    client_id = ClientId,
                    resource = ResourceUri,
                    code = data.device_code.ToString()
                }, Encoding.UTF8, MediaType);

                var token = await _client.PostAsync(new Uri(TokenUri), vc);
                if ((int)token.StatusCode == 200)
                {
                    var content = await token.Content.ReadAsStringAsync();
                    if (Verbose)
                        Console.Error.WriteLine(JsonFormatter(content));
                    return JsonConvert.DeserializeObject<AuthResult>(content, _snakeSettings);
                }
                await Task.Delay(wait);
                Console.Error.Write(".");
            }
        }

        public async Task<TR> GetValue<TR>(string target, string accessToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = await _client.GetAsync(target);
            if ((int)result.StatusCode == 200)
            {
                var content = await result.Content.ReadAsStringAsync();
                if (Verbose)
                    Console.Error.WriteLine(JsonFormatter(content));

                return JsonConvert.DeserializeObject<TR>(content, _camelSettings);
            }
            // TODO better error handling
            return default(TR);
        }


        public async Task<Tenants> GetTenants(string accessToken)
        {
            var tenantsUri = "https://management.azure.com/tenants?api-version=2016-06-01";
            return await GetValue<Tenants>(tenantsUri, accessToken);
        }

        public async Task<Subscriptions> GetSubscriptions(string accessToken)
        {
            var subscriptionUri = "https://management.azure.com/subscriptions?api-version=2016-06-01";
            return await GetValue<Subscriptions>(subscriptionUri, accessToken);
        }


        public async Task<AuthResult> LoginTenant(string tenantId, string refreshToken)
        {
            // POST https://login.microsoftonline.com/68dae846-2402-4566-b0b1-a1cea5fe3f3e/oauth2/token HTTP/1.1
            var logginUri = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            var lc = ToStringContent(new
            {
                grant_type = "refresh_token",
                client_id = ClientId,
                resource = ResourceUri,
                refresh_token = refreshToken
            }, Encoding.UTF8, MediaType);

            var token = await _client.PostAsync(new Uri(logginUri), lc);
            if ((int)token.StatusCode == 200)
            {
                var content = await token.Content.ReadAsStringAsync();
                if (Verbose)
                    Console.Error.WriteLine(JsonFormatter(content));
                return JsonConvert.DeserializeObject<AuthResult>(content, _snakeSettings);
            }
            // TODO better error handling
            return new AuthResult();
        }

        static StringContent ToStringContent(object data, Encoding encoding, string mediaType)
        {
            var s = data.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Select(info =>
                    string.Format("{0}={1}",
                        info.Name,
                        Uri.EscapeDataString((string)info.GetValue(data)).Replace("%20", "+")))
                .ToArray();

            return new StringContent(string.Join("&", s), encoding, mediaType);
        }

         string JsonFormatter(string data)
        {
            var obj = JsonConvert.DeserializeObject(data, _snakeSettings);
            return JsonConvert.SerializeObject(obj, _snakeSettings);
        }
    }
}
