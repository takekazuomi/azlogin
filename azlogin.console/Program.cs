/* 
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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace azlogin.console
{
    /// <summary>
    /// device flow azure login console 
    /// </summary>
    class Program
    {
        private static bool Verbose { get; set; }

        private async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                var o = new AzOAuth2(client) {Verbose = Verbose};
                var aresult = await o.DeviceFlow();
                var tenants = await o.GetTenants(aresult.AccessToken);

                var sresults = new List<Tuple<Tenant, Subscription, AuthResult>>();
                foreach (var tenant in tenants.Value)
                {
                    var tenantToken = await o.LoginTenant(tenant.TenantId, aresult.RefreshToken);
                    var subscriptions = await o.GetSubscriptions(tenantToken.AccessToken);
                    var enabled = subscriptions.Value.Where(s => s.State.Equals("Enabled", StringComparison.InvariantCultureIgnoreCase)).Select(s=> Tuple.Create(tenant, s, tenantToken)).ToArray();
                    if (enabled.Length > 0)
                        sresults.AddRange(enabled);
                }
                var result = sresults.Select(arg => new { tenantId = arg.Item1.TenantId, subscriptionId = arg.Item2.SubscriptionId, subscriptionName = arg.Item2.DisplayName, bearer = arg.Item3.AccessToken }).ToArray();
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            }
        }

        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("-v", StringComparison.InvariantCulture))
                Verbose = true;

            try
            {
                var p = new Program();
                p.RunAsync().Wait();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
        }
    }
}
